using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vsky.Api.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
        {
            // Set Cors:AllowedOrigins per environment. With no list, any origin stays allowed as
            // before, so deploying this can't block a client that isn't known yet.
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    if (allowedOrigins?.Length > 0)
                        builder.WithOrigins(allowedOrigins);
                    else
                        builder.AllowAnyOrigin();

                    builder.AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                // Reset links are now actually checked, so give people time to open their email.
                options.TokenLifespan = TimeSpan.FromHours(1);
            });
        }

        public static void ConfigureIISIntegration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {
            });
        }
    }
}