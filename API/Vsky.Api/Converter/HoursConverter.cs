using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vsky.Api.Converter
{
    public class HoursConverter : JsonConverter<decimal>
    {
        public static decimal ConvertTimeToDecimalHours(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return 0;

            var parts = time.Split(':');
            return int.Parse(parts[0]) + (int.Parse(parts[1]) / 60m);
        }

      
        public static string ConvertDecimalHoursToTime(decimal value)
        {
            int hours = (int)Math.Floor(value);
            int minutes = (int)Math.Round((value - hours) * 60);

            if (minutes == 60)
            {
                hours++;
                minutes = 0;
            }

            return $"{hours:D2}:{minutes:D2}";
        }

        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return ConvertTimeToDecimalHours(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ConvertDecimalHoursToTime(value));
        }
    }
}
