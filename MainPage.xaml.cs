using System.Collections.ObjectModel;
using ShelterManager.Models;
using CommunityToolkit.Mvvm.Messaging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ShelterManager;

public partial class MainPage : ContentPage
{
    // Główna kolekcja danych
    public ObservableCollection<Zwierze> Zwierzeta { get; } = new();
    
    // Kolekcja dla UI obsługująca filtrowanie
    public ObservableCollection<Zwierze> FiltrowaneZwierzeta { get; set; } = new();

    private readonly string filePath = Path.Combine(FileSystem.AppDataDirectory, "shelter_data.json");

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;

        WczytajDane();

        // Dane startowe generowane tylko przy pierwszym uruchomieniu
        if (Zwierzeta.Count == 0)
        {
            Zwierzeta.Add(new Zwierze { Imie = "Burek", Rasa = "Owczarek", Status = "Kwarantanna", Zdjecie = "https://loremflickr.com/400/400/dog,owczarek?lock=1" });
            Zwierzeta.Add(new Zwierze { Imie = "Mruczek", Rasa = "Dachowiec", Status = "Kwarantanna", Zdjecie = "https://loremflickr.com/400/400/cat,dachowiec?lock=2" });
            ZapiszDane();
            AktualizujWidok();
        }

        // Rejestracja komunikatów dla dodawania nowych zwierząt
        WeakReferenceMessenger.Default.Register<Zwierze>(this, (r, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() => {
                Zwierzeta.Add(m);
                ZapiszDane();
                AktualizujWidok();
            });
        });

        // Rejestracja komunikatów dla zapisu po edycji
        WeakReferenceMessenger.Default.Register<string>(this, (r, m) =>
        {
            if (m == "ZapiszMnie")
            {
                ZapiszDane(); 
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    await Task.Delay(200); // Krótka pauza na stabilizację UI
                    try { AktualizujWidok(); }
                    catch (Exception ex) { Debug.WriteLine(ex.Message); }
                });
            }
        });
    }

    // Obsługa wyszukiwarki (Punkt 10 założeń - Filtrowanie)
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
        OnPropertyChanged(nameof(FiltrowaneZwierzeta)); // Odświeżenie zbindowanej listy
    }

    private void AktualizujWidok()
    {
        // Aktualizacja liczników na Dashboardzie
        if (LblTotalAnimals != null) 
            LblTotalAnimals.Text = Zwierzeta.Count.ToString();

        if (LblInQuarantine != null)
            LblInQuarantine.Text = Zwierzeta.Count(z => z.Status == "Kwarantanna").ToString();

        // Odświeżenie listy z uwzględnieniem aktualnego filtra
        string obecnyTekst = SearchBarZwierzeta?.Text ?? "";
        OnSearchTextChanged(this, new TextChangedEventArgs("", obecnyTekst));
    }

    private async void OnAddClicked(object sender, EventArgs e) 
        => await Navigation.PushAsync(new AddAnimalPage());

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Zwierze wybraneZwierze)
        {
            await Navigation.PushAsync(new EditAnimalPage(wybraneZwierze));
            if (sender is CollectionView cv) cv.SelectedItem = null;
        }
    }

    // ZABEZPIECZENIE ROLI (Punkt 8 założeń)
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        // Sprawdzenie, czy użytkownik ma rolę Admina zapisaną podczas logowania
        if (LoginPage.RolaUzytkownika != "Admin")
        {
            await DisplayAlert("Brak uprawnień", "Tylko Administrator może usuwać zwierzęta z bazy danych!", "OK");
            return;
        }

        // Logika usuwania dostępna tylko dla Admina
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

    private void ZapiszDane()
    {
        try {
            var json = JsonConvert.SerializeObject(Zwierzeta);
            File.WriteAllText(filePath, json);
        } catch (Exception ex) {
            Debug.WriteLine($"Błąd zapisu: {ex.Message}");
        }
    }

    private void WczytajDane()
    {
        if (File.Exists(filePath))
        {
            try {
                var json = File.ReadAllText(filePath);
                var wczytane = JsonConvert.DeserializeObject<ObservableCollection<Zwierze>>(json);
                if (wczytane != null)
                {
                    Zwierzeta.Clear();
                    foreach (var z in wczytane) Zwierzeta.Add(z);
                }
            } catch (Exception ex) {
                Debug.WriteLine($"Błąd wczytywania: {ex.Message}");
            }
        }
        AktualizujWidok();
    }
}