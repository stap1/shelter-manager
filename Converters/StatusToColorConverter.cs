using System.Globalization;

namespace ShelterManager.Converters;

public class StatusToColorConverter : IValueConverter
{
    // Zmieniliśmy 'object' na 'object?' (z pytajnikiem), żeby kompilator był szczęśliwy
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            switch (status)
            {
                case "Do adopcji":
                    return Color.FromArgb("#4CAF50"); // Zielony
                case "Adoptowany":
                    return Color.FromArgb("#2196F3"); // Niebieski
                case "Kwarantanna":
                    return Color.FromArgb("#F44336"); // Czerwony
                case "W leczeniu":
                    return Color.FromArgb("#FF9800"); // Pomarańczowy
                default:
                    return Colors.Gray;
            }
        }
        return Colors.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Tutaj nic nie robimy, więc zwracamy null
        return null; 
    }
}