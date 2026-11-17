using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Vsky.Api.ApiErrors;
using Vsky.Api.Extensions;

namespace Vsky.Api.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        [NonAction]
        public virtual ObjectResult InternalServerError(string error = null)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return new ObjectResult(new InternalServerError(error)) { StatusCode = StatusCodes.Status500InternalServerError };
            }
            else
            {
                return new ObjectResult(new InternalServerError()) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [NonAction]
        public virtual ObjectResult InternalServerError(IEnumerable<IdentityError> identityErrors)
        {
            var error = identityErrors.GetErrorMessages();

            return new ObjectResult(new InternalServerError(error)) { StatusCode = StatusCodes.Status500InternalServerError };
        }

        [NonAction]
        public virtual ObjectResult AmbiguousError(string error = null)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                return new ObjectResult(new AmbiguousError(error)) { StatusCode = StatusCodes.Status300MultipleChoices };
            }
            else
            {
                return new ObjectResult(new AmbiguousError()) { StatusCode = StatusCodes.Status300MultipleChoices };
            }
        }

        [NonAction]
        public virtual ObjectResult ModelStateError(ModelStateDictionary modelState)
        {
            var error = modelState.GetErrorMessages();

            return new ObjectResult(new BadRequestError(error)) { StatusCode = StatusCodes.Status400BadRequest };
        }

        // Messages the application raises for users on purpose. Anything else could expose internal
        // details such as SQL or file paths, so it's logged and the user gets a reference ID.
        private static readonly string[] UserFacingMessages =
        {
            "Email account not found",
            "Module not found",
            "Invalid release type",
            "A swimlane with the same name already exists.",
            "This file type can't be uploaded.",
            "SendGrid settings not configured."
        };

        [NonAction]
        protected string SafeErrorMessage(Exception ex)
        {
            if (ex.InnerException == null && System.Array.IndexOf(UserFacingMessages, ex.Message) >= 0)
                return ex.Message;

            var referenceId = HttpContext?.RequestServices.GetService<SecurityLog>()
                ?.WriteError(null, ex);

            return referenceId == null
                ? "An error occurred while processing your request."
                : $"An error occurred while processing your request. Reference id is {referenceId}";
        }

        // For catch blocks that deliberately carry on or return their own response, so the failure
        // still leaves a record. This never changes the response.
        [NonAction]
        protected void LogHandled(Exception ex, string description = null)
        {
            HttpContext?.RequestServices.GetService<SecurityLog>()
                ?.WriteError(description ?? $"{ControllerContext?.ActionDescriptor?.ActionName} failed", ex);
        }

        protected IActionResult AccessDenied()
        {
            return BadRequest(new BadRequestError("You do not have permission to perform the selected operation."));
        }
    }
}