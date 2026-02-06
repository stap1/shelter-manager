using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using ShelterManager.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShelterManager;

public partial class MainPage : ContentPage
{
    // Główna kolekcja danych (baza w pamięci)
    public ObservableCollection<Zwierze> Zwierzeta { get; } = new();

    // Kolekcja dla UI (to, co widzi użytkownik - może być przefiltrowane)
    public ObservableCollection<Zwierze> FiltrowaneZwierzeta { get; set; } = new();

    private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "shelter_data.json");

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        WczytajDane();

        // Dane startowe (tylko przy pierwszym uruchomieniu aplikacji w ogóle)
        if (Zwierzeta.Count == 0)
        {
            Zwierzeta.Add(new Zwierze { Imie = "Burek", Rasa = "Owczarek", Status = "Kwarantanna", Zdjecie = "https://loremflickr.com/400/400/dog,owczarek?lock=1" });
            Zwierzeta.Add(new Zwierze { Imie = "Mruczek", Rasa = "Dachowiec", Status = "Kwarantanna", Zdjecie = "https://loremflickr.com/400/400/cat,dachowiec?lock=2" });
            Zwierzeta.Add(new Zwierze { Imie = "Reksio", Rasa = "Labrador", Status = "Do adopcji", Zdjecie = "https://loremflickr.com/400/400/dog,labrador?lock=3" });
            ZapiszDane();
            AktualizujWidok();
        }

        // --- OBSŁUGA KOMUNIKATÓW (MESSENGER) ---

        // 1. Gdy dodano nowego zwierzaka (z AddAnimalPage)
        WeakReferenceMessenger.Default.Register<Zwierze>(this, (r, m) =>
        {
            Dispatcher.Dispatch(() =>
            {
                Zwierzeta.Add(m);
                ZapiszDane();
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
                    ZapiszDane();
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
            LblInQuarantine.Text = Zwierzeta.Count(z => z.Status == "Kwarantanna").ToString();

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
                ZapiszDane();
                AktualizujWidok();
            }
        }
    }

    // Zapis do pliku JSON
    private void ZapiszDane()
    {
        try
        {
            var json = JsonConvert.SerializeObject(Zwierzeta);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd zapisu: {ex.Message}");
        }
    }

    // Odczyt z pliku JSON
    private void WczytajDane()
    {
        if (File.Exists(filePath))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var wczytane = JsonConvert.DeserializeObject<ObservableCollection<Zwierze>>(json);
                if (wczytane != null)
                {
                    Zwierzeta.Clear();
                    foreach (var z in wczytane) Zwierzeta.Add(z);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd wczytywania: {ex.Message}");
            }
        }
        AktualizujWidok();
    }
}