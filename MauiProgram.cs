using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui; // Wymaga zainstalowanego pakietu CommunityToolkit.Maui
using ShelterManager.Data;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Services;

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


        // Rejestracja warstwy dostępu do danych (jeden plik JSON: shelter_db.json)
        builder.Services.AddSingleton<ShelterDataStore>();
        builder.Services.AddSingleton<IAnimalRepository, AnimalFileRepository>();
        builder.Services.AddSingleton<ICageRepository, CageFileRepository>();
        builder.Services.AddSingleton<IResourceRepository, ResourceFileRepository>();
        builder.Services.AddSingleton<IInventoryTransactionRepository, InventoryTransactionFileRepository>();
        builder.Services.AddSingleton<ITaskRepository, TaskFileRepository>();

        // Serwisy domenowe
        builder.Services.AddSingleton<CageAllocationService>();

        var app = builder.Build();
        // Umożliwia pobieranie serwisów w stronach tworzonych przez DataTemplate.
        ServiceLocator.Services = app.Services;
        return app;
    }
}