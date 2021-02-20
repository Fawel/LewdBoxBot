using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ImageDanbooruPuller.Converters
{
    /// <summary>
    /// Конвертирует строку в <see cref="DanbooruNSFWRating"/>
    /// </summary>
    public class StringRatingConverter : JsonConverter<DanbooruNSFWRating>
    {
        public override DanbooruNSFWRating ReadJson(
            JsonReader reader,
            Type objectType,
            [AllowNull] DanbooruNSFWRating existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            string valueString = (string)reader.Value;

            DanbooruNSFWRating enumNSFWRating = valueString switch
            {
                "s" or "safe" => DanbooruNSFWRating.Safe,
                "q" or "questionable" => DanbooruNSFWRating.Questionable,
                "e" or "explicit" => DanbooruNSFWRating.Explicit,
                _ => throw new NotSupportedException($"Неизвестный рейтинг {valueString}")
            };

            return enumNSFWRating;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] DanbooruNSFWRating value, JsonSerializer serializer)
        {
            string danbooruRatingStringValue = value switch
            {
                DanbooruNSFWRating.Safe => "s",
                DanbooruNSFWRating.Questionable => "q",
                DanbooruNSFWRating.Explicit => "e",
                _ => throw new NotSupportedException($"Неизвестный рейтинг {value}")
            };

            writer.WriteValue(danbooruRatingStringValue);
        }
    }
}
