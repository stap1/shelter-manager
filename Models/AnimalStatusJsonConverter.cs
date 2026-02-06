using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Konwerter JSON dla statusu.
///
/// Obsługuje:
/// - nowy format: nazwy enum (np. "Quarantine")
/// - stary format: polskie etykiety (np. "Kwarantanna", "W leczeniu", "Do adopcji", "Adoptowany")
/// - wartości liczbowe (gdyby kiedyś pojawiły się w danych)
/// </summary>
public sealed class AnimalStatusJsonConverter : JsonConverter<AnimalStatus>
{
    public override void WriteJson(JsonWriter writer, AnimalStatus value, JsonSerializer serializer)
    {
        // Zapisujemy jako nazwę enuma (stabilne, bez polskich znaków).
        writer.WriteValue(value.ToString());
    }

    public override AnimalStatus ReadJson(
        JsonReader reader,
        Type objectType,
        AnimalStatus existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return AnimalStatus.Unknown;

        if (reader.TokenType == JsonToken.Integer)
        {
            try
            {
                var n = System.Convert.ToInt32(reader.Value);
                return Enum.IsDefined(typeof(AnimalStatus), n) ? (AnimalStatus)n : AnimalStatus.Unknown;
            }
            catch
            {
                return AnimalStatus.Unknown;
            }
        }

        if (reader.TokenType != JsonToken.String)
            return AnimalStatus.Unknown;

        var raw = (reader.Value?.ToString() ?? string.Empty).Trim();
        if (raw.Length == 0)
            return AnimalStatus.Unknown;

        // 1) Obsługa legacy PL
        var key = raw.ToLowerInvariant();
        if (key == "kwarantanna") return AnimalStatus.Quarantine;
        if (key == "w leczeniu" || key == "leczenie") return AnimalStatus.Treatment;
        if (key == "do adopcji") return AnimalStatus.ForAdoption;
        if (key == "adoptowany") return AnimalStatus.Adopted;

        // 2) Nowy format (Enum.ToString) lub inne warianty
        return Enum.TryParse<AnimalStatus>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : AnimalStatus.Unknown;
    }
}
