using Microsoft.AspNetCore.Http;

namespace Vsky.Api.Helpers
{
    public static class HttpContextHelper
    {
        public static IHttpContextAccessor Accessor { get; set; }
    }
}