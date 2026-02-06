using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Konwerter JSON dla gatunku.
///
/// Obsługuje:
/// - nowy format: nazwy enum (np. "Dog")
/// - legacy PL: "Pies", "Kot", "Inne"
/// - wartości liczbowe
/// </summary>
public sealed class AnimalSpeciesJsonConverter : JsonConverter<AnimalSpecies>
{
    public override void WriteJson(JsonWriter writer, AnimalSpecies value, JsonSerializer serializer)
    {
        // Zapis jako enum name (bez polskich znaków).
        writer.WriteValue(value.ToString());
    }

    public override AnimalSpecies ReadJson(
        JsonReader reader,
        Type objectType,
        AnimalSpecies existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return AnimalSpecies.Unknown;

        if (reader.TokenType == JsonToken.Integer)
        {
            try
            {
                var n = System.Convert.ToInt32(reader.Value);
                return Enum.IsDefined(typeof(AnimalSpecies), n) ? (AnimalSpecies)n : AnimalSpecies.Unknown;
            }
            catch
            {
                return AnimalSpecies.Unknown;
            }
        }

        if (reader.TokenType != JsonToken.String)
            return AnimalSpecies.Unknown;

        var raw = (reader.Value?.ToString() ?? string.Empty).Trim();
        if (raw.Length == 0)
            return AnimalSpecies.Unknown;

        var key = raw.ToLowerInvariant();
        if (key == "pies" || key == "dog") return AnimalSpecies.Dog;
        if (key == "kot" || key == "cat") return AnimalSpecies.Cat;
        if (key == "inne" || key == "other") return AnimalSpecies.Other;

        return Enum.TryParse<AnimalSpecies>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : AnimalSpecies.Unknown;
    }
}
