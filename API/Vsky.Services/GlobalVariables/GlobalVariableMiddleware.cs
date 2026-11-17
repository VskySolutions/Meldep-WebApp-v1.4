using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Vsky.Models;

namespace Vsky.Services.GlobalVariables
{
    public class GlobalVariableMiddleware
    {
        private readonly RequestDelegate _next;
        public GlobalVariableMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, GlobalVariable tenantContext)
        {
            // Trimmed, because a value pasted into Swagger or Postman often carries a stray space.
            tenantContext.SiteId = context.Request.Headers["X-Site-Id"].ToString().Trim();
            tenantContext.TimeZone = context.Request.Headers["X-Site-TimeZone"].ToString().Trim();

            await _next(context);
        }
    }
}
