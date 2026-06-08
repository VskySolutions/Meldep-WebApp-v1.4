using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Vsky.Api.Helpers;
using Vsky.Models;

namespace Vsky.Api.Converter
{
    public class CustomTimeSpanConverter : JsonConverter<TimeSpan>
    {
        private TimeZoneInfo GetTimeZone()
        {
            var httpContext = HttpContextHelper.Accessor?.HttpContext;
            var globalVariable = httpContext?.RequestServices.GetService<GlobalVariable>();
            var timezone = string.IsNullOrEmpty(globalVariable.TimeZone) ? "India Standard Time" : globalVariable.TimeZone;

            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();

            if (string.IsNullOrWhiteSpace(value))
                return TimeSpan.Zero;

            // hh:mm tt
            if (DateTime.TryParseExact(
                value,
                "hh:mm tt",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateTime))
            {
                return dateTime.TimeOfDay;
            }

            // HH:mm:ss
            if (TimeSpan.TryParse(value, out var timeSpan))
            {
                return timeSpan;
            }

            return TimeSpan.Zero;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            // Create UTC datetime from TimeSpan
            var utcDateTime = DateTime.SpecifyKind(DateTime.Today.Add(value),DateTimeKind.Utc);

            // Convert UTC -> Site Timezone
            var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, GetTimeZone());

            writer.WriteStringValue(
                convertedDateTime.ToString("hh:mm tt", CultureInfo.InvariantCulture)
            );
        }
    }
}