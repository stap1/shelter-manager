using Microsoft.Extensions.DependencyInjection;

namespace ShelterManager.Infrastructure;

/// <summary>
/// Minimalny "Service Locator" wyłącznie po to, żeby strony tworzone przez XAML DataTemplate
/// (a więc bez wstrzykiwania w konstruktor) mogły pobrać zarejestrowane serwisy.
/// 
/// Docelowo warto przejść na pełne DI w nawigacji, ale tutaj celowo trzymamy zmianę małą.
/// </summary>
public static class ServiceLocator
{
    public static IServiceProvider? Services { get; set; }

    public static T GetRequiredService<T>() where T : notnull
    {
        if (Services is null)
            throw new InvalidOperationException("Brak skonfigurowanego kontenera DI. Sprawdź MauiProgram.");

        return Services.GetRequiredService<T>();
    }
}
