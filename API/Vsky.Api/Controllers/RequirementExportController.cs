using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;
using Vsky.Api.Services.RequirementExport;
using Vsky.Data;
using Vsky.Models;

namespace Vsky.Api.Controllers
{
    // Title: RequirementExportController (WO-73)
    // Description: GET requirement/{id}/export returns one .xlsx for the selected requirement. Added as a
    // separate controller so RequirementController and Program.cs stay untouched. The export services are
    // composed here from services that are already registered.
    //
    // The endpoint is OFF until "Features:RequirementExport:Enabled" is set to true (default false): the
    // field mappings still need product-owner confirmation. While off it returns 404 before any other filter.
    [Route("requirement")]
    [RequireMenu(MenuLinks.Requirement)]
    public class RequirementExportController : BaseController
    {
        public const string FeatureFlagKey = "Features:RequirementExport:Enabled";

        private readonly GlobalVariable _globalVariable;
        private readonly IRequirementExportService _exportService;

        [ActivatorUtilitiesConstructor]
        public RequirementExportController(
            GlobalVariable globalVariable,
            ApplicationDbContext db,
            AdminAccess adminAccess,
            IPermissionService permissionService,
            ProjectRecordAccess projectRecordAccess)
            : this(globalVariable, new RequirementExportService(
                new RequirementExportDataSource(new DbContextRequirementExportQueries(db)),
                new RequirementExportAuthorizer(adminAccess, permissionService, projectRecordAccess),
                new RequirementWorkbookWriter()))
        {
        }

        // For tests, and for a later DI registration of IRequirementExportService.
        public RequirementExportController(GlobalVariable globalVariable, IRequirementExportService exportService)
        {
            _globalVariable = globalVariable;
            _exportService = exportService;
        }

        [HttpGet("{id}/export")]
        [RequirementExportFeatureGate]
        [SiteOwned(typeof(Requirement))]
        [RecordScope(typeof(Requirement))]
        [LogActivity]
        public async Task<IActionResult> ExportRequirement(string id, CancellationToken ct)
        {
            try
            {
                var userId = User.GetLoggedInUserId<string>();
                var file = await _exportService.ExportAsync(_globalVariable.SiteId, userId, id, ct);

                Response.Headers["Cache-Control"] = "private, no-store";
                Response.Headers["Pragma"] = "no-cache";
                return File(file.Content, RequirementExportFile.ContentType, file.FileName);
            }
            catch (RequirementExportException ex) when (ex.Failure == RequirementExportFailure.NotFound)
            {
                return NotFound(new NotFoundError(ex.Message));
            }
            catch (RequirementExportException ex) when (ex.Failure == RequirementExportFailure.Forbidden)
            {
                return new ObjectResult(new ApiError(StatusCodes.Status403Forbidden, ex.Message)) { StatusCode = StatusCodes.Status403Forbidden };
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return new EmptyResult();
            }
            catch (Exception ex)
            {
                return InternalServerError(SafeErrorMessage(ex));
            }
        }
    }

    /// <summary>Short-circuits with 404 while the export feature flag is off. Runs before the record filters.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RequirementExportFeatureGateAttribute : Attribute, IAsyncActionFilter, IOrderedFilter
    {
        public int Order => int.MinValue;

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
            if (!IsEnabled(configuration))
            {
                context.Result = new NotFoundObjectResult(new NotFoundError());
                return Task.CompletedTask;
            }

            return next();
        }

        public static bool IsEnabled(IConfiguration configuration) =>
            configuration != null && configuration.GetValue(RequirementExportController.FeatureFlagKey, false);
    }
}
