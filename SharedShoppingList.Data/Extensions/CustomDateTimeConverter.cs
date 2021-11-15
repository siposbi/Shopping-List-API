using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SharedShoppingList.Data.Extensions
{
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private string _dateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString()!, _dateTimeFormat, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_dateTimeFormat, CultureInfo.InvariantCulture));
        }
    }
}