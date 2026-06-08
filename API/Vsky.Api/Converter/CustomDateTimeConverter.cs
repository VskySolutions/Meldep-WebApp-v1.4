using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Api.Helpers;
using Vsky.Models;

namespace Vsky.Api.Converter
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private TimeZoneInfo GetTimeZone()
        {
            var httpContext = HttpContextHelper.Accessor?.HttpContext;
            var globalVariable = httpContext?.RequestServices.GetService<GlobalVariable>();
            var timezone = string.IsNullOrEmpty(globalVariable.TimeZone) ? "India Standard Time" : globalVariable.TimeZone;

            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();

            if (string.IsNullOrEmpty(dateString))
                return default;

            DateTime parsedDate;

            if (DateTime.TryParseExact(
                dateString,
                "MM/dd/yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateOnly))
            {
                return dateOnly;
            }

            if (DateTime.TryParseExact(
                dateString,
                "MM/dd/yyyy hh:mm tt",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateTime))
            {
                parsedDate = dateTime;
            }
            else
            {
                parsedDate = DateTime.Parse(dateString, CultureInfo.InvariantCulture);
            }

            // Skip timezone conversion for date-only values
            if (parsedDate.TimeOfDay == TimeSpan.Zero)
                return parsedDate;

            // VERY IMPORTANT
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTime(parsedDate, GetTimeZone());
        }
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            DateTime finalValue = value;

            // Skip timezone conversion for date-only values
            if (value.TimeOfDay != TimeSpan.Zero)
            {
                // Ensure value is treated as UTC
                if (value.Kind == DateTimeKind.Unspecified)
                {
                    value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                }

                var timeZone = GetTimeZone();

                // Convert UTC -> Site Timezone
                finalValue = TimeZoneInfo.ConvertTimeFromUtc(value, timeZone);
            }

            writer.WriteStringValue(
                finalValue.TimeOfDay == TimeSpan.Zero
                    ? finalValue.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                    : finalValue.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture)
            );
        }
    }
}