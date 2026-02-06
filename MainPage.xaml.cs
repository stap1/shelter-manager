using CommunityToolkit.Mvvm.Messaging;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using ShelterManager.Services;
using System.Collections.ObjectModel;

namespace ShelterManager;

public partial class MainPage : ContentPage
{
    private readonly IAnimalRepository _animalRepository;
    private readonly CageAllocationService _cageAllocationService;

    // Główna kolekcja danych (źródło repozytorium)
    public ObservableCollection<Zwierze> Zwierzeta { get; }

    // Kolekcja dla UI (to, co widzi użytkownik - może być przefiltrowane)
    public ObservableCollection<Zwierze> FiltrowaneZwierzeta { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        _animalRepository = ServiceLocator.GetRequiredService<IAnimalRepository>();
        _cageAllocationService = ServiceLocator.GetRequiredService<CageAllocationService>();
        Zwierzeta = _animalRepository.Animals;
        AktualizujWidok();

        // --- OBSŁUGA KOMUNIKATÓW (MESSENGER) ---

        // 1. Gdy dodano nowego zwierzaka (z AddAnimalPage)
        WeakReferenceMessenger.Default.Register<Zwierze>(this, (r, m) =>
        {
            Dispatcher.Dispatch(() =>
            {
                Zwierzeta.Add(m);
                _animalRepository.SaveChanges();
                AktualizujWidok();
            });
        });

        // 2. Gdy edytowano zwierzaka i trzeba odświeżyć listę
        WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
        {
            if (m == "ZapiszMnie")
            {
                // Dispatcher zapewnia bezpieczne odświeżenie UI bez "Task.Delay"
                Dispatcher.Dispatch(() =>
                {
                    _animalRepository.SaveChanges();
                    AktualizujWidok();
                });
            }
        });
    }

    // Obsługa wyszukiwarki
    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string fraza = e.NewTextValue?.ToLower() ?? "";

        // Na ekranie głównym pokazujemy tylko aktywne zwierzęta (niezarchiwizowane).
        var aktywne = Zwierzeta.Where(z => !z.IsArchived).ToList();

        if (string.IsNullOrWhiteSpace(fraza))
        {
            FiltrowaneZwierzeta = new ObservableCollection<Zwierze>(aktywne);
        }
        else
        {
            var wyniki = aktywne.Where(z => z.Imie.ToLower().Contains(fraza)).ToList();
            FiltrowaneZwierzeta = new ObservableCollection<Zwierze>(wyniki);
        }
        OnPropertyChanged(nameof(FiltrowaneZwierzeta)); // Ważne: powiadamia UI o zmianie listy
    }

    // Główna metoda odświeżająca liczniki i listę
    private void AktualizujWidok()
    {
        var aktywne = Zwierzeta.Where(z => !z.IsArchived).ToList();

        // 1. Aktualizacja liczników (Dashboard)
        if (LblTotalAnimals != null)
            LblTotalAnimals.Text = aktywne.Count.ToString();

        if (LblInQuarantine != null)
            LblInQuarantine.Text = aktywne.Count(z => z.Status == AnimalStatus.Quarantine).ToString();

        // 2. Odświeżenie listy (z uwzględnieniem wpisanego tekstu w szukajce)
        string obecnyTekst = SearchBarZwierzeta?.Text ?? "";
        OnSearchTextChanged(this, new TextChangedEventArgs("", obecnyTekst));
    }

    // Nawigacja do dodawania
    private async void OnAddClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new AddAnimalPage());

    // Nawigacja do edycji (kliknięcie w kartę)
    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Zwierze wybraneZwierze)
        {
            await Navigation.PushAsync(new EditAnimalPage(wybraneZwierze));
            if (sender is CollectionView cv) cv.SelectedItem = null; // Odznaczamy element
        }
    }

    // Usuwanie (Tylko dla Admina)
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        // Decyzja projektowa: archiwizacja (soft delete) dostępna dla Administratora i Pracownika.
        // Trwałe usunięcie jest dostępne tylko w zakładce Archiwum i tylko dla Administratora.
        if (LoginPage.RolaUzytkownika is not ("Admin" or "Pracownik"))
        {
            await DisplayAlert("Brak uprawnień", "Musisz być zalogowany jako Administrator lub Pracownik.", "OK");
            return;
        }

        if (sender is MenuFlyoutItem menu && menu.CommandParameter is Zwierze zwierz)
        {
            bool potwierdzenie = await DisplayAlert(
                "Archiwum",
                $"Czy chcesz przenieść do archiwum: {zwierz.Imie}?\n\nZwierzę będzie ukryte na liście głównej, ale nadal będzie dostępne w zakładce 'Archiwum'.",
                "Archiwizuj",
                "Anuluj");

            if (!potwierdzenie)
                return;

            // Utrzymujemy spójność danych: zwierzę zarchiwizowane nie może zajmować boksu.
            _cageAllocationService.RemoveAnimalFromCage(zwierz.Id);

            zwierz.IsArchived = true;
            zwierz.ArchivedAt = DateTime.UtcNow;

            _animalRepository.SaveChanges();
            AktualizujWidok();
        }
    }
}