using System.Globalization;
using ShelterManager.Models;

namespace ShelterManager.Converters;

public class StatusToColorConverter : IValueConverter
{
    // Zmieniliśmy 'object' na 'object?' (z pytajnikiem), żeby kompilator był szczęśliwy
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Obsługujemy zarówno nowy typ (enum), jak i legacy string z istniejących plików JSON.
        var status = value switch
        {
            AnimalStatus st => st,
            string s => ParseLegacyStatus(s),
            _ => AnimalStatus.Unknown
        };

        return status switch
        {
            AnimalStatus.ForAdoption => Color.FromArgb("#4CAF50"), // Zielony
            AnimalStatus.Adopted => Color.FromArgb("#2196F3"),     // Niebieski
            AnimalStatus.Quarantine => Color.FromArgb("#F44336"),  // Czerwony
            AnimalStatus.Treatment => Color.FromArgb("#FF9800"),   // Pomarańczowy
            _ => Colors.Gray
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Tutaj nic nie robimy, więc zwracamy null
        return null; 
    }

    private static AnimalStatus ParseLegacyStatus(string raw)
    {
        string key = (raw ?? string.Empty).Trim().ToLowerInvariant();

        if (key == "kwarantanna") return AnimalStatus.Quarantine;
        if (key == "w leczeniu" || key == "leczenie") return AnimalStatus.Treatment;
        if (key == "do adopcji") return AnimalStatus.ForAdoption;
        if (key == "adoptowany") return AnimalStatus.Adopted;

        return Enum.TryParse<AnimalStatus>(raw, ignoreCase: true, out var parsed)
            ? parsed
            : AnimalStatus.Unknown;
    }
}