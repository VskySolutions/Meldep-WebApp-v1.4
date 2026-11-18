using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Api.Services.RequirementExport
{
    /// <summary>
    /// Read-only query roots used by the export. The EF implementation wraps ApplicationDbContext with
    /// AsNoTracking; tests supply in-memory lists so no database is touched.
    /// </summary>
    public interface IRequirementExportQueries
    {
        IQueryable<Requirement> Requirements { get; }
        IQueryable<ProjectTask> Tasks { get; }
        IQueryable<TestCase> TestCases { get; }
        IQueryable<Issue> Issues { get; }
        IQueryable<Timesheet> Timesheets { get; }
        IQueryable<TimesheetLines> TimesheetLines { get; }
        IQueryable<ProjectQuestionsAnswers> Questions { get; }
        IQueryable<ProjectActionItems> ActionItems { get; }
        IQueryable<RequirementDataMapping> Mappings { get; }
        IQueryable<Notes> Notes { get; }
    }

    public sealed class DbContextRequirementExportQueries : IRequirementExportQueries
    {
        private readonly ApplicationDbContext _db;

        public DbContextRequirementExportQueries(ApplicationDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public IQueryable<Requirement> Requirements => _db.Set<Requirement>().AsNoTracking();
        public IQueryable<ProjectTask> Tasks => _db.Set<ProjectTask>().AsNoTracking();
        public IQueryable<TestCase> TestCases => _db.Set<TestCase>().AsNoTracking();
        public IQueryable<Issue> Issues => _db.Set<Issue>().AsNoTracking();
        public IQueryable<Timesheet> Timesheets => _db.Set<Timesheet>().AsNoTracking();
        public IQueryable<TimesheetLines> TimesheetLines => _db.Set<TimesheetLines>().AsNoTracking();
        public IQueryable<ProjectQuestionsAnswers> Questions => _db.Set<ProjectQuestionsAnswers>().AsNoTracking();
        public IQueryable<ProjectActionItems> ActionItems => _db.Set<ProjectActionItems>().AsNoTracking();
        public IQueryable<RequirementDataMapping> Mappings => _db.Set<RequirementDataMapping>().AsNoTracking();
        public IQueryable<Notes> Notes => _db.Set<Notes>().AsNoTracking();
    }

    public interface IRequirementExportDataSource
    {
        Task<RequirementInfoRow> GetRequirementAsync(string siteId, string requirementId, CancellationToken ct = default);
        Task<List<RequirementTaskRow>> GetTasksAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementTestCaseRow>> GetTestCasesAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementIssueRow>> GetIssuesAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementTimesheetRow>> GetTimesheetsAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementQuestionRow>> GetQuestionsAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementActionItemRow>> GetActionItemsAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementMappingRow>> GetMappingsAsync(RequirementExportScope scope, CancellationToken ct = default);
        Task<List<RequirementNoteRow>> GetNotesAsync(RequirementExportScope scope, CancellationToken ct = default);
    }

    // Title: RequirementExportDataSource
    // Description: Complete (unpaged) requirement-scoped reads. Every query constrains the selected
    // requirement ID and, where the entity carries one, the active site. Deleted records are excluded,
    // matching the module lists. Timesheet lines are indirect: they are reached only through tasks that
    // belong to the selected requirement and timesheets in the active site.
    public sealed class RequirementExportDataSource : IRequirementExportDataSource
    {
        // Matches RequirementService's requirement-notes convention (Type "Requirement", SubModuleId = requirement).
        public const string RequirementNoteType = "Requirement";

        private readonly IRequirementExportQueries _q;

        public RequirementExportDataSource(IRequirementExportQueries queries)
        {
            _q = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<RequirementInfoRow> GetRequirementAsync(string siteId, string requirementId, CancellationToken ct = default)
        {
            var rows = await ToListAsync(_q.Requirements
                .Where(x => x.Id == requirementId && x.SiteId == siteId && !x.Deleted)
                .Select(x => new RequirementInfoRow
                {
                    Id = x.Id,
                    SiteId = x.SiteId,
                    ProjectId = x.ProjectId,
                    RequirementNumber = x.RequirementNumber,
                    Title = x.Title,
                    Project = x.Project == null ? null : x.Project.Name,
                    ProjectModule = x.ProjectModule == null ? null : x.ProjectModule.Name,
                    RequirementGroup = x.RequirementGroup == null ? null : x.RequirementGroup.Name,
                    RequirementType = x.RequirementType == null ? null : x.RequirementType.DropDownValue,
                    Status = x.Status == null ? null : x.Status.DropDownValue,
                    Priority = x.Priority == null ? null : x.Priority.DropDownValue,
                    Area = x.Area == null ? null : x.Area.DropDownValue,
                    Workspace = x.Workspace == null ? null : x.Workspace.DropDownValue,
                    IdentifiedUserType = x.UserType == null ? null : x.UserType.DropDownValue,
                    IdentifiedEmployee = x.Employee == null || x.Employee.Person == null ? null : x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    IdentifiedCustomer = x.Customer == null ? null : x.Customer.FirstName + " " + x.Customer.LastName,
                    IdentifiedDate = x.IdentifiedDate,
                    RequirementOwner = x.RequirementOwner == null || x.RequirementOwner.Person == null ? null : x.RequirementOwner.Person.FirstName + " " + x.RequirementOwner.Person.LastName,
                    CustomerOwner = x.CustomerOwner == null ? null : x.CustomerOwner.FirstName + " " + x.CustomerOwner.LastName,
                    EnteredBy = x.RequirementEntered == null || x.RequirementEntered.Person == null ? null : x.RequirementEntered.Person.FirstName + " " + x.RequirementEntered.Person.LastName,
                    ConfirmedBy = x.ConfirmedBy == null || x.ConfirmedBy.Person == null ? null : x.ConfirmedBy.Person.FirstName + " " + x.ConfirmedBy.Person.LastName,
                    ApprovedBy = x.ApprovedBy == null || x.ApprovedBy.Person == null ? null : x.ApprovedBy.Person.FirstName + " " + x.ApprovedBy.Person.LastName,
                    ApprovalStatus = x.ApprovalStatusDropDown == null ? x.ApprovalStatus : x.ApprovalStatusDropDown.DropDownValue,
                    PlannedStartDate = x.PlannedStartDate,
                    PlannedEndDate = x.PlannedEndDate,
                    ActualStartDate = x.ActualStartDate,
                    ActualEndDate = x.ActualEndDate,
                    CloseDate = x.CloseDate,
                    SourceLabel = x.SourceLabel,
                    TargetLabel = x.TargetLabel,
                    ShortDescription = x.ShortDescription,
                    Description = x.Description,
                    Scope = x.Scope,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc
                }), ct);

            return rows.FirstOrDefault();
        }

        public Task<List<RequirementTaskRow>> GetTasksAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            return ToListAsync(ScopedTasks(scope).Select(x => new RequirementTaskRow
            {
                Id = x.Id,
                TaskNumber = x.ProjectTaskNumber,
                Name = x.Name,
                ProjectModule = x.ProjectModule == null ? null : x.ProjectModule.Name,
                Type = x.Type == null ? null : x.Type.DropDownValue,
                Status = x.Status == null ? null : x.Status.DropDownValue,
                Priority = x.Priority == null ? null : x.Priority.DropDownValue,
                AssignedTo = x.AssignedTo == null || x.AssignedTo.Person == null ? null : x.AssignedTo.Person.FirstName + " " + x.AssignedTo.Person.LastName,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DueDate = x.DueDate,
                EstimateTime = x.EstimateTime,
                Description = x.Description,
                CreatedOnUtc = x.CreatedOnUtc
            }), ct);
        }

        public Task<List<RequirementTestCaseRow>> GetTestCasesAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            return ToListAsync(_q.TestCases
                .Where(x => x.RequirementId == scope.RequirementId && x.SiteId == scope.SiteId && !x.Deleted)
                .Select(x => new RequirementTestCaseRow
                {
                    Id = x.Id,
                    TestCaseNumber = x.TestCaseNumber,
                    Name = x.Name,
                    TestPlan = x.TestPlan == null ? null : x.TestPlan.Name,
                    ProjectModule = x.ProjectModule == null ? null : x.ProjectModule.Name,
                    Status = x.Status == null ? null : x.Status.DropDownValue,
                    AssignedTo = x.Employee == null || x.Employee.Person == null ? null : x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    TestedBy = x.TestedByEmployee == null || x.TestedByEmployee.Person == null ? null : x.TestedByEmployee.Person.FirstName + " " + x.TestedByEmployee.Person.LastName,
                    TestedDate = x.TestedDate,
                    TestResult = x.TestResult,
                    Description = x.Description,
                    Steps = x.Steps,
                    ExpectedResult = x.ExpectedResult,
                    ActualResult = x.ActualResult,
                    CreatedOnUtc = x.CreatedOnUtc
                }), ct);
        }

        public Task<List<RequirementIssueRow>> GetIssuesAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            return ToListAsync(_q.Issues
                .Where(x => x.RequirementId == scope.RequirementId && x.SiteId == scope.SiteId && !x.Deleted)
                .Select(x => new RequirementIssueRow
                {
                    Id = x.Id,
                    IssueNumber = x.IssueNumber,
                    Name = x.Name,
                    ProjectModule = x.ProjectModule == null ? null : x.ProjectModule.Name,
                    Type = x.Type == null ? null : x.Type.DropDownValue,
                    Status = x.Status == null ? null : x.Status.DropDownValue,
                    Priority = x.Priority == null ? null : x.Priority.DropDownValue,
                    AssignedTo = x.Employee == null || x.Employee.Person == null ? null : x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    ReportedBy = x.ReportedBy == null || x.ReportedBy.Person == null ? null : x.ReportedBy.Person.FirstName + " " + x.ReportedBy.Person.LastName,
                    DueDate = x.DueDate,
                    CloseDate = x.CloseDate,
                    Description = x.Description,
                    CreatedOnUtc = x.CreatedOnUtc
                }), ct);
        }

        public async Task<List<RequirementTimesheetRow>> GetTimesheetsAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            // Indirect association: verify the parent task belongs to the selected requirement first.
            var taskIds = await ToListAsync(ScopedTasks(scope).Select(x => x.Id), ct);
            if (taskIds.Count == 0)
                return new List<RequirementTimesheetRow>();

            var query =
                from line in _q.TimesheetLines
                join sheet in _q.Timesheets on line.TimesheetId equals sheet.Id
                where taskIds.Contains(line.ProjectTaskId)
                      && !line.Deleted
                      && !sheet.Deleted
                      && sheet.SiteId == scope.SiteId
                select new RequirementTimesheetRow
                {
                    Id = line.Id,
                    TimesheetDate = sheet.TimesheetDate,
                    Employee = sheet.Employee == null || sheet.Employee.Person == null ? null : sheet.Employee.Person.FirstName + " " + sheet.Employee.Person.LastName,
                    TaskNumber = line.Task == null ? 0 : line.Task.ProjectTaskNumber,
                    Task = line.Task == null ? null : line.Task.Name,
                    Activity = line.ProjectActivity == null ? null : line.ProjectActivity.Name,
                    Hours = line.Hours,
                    BillableHours = line.BillableHours,
                    IsApproved = line.IsApproved,
                    Description = line.Description,
                    CreatedOnUtc = line.CreatedOnUtc
                };

            return await ToListAsync(query, ct);
        }

        public Task<List<RequirementQuestionRow>> GetQuestionsAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            return ToListAsync(_q.Questions
                .Where(x => x.RequirementId == scope.RequirementId && x.SiteId == scope.SiteId && !x.Deleted)
                .Select(x => new RequirementQuestionRow
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    CreatedBy = x.CreatedBy == null || x.CreatedBy.Person == null ? null : x.CreatedBy.Person.FirstName + " " + x.CreatedBy.Person.LastName,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc
                }), ct);
        }

        public Task<List<RequirementActionItemRow>> GetActionItemsAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            return ToListAsync(_q.ActionItems
                .Where(x => x.RequirementId == scope.RequirementId && x.SiteId == scope.SiteId && !x.Deleted)
                .Select(x => new RequirementActionItemRow
                {
                    Id = x.Id,
                    Title = x.Title,
                    Priority = x.Priority == null ? null : x.Priority.DropDownValue,
                    Employee = x.Employee == null || x.Employee.Person == null ? null : x.Employee.Person.FirstName + " " + x.Employee.Person.LastName,
                    Customer = x.Customer == null ? null : x.Customer.Name,
                    DueDate = x.DueDate,
                    Description = x.Description,
                    CreatedOnUtc = x.CreatedOnUtc
                }), ct);
        }

        public Task<List<RequirementMappingRow>> GetMappingsAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            // RequirementDataMapping has no SiteId. The requirement itself was already resolved inside the
            // active site, so the requirement predicate (plus its project) is the isolation boundary.
            return ToListAsync(_q.Mappings
                .Where(x => x.RequirementId == scope.RequirementId && !x.Deleted
                            && (x.ProjectId == null || x.ProjectId == scope.ProjectId))
                .Select(x => new RequirementMappingRow
                {
                    Id = x.Id,
                    ProjectModule = x.ProjectModule == null ? null : x.ProjectModule.Name,
                    Source = x.Source,
                    Target = x.Target,
                    IntegrationDirection = x.IntegrationDirection == null ? null : x.IntegrationDirection.DropDownValue,
                    CreatedOnUtc = x.CreatedOnUtc,
                    UpdatedOnUtc = x.UpdatedOnUtc
                }), ct);
        }

        public Task<List<RequirementNoteRow>> GetNotesAsync(RequirementExportScope scope, CancellationToken ct = default)
        {
            return ToListAsync(_q.Notes
                .Where(x => x.Type == RequirementNoteType && x.SubModuleId == scope.RequirementId && x.SiteId == scope.SiteId && !x.Deleted)
                .Select(x => new RequirementNoteRow
                {
                    Id = x.Id,
                    NoteType = x.NoteType == null ? null : x.NoteType.DropDownValue,
                    Note = x.Note,
                    CreatedBy = x.User == null || x.User.Person == null ? null : x.User.Person.FirstName + " " + x.User.Person.LastName,
                    CreatedOnUtc = x.CreatedOnUtc
                }), ct);
        }

        private IQueryable<ProjectTask> ScopedTasks(RequirementExportScope scope)
        {
            return _q.Tasks.Where(x => x.RequirementId == scope.RequirementId && x.SiteId == scope.SiteId && !x.Deleted);
        }

        // EF queries run asynchronously; in-memory fixtures (no async provider) fall back to ToList.
        private static async Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken ct)
        {
            if (query.Provider is IAsyncQueryProvider)
                return await query.ToListAsync(ct);

            ct.ThrowIfCancellationRequested();
            return query.ToList();
        }
    }
}
