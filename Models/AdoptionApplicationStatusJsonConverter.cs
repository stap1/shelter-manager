using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Konwerter JSON dla AdoptionApplicationStatus.
/// Pozwala wczytać zarówno nazwy enumów ("Approved"), jak i typowe etykiety PL.
/// </summary>
public sealed class AdoptionApplicationStatusJsonConverter : JsonConverter<AdoptionApplicationStatus>
{
    public override void WriteJson(JsonWriter writer, AdoptionApplicationStatus value, JsonSerializer serializer)
        => writer.WriteValue(value.ToString());

    public override AdoptionApplicationStatus ReadJson(
        JsonReader reader,
        Type objectType,
        AdoptionApplicationStatus existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        var raw = (reader.Value ?? string.Empty).ToString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw))
            return AdoptionApplicationStatus.New;

        // Normalizacja pod wczytywanie etykiet PL.
        var key = raw.ToLowerInvariant();
        if (key is "nowy" or "new") return AdoptionApplicationStatus.New;
        if (key is "weryfikacja" or "inreview" or "in_review" or "in review") return AdoptionApplicationStatus.InReview;
        if (key is "zatwierdzony" or "approved") return AdoptionApplicationStatus.Approved;
        if (key is "odrzucony" or "rejected") return AdoptionApplicationStatus.Rejected;

        return Enum.TryParse<AdoptionApplicationStatus>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : AdoptionApplicationStatus.New;
    }
}
