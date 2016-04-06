using System;
using Newtonsoft.Json;

namespace B2Lib.Utilities
{
    /// <summary>
    /// This converter reads Unix Epoch datetimes as a millisecond variant, and writes them back as ISO 8601 datetimes.
    /// </summary>
    public class UnixDateTimeConverter : JsonConverter
    {
        private static DateTimeOffset _unixEpoch = new DateTimeOffset(1970, 1, 1, 0, 1, 1, TimeSpan.Zero);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime)
            {
                DateTime dateTime = ((DateTime)value).ToUniversalTime();
                writer.WriteValue(dateTime.ToString("o"));
            }
            else if (value is DateTimeOffset)
            {
                DateTimeOffset dateTimeOffset = ((DateTimeOffset)value).ToUniversalTime();
                writer.WriteValue(dateTimeOffset.ToString("o"));
            }
            else
            {
                throw new JsonSerializationException();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Integer || reader.TokenType == JsonToken.Float)
            {
                long value;
                if (reader.TokenType == JsonToken.Integer)
                    value = (long)reader.Value;
                else
                    value = (long)(double)reader.Value;

                if (objectType == typeof(DateTime))
                    return _unixEpoch.AddMilliseconds(value).UtcDateTime;

                return _unixEpoch.AddMilliseconds(value);
            }

            if (reader.TokenType == JsonToken.Date)
            {
                DateTime value = (DateTime)reader.Value;
                if (objectType == typeof(DateTime))
                    return value;

                return new DateTimeOffset(value);
            }

            throw new JsonSerializationException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset);
        }
    }
}