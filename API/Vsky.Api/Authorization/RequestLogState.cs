using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// What the logging code has learned about the current request. It lives in HttpContext.Items,
    /// so filters, the log writer and the activity middleware can share it.
    /// </summary>
    public sealed class RequestLogState
    {
        private const string ItemKey = "Vsky.RequestLogState";
        private static readonly Regex SafeId = new("^[A-Za-z0-9_.:-]{1,64}$", RegexOptions.Compiled);

        public List<string> RecordIds { get; } = new();

        /// <summary>The route itself names a record, as in GET /issues/{id}.</summary>
        public bool HasRouteRecord { get; set; }

        public string ResultMessage { get; set; }

        public string[] InvalidFields { get; set; }

        /// <summary>The ID of the error row written for this request, if there is one.</summary>
        public string ReferenceId { get; set; }

        /// <summary>A specific event already describes this request, so it needs no activity row.</summary>
        public bool EventRecorded { get; set; }

        public static RequestLogState For(HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(ItemKey, out var existing) && existing is RequestLogState state)
                return state;

            state = new RequestLogState();
            httpContext.Items[ItemKey] = state;
            return state;
        }

        /// <summary>The exception handler clears the endpoint, but keeps a copy on its feature.</summary>
        public static ControllerActionDescriptor ActionOf(HttpContext httpContext)
        {
            var endpoint = httpContext.GetEndpoint() ?? httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint;
            return endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        }

        /// <summary>
        /// Read from the header rather than GlobalVariable, which isn't set yet when the rate
        /// limiter rejects a request. The value is client-supplied, so it's context, not evidence.
        /// </summary>
        public static string SiteIdOf(HttpContext httpContext)
        {
            var siteId = httpContext.Request.Headers["X-Site-Id"].ToString();
            return IsSafeId(siteId) ? siteId : null;
        }

        public static bool IsSafeId(string value)
        {
            return !string.IsNullOrEmpty(value) && SafeId.IsMatch(value);
        }
    }
}
