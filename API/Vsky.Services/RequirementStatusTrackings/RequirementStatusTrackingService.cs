using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vsky.Core;
using Vsky.Data;
using Vsky.Models;
using Vsky.Services.ApplicationUserRoles;

namespace Vsky.Services.RequirementStatusTrackings
{
    public class RequirementStatusTrackingService : IRequirementStatusTrackingService
    {
        #region Define Services
        private readonly IRepository<RequirementStatusTracking> _trackingRepository;
        private readonly IRepository<Requirement> _requirementRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<ProjectModule> _projectModuleRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApplicationUserRoleService _applicationUserRoleService;
        #endregion

        #region Services Initializations
        public RequirementStatusTrackingService(
            IRepository<RequirementStatusTracking> trackingRepository,
            IRepository<Requirement> requirementRepository,
            IRepository<Project> projectRepository,
            IRepository<ProjectModule> projectModuleRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IApplicationUserRoleService applicationUserRoleService
        )
        {
            _trackingRepository = trackingRepository;
            _requirementRepository = requirementRepository;
            _projectRepository = projectRepository;
            _projectModuleRepository = projectModuleRepository;
            _context = context;
            _userManager = userManager;
            _applicationUserRoleService = applicationUserRoleService;
        }
        #endregion

        #region Private Methods
        // Allowlisted record-level sort keys (client key -> RecordKey property).
        private static readonly Dictionary<string, string> SortColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            ["trackingDate"] = nameof(RecordKey.TrackingDate),
            ["project.name"] = nameof(RecordKey.ProjectName),
            ["projectName"] = nameof(RecordKey.ProjectName),
            ["projectModule.name"] = nameof(RecordKey.ProjectModuleName),
            ["projectModuleName"] = nameof(RecordKey.ProjectModuleName),
            ["createdOnUtc"] = nameof(RecordKey.CreatedOnUtc),
            ["updatedOnUtc"] = nameof(RecordKey.UpdatedOnUtc)
        };

        private class RecordKey
        {
            public string RecordId { get; set; }
            public string ProjectId { get; set; }
            public string ProjectName { get; set; }
            public string ProjectModuleId { get; set; }
            public string ProjectModuleName { get; set; }
            public DateTime TrackingDate { get; set; }
            public DateTime CreatedOnUtc { get; set; }
            public DateTime UpdatedOnUtc { get; set; }
            public string CreatedByName { get; set; }
            public string UpdatedByName { get; set; }

            public virtual ApplicationUser CreatedBy { get; set; }
            public virtual ApplicationUser UpdatedBy { get; set; }
        }

        private async Task<bool> IsCurrentUserAdmin(string userId, string siteId)
        {
            var userdata = await _userManager.FindByIdAsync(userId);
            var user = await _userManager.FindByNameAsync(userdata.UserName);
            var roles = await _applicationUserRoleService.GetRoleNamesByUserAndSite(user.Id, siteId);
            return roles.Contains("Admin") || roles.Contains("Site Super Admin") || roles.Contains("System Super Admin") || roles.Contains("Project Admin");
        }

        // Same project-role rule as the Requirement list: reads need FullAccess/ViewOnly/Notes, writes need FullAccess.
        private IQueryable<string> AccessibleProjectIds(string siteId, string userId, string employeeId, bool isAdmin, bool forWrite)
        {
            var query = _projectRepository.TableNoTracking.Where(p => p.SiteId == siteId && !p.Deleted && !p.IsArchived);

            if (!isAdmin)
            {
                query = forWrite
                    ? query.Where(p =>
                        p.CreatedById == userId ||
                        p.ProjectEmployeeMappings.Any(m =>
                            !m.Deleted &&
                            m.EmployeeId == employeeId &&
                            m.ProjectEmployeeRoleMappings.Any(r =>
                                !r.Deleted &&
                                r.SitesProjectRoles.SitesProjectRolesPermissions.Any(x => !x.Deleted && x.FullAccess))))
                    : query.Where(p =>
                        p.CreatedById == userId ||
                        p.ProjectEmployeeMappings.Any(m =>
                            !m.Deleted &&
                            m.EmployeeId == employeeId &&
                            m.ProjectEmployeeRoleMappings.Any(r =>
                                !r.Deleted &&
                                r.SitesProjectRoles.SitesProjectRolesPermissions.Any(x => !x.Deleted && (x.FullAccess || x.ViewOnly || x.Notes)))));
            }

            return query.Select(p => p.Id);
        }

