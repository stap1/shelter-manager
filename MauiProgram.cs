using CommunityToolkit.Maui; // Wymaga zainstalowanego pakietu CommunityToolkit.Maui
using Microsoft.Extensions.Logging;

namespace ShelterManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // To naprawia błędy inicjalizacji Toolkitu przy starcie
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                // Upewnij się, że te pliki istnieją w Resources/Fonts!
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        // Umożliwia podgląd błędów w konsoli podczas debugowania
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}