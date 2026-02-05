using CommunityToolkit.Mvvm.Messaging;
using ShelterManager.Models;

namespace ShelterManager;

public partial class AddAnimalPage : ContentPage
{
    public AddAnimalPage()
    {
        InitializeComponent();
        PickerGatunek.SelectedIndex = 0;
        PickerStatus.SelectedIndex = 0;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // 1. Walidacja (Imię jest wymagane)
        if (string.IsNullOrWhiteSpace(EntryImie.Text))
        {
            await DisplayAlert("Błąd", "Musisz podać imię zwierzaka!", "OK");
            return;
        }

        // 2. Pobieranie danych z formularza
        string imie = EntryImie.Text;
        string rasa = string.IsNullOrWhiteSpace(EntryRasa.Text) ? "Mieszaniec" : EntryRasa.Text.Trim(); 
        string gatunek = PickerGatunek.SelectedItem?.ToString() ?? "Pies";
        string status = PickerStatus.SelectedItem?.ToString() ?? "Kwarantanna";
        
        // --- NOWE DANE (Pobieramy z nowych pól) ---
        // Jeśli pole jest puste, wpisujemy wartość domyślną
        string wiek = string.IsNullOrWhiteSpace(EntryWiek.Text) ? "Nieznany" : EntryWiek.Text.Trim();
        string historia = string.IsNullOrWhiteSpace(EntryHistoria.Text) ? "Brak wpisów" : EntryHistoria.Text.Trim();

        // 3. --- INTELLIGENTNE DOBIERANIE ZDJĘCIA ---
        string wpisanaRasa = rasa.ToLower(); 
        string tagDoWyszukiwania = "animal"; 

        // -- TŁUMACZ RAS (PL -> EN) --
        // PSY
        if (wpisanaRasa.Contains("owczar")) tagDoWyszukiwania = "germanshepherd";
        else if (wpisanaRasa.Contains("labrador")) tagDoWyszukiwania = "labrador";
        else if (wpisanaRasa.Contains("husky")) tagDoWyszukiwania = "husky";
        else if (wpisanaRasa.Contains("jamnik")) tagDoWyszukiwania = "dachshund";
        else if (wpisanaRasa.Contains("buldog")) tagDoWyszukiwania = "bulldog";
        else if (wpisanaRasa.Contains("mops")) tagDoWyszukiwania = "pug";
        else if (wpisanaRasa.Contains("beagle")) tagDoWyszukiwania = "beagle";
        else if (wpisanaRasa.Contains("chihuahua")) tagDoWyszukiwania = "chihuahua";
        else if (wpisanaRasa.Contains("rottweiler")) tagDoWyszukiwania = "rottweiler";
        else if (wpisanaRasa.Contains("doberman")) tagDoWyszukiwania = "doberman";
        
        // KOTY
        else if (wpisanaRasa.Contains("sfinks")) tagDoWyszukiwania = "sphynx";
        else if (wpisanaRasa.Contains("pers")) tagDoWyszukiwania = "persian,cat";
        else if (wpisanaRasa.Contains("bengal")) tagDoWyszukiwania = "bengal,cat";
        else if (wpisanaRasa.Contains("maine")) tagDoWyszukiwania = "mainecoon";
        else if (wpisanaRasa.Contains("syjam")) tagDoWyszukiwania = "siamese,cat";
        else if (wpisanaRasa.Contains("dachowiec")) tagDoWyszukiwania = "cat";

        // INNE
        else if (wpisanaRasa.Contains("chomik")) tagDoWyszukiwania = "hamster";
        else if (wpisanaRasa.Contains("królik")) tagDoWyszukiwania = "rabbit";
        else if (wpisanaRasa.Contains("papuga")) tagDoWyszukiwania = "parrot";
        else 
        {
            tagDoWyszukiwania = gatunek == "Pies" ? "dog" : (gatunek == "Kot" ? "cat" : "animal");
        }

        string losowyNumer = Guid.NewGuid().ToString().Substring(0, 5);
        string zdjecieUrl = $"https://loremflickr.com/500/500/{tagDoWyszukiwania}?lock={losowyNumer}";

        // 4. Tworzenie obiektu (TERAZ Z WIEKIEM I HISTORIĄ)
        var noweZwierze = new Zwierze
        {
            Imie = imie,
            Rasa = rasa, 
            Status = status,
            Zdjecie = zdjecieUrl,
            // Przypisujemy nowe właściwości:
            Wiek = wiek,
            HistoriaMedyczna = historia
        };

        // 5. Wyślij i zamknij
        WeakReferenceMessenger.Default.Send(noweZwierze);
        await Navigation.PopAsync();
    }
}