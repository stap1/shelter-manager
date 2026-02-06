using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Konwerter JSON dla AnimalEventType.
///
/// Dlaczego: domyślnie Newtonsoft zapisuje enum jako liczbę.
/// Chcemy zapis w formie string (czytelniejszy JSON) oraz tolerancję na warianty tekstowe.
/// </summary>
public sealed class AnimalEventTypeJsonConverter : JsonConverter<AnimalEventType>
{
    public override void WriteJson(JsonWriter writer, AnimalEventType value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override AnimalEventType ReadJson(JsonReader reader, Type objectType, AnimalEventType existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var raw = (reader.Value?.ToString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return AnimalEventType.Unknown;

            // Najpierw próbujemy standardowego parsowania enum.
            if (Enum.TryParse<AnimalEventType>(raw, ignoreCase: true, out var parsed))
                return parsed;

            // Warianty PL/legacy
            var key = raw.ToLowerInvariant();
            return key switch
            {
                "utworzenie" or "utworzono" => AnimalEventType.Created,
                "edycja" or "zmiana" or "zaktualizowano" => AnimalEventType.Edited,
                "archiwizacja" or "zarchiwizowano" => AnimalEventType.Archived,
                "przywrócenie" or "przywrocenie" or "przywrócono" or "przywrocono" => AnimalEventType.Restored,
                "zmiana statusu" => AnimalEventType.StatusChanged,
                "przydzielono boks" or "przydzielono do boksu" => AnimalEventType.CageAssigned,
                "przeniesiono boks" or "zmiana boksu" => AnimalEventType.CageMoved,
                "zdjęto z boksu" or "zdjeto z boksu" => AnimalEventType.CageRemoved,
                "adopcja" or "zatwierdzono adopcję" or "zatwierdzono adopcje" => AnimalEventType.AdoptionApproved,
                "wykonano zadanie" => AnimalEventType.CareTaskDone,
                _ => AnimalEventType.Unknown
            };
        }

        if (reader.TokenType == JsonToken.Integer)
        {
            // Obsługa, jeśli ktoś miał zapis numeryczny w JSON.
            try
            {
                var num = Convert.ToInt32(reader.Value);
                return Enum.IsDefined(typeof(AnimalEventType), num) ? (AnimalEventType)num : AnimalEventType.Unknown;
            }
            catch
            {
                return AnimalEventType.Unknown;
            }
        }

        return AnimalEventType.Unknown;
    }
}
