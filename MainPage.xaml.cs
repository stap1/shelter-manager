using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class MainPage : ContentPage
{
    private readonly IAnimalRepository _animalRepository;

    // Główna kolekcja danych (źródło repozytorium)
    public ObservableCollection<Zwierze> Zwierzeta { get; }

    // Kolekcja dla UI (to, co widzi użytkownik - może być przefiltrowane)
    public ObservableCollection<Zwierze> FiltrowaneZwierzeta { get; set; } = new();

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        _animalRepository = ServiceLocator.GetRequiredService<IAnimalRepository>();
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

        if (string.IsNullOrWhiteSpace(fraza))
        {
            FiltrowaneZwierzeta = new ObservableCollection<Zwierze>(Zwierzeta);
        }
        else
        {
            var wyniki = Zwierzeta.Where(z => z.Imie.ToLower().Contains(fraza)).ToList();
            FiltrowaneZwierzeta = new ObservableCollection<Zwierze>(wyniki);
        }
        OnPropertyChanged(nameof(FiltrowaneZwierzeta)); // Ważne: powiadamia UI o zmianie listy
    }

    // Główna metoda odświeżająca liczniki i listę
    private void AktualizujWidok()
    {
        // 1. Aktualizacja liczników (Dashboard)
        if (LblTotalAnimals != null)
            LblTotalAnimals.Text = Zwierzeta.Count.ToString();

        if (LblInQuarantine != null)
			LblInQuarantine.Text = Zwierzeta.Count(z => z.Status == AnimalStatus.Quarantine).ToString();

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
        if (LoginPage.RolaUzytkownika != "Admin")
        {
            await DisplayAlert("Brak uprawnień", "Tylko Administrator może usuwać zwierzęta z bazy danych!", "OK");
            return;
        }

        if (sender is MenuFlyoutItem menu && menu.CommandParameter is Zwierze zwierz)
        {
            bool potwierdzenie = await DisplayAlert("Potwierdzenie", $"Czy na pewno chcesz trwale usunąć: {zwierz.Imie}?", "Tak", "Nie");
            if (potwierdzenie)
            {
                Zwierzeta.Remove(zwierz);
                _animalRepository.SaveChanges();
                AktualizujWidok();
            }
        }
    }
}