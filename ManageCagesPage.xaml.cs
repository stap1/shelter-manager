using System.Collections.ObjectModel;
using System.Linq;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

/// <summary>
/// Ekran zarządzania boksami:
/// - dodawanie
/// - usuwanie (tylko pusty)
/// - edycja pojemności
/// 
/// Lista boksów jest przechowywana w pliku JSON (shelter_db.json) przez repozytorium.
/// </summary>
public partial class ManageCagesPage : ContentPage
{
    private readonly ICageRepository _cageRepository;

    /// <summary>
    /// Kolekcja z repozytorium, podpięta pod XAML.
    /// </summary>
    public ObservableCollection<Cage> Cages { get; }

    public ManageCagesPage()
    {
        InitializeComponent();

        _cageRepository = ServiceLocator.GetRequiredService<ICageRepository>();
        Cages = _cageRepository.Cages;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _cageRepository.Reload();
        SortCages();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnAddCageClicked(object sender, EventArgs e)
    {
        string? numberRaw = await DisplayPromptAsync(
            "Nowy boks",
            "Podaj numer boksu (liczba całkowita):",
            accept: "OK",
            cancel: "Anuluj",
            keyboard: Keyboard.Numeric);

        if (string.IsNullOrWhiteSpace(numberRaw))
            return;

        if (!int.TryParse(numberRaw.Trim(), out int number) || number < 1)
        {
            await DisplayAlert("Błąd", "Numer boksu musi być liczbą >= 1.", "OK");
            return;
        }

        if (Cages.Any(c => c.Numer == number))
        {
            await DisplayAlert("Błąd", $"Boks o numerze {number} już istnieje.", "OK");
            return;
        }

        string? capacityRaw = await DisplayPromptAsync(
            "Nowy boks",
            "Podaj pojemność (liczba >= 1):",
            accept: "OK",
            cancel: "Anuluj",
            keyboard: Keyboard.Numeric,
            initialValue: "1");

        if (string.IsNullOrWhiteSpace(capacityRaw))
            return;

        if (!int.TryParse(capacityRaw.Trim(), out int capacity) || capacity < 1)
        {
            await DisplayAlert("Błąd", "Pojemność musi być liczbą >= 1.", "OK");
            return;
        }

        var cage = new Cage
        {
            Numer = number,
            Capacity = capacity
        };

        _cageRepository.AddCage(cage);
        SortCages();
    }

    private async void OnEditCapacityClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Cage cage)
            return;

        string? capacityRaw = await DisplayPromptAsync(
            "Edycja pojemności",
            $"Boks {cage.Numer}: podaj nową pojemność (>=1):",
            accept: "OK",
            cancel: "Anuluj",
            keyboard: Keyboard.Numeric,
            initialValue: cage.Capacity.ToString());

        if (string.IsNullOrWhiteSpace(capacityRaw))
            return;

        if (!int.TryParse(capacityRaw.Trim(), out int capacity) || capacity < 1)
        {
            await DisplayAlert("Błąd", "Pojemność musi być liczbą >= 1.", "OK");
            return;
        }

        // Nie zmniejszamy pojemności poniżej zajętości.
        if (capacity < cage.OccupiedCount)
        {
            await DisplayAlert(
                "Błąd",
                $"Nie można ustawić pojemności {capacity}, ponieważ w boksie jest {cage.OccupiedCount} zwierząt.",
                "OK");
            return;
        }

        cage.Capacity = capacity;
        _cageRepository.SaveChanges();
    }

    private async void OnDeleteCageClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn || btn.CommandParameter is not Cage cage)
            return;

        if (cage.OccupiedCount > 0)
        {
            await DisplayAlert("Nie można usunąć", "Boks nie jest pusty. Najpierw usuń/przenieś zwierzęta.", "OK");
            return;
        }

        bool confirm = await DisplayAlert(
            "Usuń boks",
            $"Czy na pewno chcesz usunąć boks {cage.Numer}?",
            "Tak",
            "Nie");

        if (!confirm)
            return;

        bool removed = _cageRepository.TryRemoveCage(cage);
        if (!removed)
            await DisplayAlert("Nie można usunąć", "Boks nie jest pusty.", "OK");
    }

    private void SortCages()
    {
        // ObservableCollection nie ma wbudowanego sortowania.
        // Czyścimy i odtwarzamy kolejność (UI zostanie odświeżone).
        var sorted = Cages.OrderBy(c => c.Numer).ToList();
        if (sorted.Count != Cages.Count)
            return;

        bool alreadySorted = true;
        for (int i = 0; i < sorted.Count; i++)
        {
            if (!ReferenceEquals(sorted[i], Cages[i]))
            {
                alreadySorted = false;
                break;
            }
        }

        if (alreadySorted) return;

        Cages.Clear();
        foreach (var c in sorted)
            Cages.Add(c);
    }
}
