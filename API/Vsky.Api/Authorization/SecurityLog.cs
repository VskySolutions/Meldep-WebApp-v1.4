using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.Authorization;
using LogLevel = Vsky.Models.LogLevel;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// The single writer for the existing Log table. It uses its own database context: the request's
    /// context may hold unsaved or failed changes, and saving a log row through it would save or retry
    /// those. Never stores request bodies, query strings or tokens. A logging failure never affects a
    /// request: if the database can't be reached, rows go to a fallback under the same ID.
    /// </summary>
    public class SecurityLog : ISecurityLog
    {
        private const string ReferenceKey = "Vsky.LogReferenceId";
        private const int MaxFullMessageLength = 64 * 1024;
        private const int MaxExceptionTextLength = 48 * 1024;
        private const int MaxShortMessageLength = 1000;
        private static readonly TimeSpan OutageSkip = TimeSpan.FromSeconds(30);

        // SQL Server errors caused by a row's data rather than by the database being unavailable.
        private const int ForeignKeyViolation = 547;
        private static readonly int[] RowDataErrors = { ForeignKeyViolation, 515, 2601, 2627, 2628, 8152 };

        // Events that fully describe their request. Audit-mode events aren't here, because the
        // request carries on and still needs its own activity row.
        private static readonly HashSet<string> EventsThatReplaceActivity = new()
        {
            "Login succeeded", "Login failed", "Login blocked: account locked", "Login denied: no site access",
            "Microsoft login rejected", "Password reset requested", "Password reset completed",
            "Logout", "Sessions revoked", "Site switched", "Permission change", "Authorization denied"
        };

        private static readonly TimeSpan MaskTimeout = TimeSpan.FromSeconds(1);
        private static readonly (Regex Pattern, string Replacement)[] SecretMasks =
        {
            (new Regex(@"\b(password|pwd|accountkey|sharedaccesssignature|sharedaccesskey|client_secret|secret)\s*=\s*[^;&\s""']+", RegexOptions.IgnoreCase | RegexOptions.Compiled, MaskTimeout), "$1=[removed]"),
            (new Regex(@"""(password|pwd|token|secret|apikey|servicekey)""\s*:\s*""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.Compiled, MaskTimeout), @"""$1"":""[removed]"""),
            (new Regex(@"\bBearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.Compiled, MaskTimeout), "Bearer [removed]"),
            (new Regex(@"\beyJ[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+\.[A-Za-z0-9_\-]+", RegexOptions.Compiled, MaskTimeout), "[removed]"),
            (new Regex(@"([?&](?:token|sig|servicekey|code)=)[^&\s""']+", RegexOptions.IgnoreCase | RegexOptions.Compiled, MaskTimeout), "$1[removed]")
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SecurityLog> _logger;
        private readonly string _fallbackPath;
        private readonly object _fallbackLock = new();
        private long _skipDatabaseUntilTicks;

        public SecurityLog(
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SecurityLog> logger,
            IOptions<SecurityAuthorizationOptions> options)
        {
            _scopeFactory = scopeFactory;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _fallbackPath = options.Value.LogFallbackPath;
        }

        /// <summary>Returns the row's ID to use as a reference, or null only if the row couldn't even be built.</summary>
        public string Write(LogLevel level, string shortMessage, string fullMessage = "", string userId = null)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext != null && shortMessage != null && EventsThatReplaceActivity.Contains(shortMessage))
                    RequestLogState.For(httpContext).EventRecorded = true;

                var details = string.IsNullOrEmpty(fullMessage) ? RequestContextJson(httpContext) : fullMessage;
                var row = CreateRow(level, shortMessage, details, userId ?? CurrentUserId(httpContext), httpContext);

                Save(new[] { row });
                return row.Id;
            }
            catch (Exception ex)
            {
                TryLogProblem(ex);
                return null;
            }
        }

        /// <summary>
        /// Records an exception once. If this exception, or one it wraps, was already recorded, this
        /// returns that row's ID and writes nothing. Returns null when the client cancelled the request.
        /// </summary>
        public string WriteError(string description, Exception exception, string userId = null)
        {
            try
            {
                if (exception == null)
                    return Write(LogLevel.Error, description ?? "Error", string.Empty, userId);

                var httpContext = _httpContextAccessor.HttpContext;
                var state = httpContext == null ? null : RequestLogState.For(httpContext);

                var existing = FindReference(exception);
                if (existing != null)
                {
                    if (state != null)
                        state.ReferenceId = existing;
                    return existing;
                }

                if (IsClientCancellation(exception, httpContext))
                    return null;

                var root = exception.GetBaseException();
                var action = httpContext == null ? null : RequestLogState.ActionOf(httpContext);

                var details = ToJson(new
                {
                    siteId = httpContext == null ? null : RequestLogState.SiteIdOf(httpContext),
                    requestId = httpContext?.TraceIdentifier,
                    method = httpContext?.Request.Method,
                    controller = action?.ControllerName,
                    action = action?.ActionName,
                    recordIds = state?.RecordIds.Count > 0 ? state.RecordIds : null,
                    exception = new
                    {
                        type = root.GetType().FullName,
                        message = Sanitize(root.Message, 2000),
                        details = Sanitize(exception.ToString(), MaxExceptionTextLength)
                    }
                });

                // Not ex.Message: EfRepository puts the whole exception text in its message.
                var row = CreateRow(LogLevel.Error, description ?? $"Error: {root.GetType().Name}", details, userId ?? CurrentUserId(httpContext), httpContext);

                exception.Data[ReferenceKey] = row.Id;
                if (state != null)
                    state.ReferenceId = row.Id;

                Save(new[] { row });
                return row.Id;
            }
            catch (Exception ex)
            {
                TryLogProblem(ex);
                return null;
            }
        }

        /// <summary>Saves rows prepared earlier, such as the background queue's. Never throws.</summary>
        public void WriteBatch(IReadOnlyList<Log> rows)
        {
            try
            {
                if (rows.Count > 0)
                    Save(rows);
            }
            catch (Exception ex)
            {
                TryLogProblem(ex);
            }
        }

        public Log CreateRow(LogLevel level, string shortMessage, string fullMessage, string userId, HttpContext httpContext)
        {
            return new Log
            {
                Id = Guid.NewGuid().ToString(),
                LogLevel = level,
                ShortMessage = Sanitize(string.IsNullOrEmpty(shortMessage) ? "Log" : shortMessage, MaxShortMessageLength),
                FullMessage = Sanitize(fullMessage, MaxFullMessageLength),
                UserId = userId,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                PageUrl = httpContext?.Request.Path.ToString(),
                CreatedOnUtc = DateTime.UtcNow
            };
        }

        /// <summary>Masks known secret patterns and limits the length.</summary>
        public static string Sanitize(string text, int maxLength = MaxFullMessageLength)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            try
            {
                foreach (var (pattern, replacement) in SecretMasks)
                    text = pattern.Replace(text, replacement);
            }
            catch (RegexMatchTimeoutException)
            {
                return "[removed: text could not be checked for secrets]";
            }

            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...[truncated]";
        }

        public static string ToJson(object value)
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        public static string CurrentUserId(HttpContext httpContext)
        {
            return httpContext?.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;
        }

        /// <summary>Saves rows that couldn't reach the database, such as those left at shutdown.</summary>
        public void WriteToFallback(IReadOnlyList<Log> rows)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_fallbackPath))
                {
                    var lines = rows.Select(row => ToJson(new
                    {
                        row.Id,
                        level = row.LogLevelId,
                        row.ShortMessage,
                        row.UserId,
                        row.IpAddress,
                        row.PageUrl,
                        row.CreatedOnUtc,
                        row.FullMessage
                    }));

                    lock (_fallbackLock)
                    {
                        Directory.CreateDirectory(_fallbackPath);
                        File.AppendAllLines(Path.Combine(_fallbackPath, $"meldep-log-{DateTime.UtcNow:yyyyMMdd}.jsonl"), lines);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                TryLogProblem(ex);
            }

            foreach (var row in rows)
            {
                try
                {
                    _logger.LogError("Log row {Id} could not reach the database. {ShortMessage} {FullMessage}", row.Id, row.ShortMessage, row.FullMessage);
                }
                catch (Exception)
                {
                    // Every option has failed. The request must still complete.
                }
            }
        }

        private void Save(IReadOnlyList<Log> rows)
        {
            if (DateTime.UtcNow.Ticks < Interlocked.Read(ref _skipDatabaseUntilTicks))
            {
                WriteToFallback(rows);
                return;
            }

            try
            {
                Insert(rows);
            }
            catch (Exception ex) when (IsRowDataError(ex))
            {
                // One bad row must not lose the others saved alongside it.
                foreach (var row in rows)
                    SaveOne(row);
            }
            catch (Exception ex)
            {
                // Skip the database for a while, so an outage doesn't add a timeout to every error.
                Interlocked.Exchange(ref _skipDatabaseUntilTicks, DateTime.UtcNow.Add(OutageSkip).Ticks);
                TryLogProblem(ex);
                WriteToFallback(rows);
            }
        }

        private void SaveOne(Log row)
        {
            try
            {
                Insert(new[] { row });
                return;
            }
            catch (Exception ex) when (HasSqlError(ex, ForeignKeyViolation) && row.UserId != null)
            {
                try
                {
                    Insert(new[] { WithoutUser(row) });
                    return;
                }
                catch (Exception retryException)
                {
                    TryLogProblem(retryException);
                }
            }
            catch (Exception ex)
            {
                TryLogProblem(ex);
            }

            WriteToFallback(new[] { row });
        }

        private void Insert(IReadOnlyList<Log> rows)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Set<Log>().AddRange(rows);
            db.SaveChanges();
        }

        private static Log WithoutUser(Log row)
        {
            // Keeps the ID in JSON rows. Plain values stay exact, because the audit screen compares them.
            var fullMessage = row.FullMessage != null && row.FullMessage.StartsWith('{')
                ? "{\"unlinkedUserId\":" + JsonSerializer.Serialize(row.UserId) + (row.FullMessage.Length > 2 ? "," : "") + row.FullMessage.Substring(1)
                : row.FullMessage;

            return new Log
            {
                Id = row.Id,
                LogLevelId = row.LogLevelId,
                ShortMessage = row.ShortMessage,
                FullMessage = fullMessage,
                IpAddress = row.IpAddress,
                PageUrl = row.PageUrl,
                CreatedOnUtc = row.CreatedOnUtc
            };
        }

        private static string RequestContextJson(HttpContext httpContext)
        {
            if (httpContext == null)
                return string.Empty;

            var action = RequestLogState.ActionOf(httpContext);
            return ToJson(new
            {
                siteId = RequestLogState.SiteIdOf(httpContext),
                requestId = httpContext.TraceIdentifier,
                method = httpContext.Request.Method,
                controller = action?.ControllerName,
                action = action?.ActionName
            });
        }

        private static string FindReference(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current.Data[ReferenceKey] is string reference)
                    return reference;
            }

            return null;
        }

        private static bool IsClientCancellation(Exception exception, HttpContext httpContext)
        {
            if (httpContext?.RequestAborted.IsCancellationRequested != true)
                return false;

            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is OperationCanceledException)
                    return true;
            }

            return false;
        }

        private static bool IsRowDataError(Exception exception)
        {
            return exception is DbUpdateException && exception.GetBaseException() is SqlException sql && RowDataErrors.Contains(sql.Number);
        }

        private static bool HasSqlError(Exception exception, int number)
        {
            return exception.GetBaseException() is SqlException sql && sql.Number == number;
        }

        private void TryLogProblem(Exception exception)
        {
            try
            {
                _logger.LogWarning("Could not write to the Log table: {Type}", exception.GetType().Name);
            }
            catch (Exception)
            {
                // Logging must never affect the request.
            }
        }
    }
}