        private IQueryable<RequirementStatusTracking> ReadableRows(string siteId, IQueryable<string> accessibleProjectIds)
        {
            return _trackingRepository.TableNoTracking.Where(x => !x.Deleted && x.Requirement.SiteId == siteId && accessibleProjectIds.Contains(x.ProjectId));
        }

        private static IQueryable<RequirementStatusTrackingRow> ProjectRows(IQueryable<RequirementStatusTracking> query)
        {
            return query.Select(x => new RequirementStatusTrackingRow
            {
                Id = x.Id,
                // Must match the key used in GroupRecords, or GetAllRecords cannot attach rows to their record.
                RecordId = x.Id,
                RequirementId = x.RequirementId,
                RequirementNumber = x.Requirement.RequirementNumber,
                RequirementTitle = x.Requirement.Title,
                IsRequirementUnavailable = x.Requirement.Deleted || x.Requirement.ProjectModuleId != x.ProjectModuleId,
                CurrentPercent = x.CurrentPercent,
                TimeSpentAsOfToday = x.TimeSpentAsOfToday,
                EstimatedRemainingTime = x.EstimatedRemainingTime,
                Scope = x.Scope,
                EstimatedTimeForCompletion = x.EstimatedTimeForCompletion,
                CreatedByName = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                CreatedOnUtc = x.CreatedOnUtc,
                UpdatedByName = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName,
                UpdatedOnUtc = x.UpdatedOnUtc
            });
        }

        private static IQueryable<RecordKey> GroupRecords(IQueryable<RequirementStatusTracking> query)
        {
            return query
                .GroupBy(x => new
                {
                    x.Id,
                    x.ProjectId,
                    ProjectName = x.Project.Name,
                    x.ProjectModuleId,
                    ProjectModuleName = x.ProjectModule.Name,
                    x.TrackingDate,
                    CreatedBy = x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    UpdatedBy = x.UpdatedBy.Person.FirstName + " " + x.UpdatedBy.Person.LastName
                })
                .Select(g => new RecordKey
                {
                    RecordId = g.Key.Id,
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.ProjectName,
                    ProjectModuleId = g.Key.ProjectModuleId,
                    ProjectModuleName = g.Key.ProjectModuleName,
                    TrackingDate = g.Key.TrackingDate,
                    CreatedOnUtc = g.Min(r => r.CreatedOnUtc),
                    UpdatedOnUtc = g.Max(r => r.UpdatedOnUtc ?? r.CreatedOnUtc),
                    CreatedByName = g.Key.CreatedBy,
                    UpdatedByName = g.Key.UpdatedBy
                });
        }

        private static RequirementStatusTrackingRecord ToRecord(RecordKey key, IEnumerable<RequirementStatusTrackingRow> rows)
        {
            return new RequirementStatusTrackingRecord
            {
                RecordId = key.RecordId,
                ProjectId = key.ProjectId,
                ProjectName = key.ProjectName,
                ProjectModuleId = key.ProjectModuleId,
                ProjectModuleName = key.ProjectModuleName,
                TrackingDate = key.TrackingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                CreatedOnUtc = key.CreatedOnUtc,
                UpdatedOnUtc = key.UpdatedOnUtc,
                CreatedByName = key.CreatedByName,
                UpdatedByName = key.UpdatedByName,
                Rows = rows.OrderBy(r => r.RequirementNumber).ThenBy(r => r.CreatedOnUtc).ThenBy(r => r.Id).ToList()
            };
        }

        // Builds an ordered, allowlisted Dynamic LINQ ordering; RecordId is the final tie-break so paging is stable.
        private static string BuildOrdering(RequirementStatusTrackingFilter filter, List<string> errors)
        {
            var parts = new List<string>();

            if (filter.Sorts != null && filter.Sorts.Any())
            {
                foreach (var sort in filter.Sorts)
                {
                    if (!SortColumns.TryGetValue(sort.Key ?? "", out var column))
                    {
                        errors.Add($"Sorting by '{sort.Key}' is not supported.");
                        continue;
                    }
                    var direction = (sort.Value ?? "").Trim().ToLowerInvariant();
                    if (direction != "asc" && direction != "desc")
                    {
                        errors.Add($"Sort direction for '{sort.Key}' must be 'asc' or 'desc'.");
                        continue;
                    }
                    parts.Add($"{column} {direction}");
                }
            }
            else if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                if (SortColumns.TryGetValue(filter.SortBy, out var column))
                    parts.Add($"{column} {(filter.Descending ? "desc" : "asc")}");
                else
                    errors.Add($"Sorting by '{filter.SortBy}' is not supported.");
            }

