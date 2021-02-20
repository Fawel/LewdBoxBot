using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ImageDanbooruPuller.Converters
{
    /// <summary>
    /// Превращает строку со множеством значений разделеённых неким символом в массив строк при десериализации
    /// Превращает массив строк в одну строку со множеством значений при сериализации
    /// </summary>
    public class StringToStringArrayConverter : JsonConverter<string[]>
    {
        private readonly string _delimeter;

        public StringToStringArrayConverter(string delimeter)
        {
            _delimeter = delimeter ?? throw new ArgumentNullException(nameof(delimeter));
        }

        public override string[] ReadJson(
            JsonReader reader,
            Type objectType,
            [AllowNull] string[] existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            string valueString = (string)reader.Value;

            if (string.IsNullOrWhiteSpace(valueString))
            {
                return new string[0];
            }

            var result = valueString.Split(_delimeter);
            return result;
        }

        public override void WriteJson(
            JsonWriter writer,
            [AllowNull] string[] value,
            JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteValue(string.Empty);
                return;
            }

            var result = string.Join(_delimeter, value);
            writer.WriteValue(result);
        }
    }
}
