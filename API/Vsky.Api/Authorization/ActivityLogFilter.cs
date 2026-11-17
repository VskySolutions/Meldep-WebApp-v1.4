using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vsky.Api.ApiErrors;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Collects what only an action filter can see: which records a request touched and why it failed.
    /// ActivityLoggingMiddleware writes the row. Only simple IDs are kept, never request bodies.
    /// </summary>
    public class ActivityLogFilter : IAsyncActionFilter
    {
        private const int MaxRecordIds = 20;
        private const int MaxResultLength = 500;
        private static readonly string[] SensitiveNameParts = { "token", "key", "secret", "password", "code" };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var state = RequestLogState.For(context.HttpContext);

            // Reflection can throw on unusual models, and collecting details must never fail a request.
            try
            {
                CollectRecordIds(context, state);
            }
            catch (Exception)
            {
                // The row is still written, without record IDs.
            }

            var executed = await next();

            try
            {
                CollectResult(context, executed, state);
            }
            catch (Exception)
            {
                // The row is still written, without the result details.
            }
        }

        private static void CollectRecordIds(ActionExecutingContext context, RequestLogState state)
        {
            foreach (var (name, value) in context.RouteData.Values)
            {
                if (IsRecordIdName(name) && AddRecordId(state, value))
                    state.HasRouteRecord = true;
            }

            foreach (var (name, value) in context.ActionArguments)
            {
                if (IsRecordIdName(name) && value is string or int or long or Guid)
                    AddRecordId(state, value);
            }

            foreach (var id in RecordIds.From(context, "Id", null))
                AddRecordId(state, id);
        }

        private static void CollectResult(ActionExecutingContext context, ActionExecutedContext executed, RequestLogState state)
        {
            if (executed.Result is not ObjectResult result)
                return;

            var status = result.StatusCode ?? StatusCodes.Status200OK;

            if (status >= StatusCodes.Status400BadRequest)
            {
                var message = result.Value as string ?? (result.Value as ApiError)?.Message;
                if (!string.IsNullOrEmpty(message))
                    state.ResultMessage = message.Length <= MaxResultLength ? message : message.Substring(0, MaxResultLength);
                return;
            }

            // The new record's ID, from a create action that returns it. Only this one property is read.
            if (status < StatusCodes.Status300MultipleChoices
                && !HttpMethods.IsGet(context.HttpContext.Request.Method)
                && result.Value != null
                && result.Value is not string
                && result.Value is not System.Collections.IEnumerable)
            {
                AddRecordId(state, result.Value.GetType().GetProperty("Id")?.GetValue(result.Value));
            }
        }

        private static bool IsRecordIdName(string name)
        {
            return (name.Equals("id", StringComparison.OrdinalIgnoreCase) || name.EndsWith("Id", StringComparison.Ordinal))
                   && !SensitiveNameParts.Any(part => name.Contains(part, StringComparison.OrdinalIgnoreCase));
        }

        private static bool AddRecordId(RequestLogState state, object value)
        {
            var id = value is string or int or long or Guid ? value.ToString() : null;

            if (!RequestLogState.IsSafeId(id) || state.RecordIds.Count >= MaxRecordIds)
                return false;

            if (!state.RecordIds.Contains(id))
                state.RecordIds.Add(id);

            return true;
        }
    }
}