            if (!parts.Any())
                parts.Add($"{nameof(RecordKey.UpdatedOnUtc)} desc");

            parts.Add($"{nameof(RecordKey.RecordId)} asc");
            return string.Join(", ", parts);
        }
        #endregion

        #region GetAllRequirementStatusTrackingRecords
        public async Task<RequirementStatusTrackingResult> GetAllRecords(string siteId, string loggedUserId, string employeeId, RequirementStatusTrackingFilter filter)
        {
            var result = new RequirementStatusTrackingResult();
            filter ??= new RequirementStatusTrackingFilter();

            DateTime fromDate = default, toDate = default;
            var hasFrom = !string.IsNullOrWhiteSpace(filter.FromDate);
            var hasTo = !string.IsNullOrWhiteSpace(filter.ToDate);
            if (hasFrom && !RequirementStatusTrackingValidator.TryParseDate(filter.FromDate, out fromDate))
                result.Errors.Add("From Date must be a valid calendar date (yyyy-MM-dd).");
            if (hasTo && !RequirementStatusTrackingValidator.TryParseDate(filter.ToDate, out toDate))
                result.Errors.Add("To Date must be a valid calendar date (yyyy-MM-dd).");
            if (hasFrom && hasTo && result.Errors.Count == 0 && fromDate > toDate)
                result.Errors.Add("From Date cannot be after To Date.");

            var ordering = BuildOrdering(filter, result.Errors);
            if (result.Errors.Any())
                return result;

            var isAdmin = await IsCurrentUserAdmin(loggedUserId, siteId);
            var rows = ReadableRows(siteId, AccessibleProjectIds(siteId, loggedUserId, employeeId, isAdmin, false));

            // Filters select records that contain at least one matching row; the full record is returned.
            var matching = rows;
            if (filter.ProjectIds != null && filter.ProjectIds.Any())
                matching = matching.Where(x => filter.ProjectIds.Contains(x.ProjectId));
            if (filter.ProjectModuleIds != null && filter.ProjectModuleIds.Any())
                matching = matching.Where(x => filter.ProjectModuleIds.Contains(x.ProjectModuleId));
            if (filter.RequirementIds != null && filter.RequirementIds.Any())
                matching = matching.Where(x => filter.RequirementIds.Contains(x.RequirementId));
            if (hasFrom)
                matching = matching.Where(x => x.TrackingDate >= fromDate);
            if (hasTo)
                matching = matching.Where(x => x.TrackingDate <= toDate);
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.Trim().ToLower();
                matching = matching.Where(x =>
                    x.Requirement.Title.ToLower().Contains(search) ||
                    x.Project.Name.ToLower().Contains(search) ||
                    x.ProjectModule.Name.ToLower().Contains(search) ||
                    x.Scope.ToLower().Contains(search));
            }

            var matchingRecordIds = matching.Select(x => x.Id).Distinct();
            var recordQuery = GroupRecords(rows.Where(x => matchingRecordIds.Contains(x.Id))).OrderBy(ordering);

            // Paging is by complete record, then every row of the paged records is loaded.
            var paged = new PagedList<RecordKey>(recordQuery, Math.Max(filter.Page, 1), filter.PageSize);
            var pagedIds = paged.Select(k => k.RecordId).ToList();
            var pagedRows = await ProjectRows(rows.Where(x => pagedIds.Contains(x.Id))).ToListAsync();

            result.List = new RequirementStatusTrackingRecordList
            {
                Records = paged.Select(k => ToRecord(k, pagedRows.Where(r => r.RecordId == k.RecordId))).ToList(),
                Total = paged.TotalCount
            };
            return result;
        }
        #endregion

        #region GetRequirementStatusTrackingRecordById
        public async Task<RequirementStatusTrackingRecord> GetRecordById(string siteId, string loggedUserId, string employeeId, string recordId)
        {
            if (string.IsNullOrWhiteSpace(recordId))
                return null;

            var isAdmin = await IsCurrentUserAdmin(loggedUserId, siteId);
            var rows = ReadableRows(siteId, AccessibleProjectIds(siteId, loggedUserId, employeeId, isAdmin, false))
                .Where(x => x.Id == recordId);

            var key = await GroupRecords(rows).FirstOrDefaultAsync();
            if (key == null)
                return null;

            return ToRecord(key, await ProjectRows(rows).ToListAsync());
        }
        #endregion

        #region SaveRequirementStatusTrackingRecord
        public async Task<RequirementStatusTrackingResult> SaveRecord(string siteId, string loggedUserId, string employeeId, RequirementStatusTrackingSaveModel model, bool isCreate, DateTime now)
        {
            var result = new RequirementStatusTrackingResult();
            result.Errors.AddRange(RequirementStatusTrackingValidator.Validate(model, isCreate, out var trackingDate));
            if (result.Errors.Any())
                return result;

            var isAdmin = await IsCurrentUserAdmin(loggedUserId, siteId);
            var writableProjectIds = AccessibleProjectIds(siteId, loggedUserId, employeeId, isAdmin, true);

            var existing = new List<RequirementStatusTracking>();
            if (!isCreate)
            {
                // There is no shared record column, so the batch is the record row plus the row ids the form sends back
                // (rows created together by an earlier Save and Continue).
                var batchRowIds = model.Rows.Select(r => r.Id)
                    .Concat(model.RemovedRowIds ?? new List<string>())
                    .Append(model.RecordId)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct()
                    .ToList();

                existing = await _trackingRepository.TableNoTracking
                    .Where(x => !x.Deleted && x.Requirement.SiteId == siteId && batchRowIds.Contains(x.Id))
                    .ToListAsync();

                if (!existing.Any(x => string.Equals(x.Id, model.RecordId, StringComparison.OrdinalIgnoreCase)))
                {
                    result.NotFound = true;
                    return result;
                }

                foreach (var savedProjectId in existing.Select(x => x.ProjectId).Distinct())
                {
                    if (!await writableProjectIds.ContainsAsync(savedProjectId))
                    {
                        result.Errors.Add("You do not have permission to change this tracking record.");
                        return result;
                    }
                }
            }

            // Relationship checks run server-side so direct API calls cannot bypass the form.
            if (!await writableProjectIds.ContainsAsync(model.ProjectId))
            {
                result.Errors.Add("Project Name: the project was not found or you do not have permission to track it.");
                return result;
            }

            var moduleBelongsToProject = await _projectModuleRepository.TableNoTracking.AnyAsync(m =>
                m.Id == model.ProjectModuleId && m.ProjectId == model.ProjectId && m.SiteId == siteId && !m.Deleted);
            if (!moduleBelongsToProject)
            {
                result.Errors.Add("Project Module: the module does not belong to the selected project.");
                return result;
            }

            // Row identities: every saved row must come back either as a submitted row or a staged removal.
            var existingById = existing.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);
            var removedIds = (model.RemovedRowIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var submittedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var removedId in removedIds.Where(id => !existingById.ContainsKey(id)))
                result.Errors.Add($"Removed row '{removedId}' does not belong to this tracking record.");

            for (var i = 0; i < model.Rows.Count; i++)
            {
                var rowId = model.Rows[i].Id;
                if (string.IsNullOrWhiteSpace(rowId))
                    continue;

                if (!existingById.ContainsKey(rowId))
                    result.Errors.Add($"Row {i + 1}: row id '{rowId}' does not belong to this tracking record.");
                else if (removedIds.Contains(rowId, StringComparer.OrdinalIgnoreCase))
                    result.Errors.Add($"Row {i + 1}: the row is both submitted and staged for removal.");
                submittedIds.Add(rowId);
            }

            var unaccounted = existing.Where(x => !submittedIds.Contains(x.Id) && !removedIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase)).ToList();
            if (unaccounted.Any())
                result.Errors.Add("The tracking record has saved rows that were neither submitted nor removed; reload the record and save again.");

            if (result.Errors.Any())
                return result;

            var requirementIds = model.Rows.Select(r => r.RequirementId).Distinct().ToList();
            var requirements = await _requirementRepository.TableNoTracking
                .Where(r => requirementIds.Contains(r.Id) && r.SiteId == siteId)
                .Select(r => new { r.Id, r.ProjectId, r.ProjectModuleId, r.Deleted })
                .ToListAsync();

            for (var i = 0; i < model.Rows.Count; i++)
            {
                var row = model.Rows[i];

                // A saved row whose requirement later moved or was deleted may be kept unchanged in the same module.
                var keepsSavedRequirement = !string.IsNullOrWhiteSpace(row.Id)
                    && existingById.TryGetValue(row.Id, out var saved)
                    && string.Equals(saved.RequirementId, row.RequirementId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(saved.ProjectModuleId, model.ProjectModuleId, StringComparison.OrdinalIgnoreCase);
                if (keepsSavedRequirement)
                    continue;

                var requirement = requirements.FirstOrDefault(r => string.Equals(r.Id, row.RequirementId, StringComparison.OrdinalIgnoreCase));
                if (requirement == null || requirement.Deleted || requirement.ProjectId != model.ProjectId || requirement.ProjectModuleId != model.ProjectModuleId)
                    result.Errors.Add($"Row {i + 1}: Requirement does not belong to the selected project module.");
            }

            if (result.Errors.Any())
                return result;

            var recordId = isCreate ? Guid.NewGuid().ToString() : model.RecordId;
            var inserts = new List<RequirementStatusTracking>();
            var updates = new List<RequirementStatusTracking>();

            foreach (var row in model.Rows)
            {
                RequirementStatusTracking entity;
                if (string.IsNullOrWhiteSpace(row.Id))
                {
                    entity = new RequirementStatusTracking
                    {
                        // On create the first row carries the record id, so the saved record can be read back
                        // and the next Save updates it instead of adding it again.
                        Id = isCreate && inserts.Count == 0 ? recordId : Guid.NewGuid().ToString(),
                        CreatedById = loggedUserId,
                        CreatedOnUtc = now,
                        Deleted = false
                    };
                    inserts.Add(entity);
                }
                else
                {
                    entity = existingById[row.Id];
                    updates.Add(entity);
                }

                // Context is repeated on every row of the record, so it is rewritten on each kept row.
                entity.ProjectId = model.ProjectId;
                entity.ProjectModuleId = model.ProjectModuleId;
                entity.TrackingDate = trackingDate;
                entity.RequirementId = row.RequirementId;
                entity.CurrentPercent = row.CurrentPercent;
                entity.TimeSpentAsOfToday = row.TimeSpentAsOfToday;
                entity.EstimatedRemainingTime = row.EstimatedRemainingTime;
                entity.Scope = string.IsNullOrWhiteSpace(row.Scope) ? null : row.Scope;
                entity.EstimatedTimeForCompletion = row.EstimatedTimeForCompletion;
                entity.UpdatedById = loggedUserId;
                entity.UpdatedOnUtc = now;
            }

            foreach (var removedId in removedIds)
            {
                var entity = existingById[removedId];
                entity.Deleted = true;
                entity.UpdatedById = loggedUserId;
                entity.UpdatedOnUtc = now;
                updates.Add(entity);
            }

            // One transaction across the repository calls so a mid-batch failure leaves nothing committed.
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (inserts.Any())
                        _trackingRepository.Insert(inserts);
                    if (updates.Any())
                        _trackingRepository.Update(updates);

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            // Return every row kept by this save (not only the record row) so the form keeps all row ids.
            var keptIds = inserts.Concat(updates.Where(x => !x.Deleted)).Select(x => x.Id).Distinct().ToList();
            var keyRecordId = keptIds.Contains(recordId, StringComparer.OrdinalIgnoreCase) ? recordId : keptIds.FirstOrDefault();
            var readable = ReadableRows(siteId, AccessibleProjectIds(siteId, loggedUserId, employeeId, isAdmin, false));
            var key = await GroupRecords(readable.Where(x => x.Id == keyRecordId)).FirstOrDefaultAsync();
            if (key != null)
                result.Record = ToRecord(key, await ProjectRows(readable.Where(x => keptIds.Contains(x.Id))).ToListAsync());
            return result;
        }
        #endregion

        #region DeleteRequirementStatusTrackingRecords
        public async Task<RequirementStatusTrackingResult> DeleteRecords(string siteId, string loggedUserId, string employeeId, List<string> recordIds, DateTime now)
        {
            var result = new RequirementStatusTrackingResult();
            var ids = (recordIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!ids.Any())
            {
                result.Errors.Add("Select at least one tracking record.");
                return result;
            }

            var rows = await _trackingRepository.TableNoTracking
                .Where(x => !x.Deleted && x.Requirement.SiteId == siteId && ids.Contains(x.Id))
                .ToListAsync();

            var missing = ids.Where(id => !rows.Any(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase))).ToList();
            if (missing.Any())
            {
                result.NotFound = ids.Count == 1;
                if (!result.NotFound)
                    result.Errors.Add($"Tracking record(s) not found: {string.Join(", ", missing)}. Nothing was deleted.");
                return result;
            }

            var isAdmin = await IsCurrentUserAdmin(loggedUserId, siteId);
            var projectIds = rows.Select(r => r.ProjectId).Distinct().ToList();
            var writable = await AccessibleProjectIds(siteId, loggedUserId, employeeId, isAdmin, true)
                .Where(id => projectIds.Contains(id))
                .ToListAsync();
            if (projectIds.Any(id => !writable.Contains(id)))
            {
                result.Errors.Add("You do not have permission to delete one or more selected tracking records. Nothing was deleted.");
                return result;
            }

            foreach (var row in rows)
            {
                row.Deleted = true;
                row.UpdatedById = loggedUserId;
                row.UpdatedOnUtc = now;
            }

            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _trackingRepository.Update(rows);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return result;
        }
        #endregion
    }

    // Pure whole-batch validation shared by create and update; kept static so it can be tested without storage.
    public static class RequirementStatusTrackingValidator
    {
        // decimal(10,2) column limit for hour values.
        public const decimal MaxHours = 99999999.99m;

        public static bool TryParseDate(string value, out DateTime date)
        {
            return DateTime.TryParseExact(value?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        private static bool HasMoreThanTwoDecimals(decimal value) => decimal.Round(value, 2) != value;

        private static void ValidateHours(decimal? value, string label, int rowNumber, List<string> errors)
        {
            if (!value.HasValue)
                return;
            if (value.Value < 0 || value.Value > MaxHours)
                errors.Add($"Row {rowNumber}: {label} must be between 0 and {MaxHours.ToString(CultureInfo.InvariantCulture)}.");
            else if (HasMoreThanTwoDecimals(value.Value))
                errors.Add($"Row {rowNumber}: {label} allows at most 2 decimal places.");
        }

        public static List<string> Validate(RequirementStatusTrackingSaveModel model, bool isCreate, out DateTime trackingDate)
        {
            var errors = new List<string>();
            trackingDate = default;

            if (model == null)
            {
                errors.Add("Tracking details are required.");
                return errors;
            }

            if (isCreate && !string.IsNullOrWhiteSpace(model.RecordId))
                errors.Add("Record id must be empty when creating a tracking record; use bulk-save to update it.");
            if (!isCreate && string.IsNullOrWhiteSpace(model.RecordId))
                errors.Add("Record id is required to update a tracking record.");
            if (string.IsNullOrWhiteSpace(model.ProjectId))
                errors.Add("Project Name is required.");
            if (string.IsNullOrWhiteSpace(model.ProjectModuleId))
                errors.Add("Project Module is required.");
            if (string.IsNullOrWhiteSpace(model.TrackingDate))
                errors.Add("Date is required.");
            else if (!TryParseDate(model.TrackingDate, out trackingDate))
                errors.Add("Date must be a valid calendar date (yyyy-MM-dd).");

            var rows = model.Rows ?? new List<RequirementStatusTrackingRowSaveModel>();
            model.Rows = rows;
            if (!rows.Any())
            {
                errors.Add("At least one requirement row is required.");
                return errors;
            }

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowNumber = i + 1;

                if (row == null)
                {
                    errors.Add($"Row {rowNumber}: the row is empty.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.RequirementId))
                    errors.Add($"Row {rowNumber}: Requirement is required.");

                if (row.CurrentPercent.HasValue)
                {
                    if (row.CurrentPercent.Value < 0 || row.CurrentPercent.Value > 100)
                        errors.Add($"Row {rowNumber}: Completion % must be between 0 and 100.");
                    else if (HasMoreThanTwoDecimals(row.CurrentPercent.Value))
                        errors.Add($"Row {rowNumber}: Completion % allows at most 2 decimal places.");
                }

                ValidateHours(row.TimeSpentAsOfToday, "Hours Spent", rowNumber, errors);
                ValidateHours(row.EstimatedRemainingTime, "Estimated Remaining Hours", rowNumber, errors);
                ValidateHours(row.EstimatedTimeForCompletion, "Additional Scope Hours", rowNumber, errors);
            }

            if (rows.Any(r => r == null))
                return errors;

            foreach (var group in rows
                .Select((row, index) => new { row.RequirementId, RowNumber = index + 1 })
                .Where(x => !string.IsNullOrWhiteSpace(x.RequirementId))
                .GroupBy(x => x.RequirementId, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1))
            {
                errors.Add($"Rows {string.Join(", ", group.Select(x => x.RowNumber))}: the same requirement is selected more than once.");
            }

            foreach (var group in rows
                .Select((row, index) => new { row.Id, RowNumber = index + 1 })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1))
            {
                errors.Add($"Rows {string.Join(", ", group.Select(x => x.RowNumber))}: the same row id is submitted more than once.");
            }

            return errors;
        }
    }
}
