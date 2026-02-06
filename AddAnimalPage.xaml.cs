using CommunityToolkit.Mvvm.Messaging;
using ShelterManager.Models;

namespace ShelterManager;

public partial class AddAnimalPage : ContentPage
{
    public AddAnimalPage()
    {
        InitializeComponent();

        // Ustawiamy domyślne wartości, żeby użytkownik nie musiał klikać wszystkiego
        PickerGatunek.SelectedIndex = 0; // Domyślnie Pies
        PickerStatus.SelectedIndex = 0;  // Domyślnie Kwarantanna
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
        string imie = EntryImie.Text.Trim();
        string rasa = string.IsNullOrWhiteSpace(EntryRasa.Text) ? "Mieszaniec" : EntryRasa.Text.Trim();

        // Bezpieczne pobranie wartości z Pickerów
        string gatunekText = PickerGatunek.SelectedItem?.ToString() ?? "Pies";
        string statusText = PickerStatus.SelectedItem?.ToString() ?? "Kwarantanna";

        // Mapujemy tekst z UI na enumy, żeby uniknąć literówek w danych.
        AnimalSpecies gatunek = ParseSpecies(gatunekText);
        AnimalStatus status = ParseStatus(statusText);

        string wiek = string.IsNullOrWhiteSpace(EntryWiek.Text) ? "Nieznany" : EntryWiek.Text.Trim();
        string historia = string.IsNullOrWhiteSpace(EntryHistoria.Text) ? "Brak wpisów" : EntryHistoria.Text.Trim();

        // 3. --- INTELLIGENTNE DOBIERANIE ZDJĘCIA (Poprawione) ---
        // Budujemy URL w oparciu o gatunek + rasę dla lepszych wyników
        string zdjecieUrl = GenerujUrlZdjecia(gatunek, rasa);

        // 4. Tworzenie obiektu
        var noweZwierze = new Zwierze
        {
            Imie = imie,
            Gatunek = gatunek,
            Rasa = rasa,
            Status = status,
            Zdjecie = zdjecieUrl,
            Wiek = wiek,
            HistoriaMedyczna = historia
        };

        // 5. Wyślij komunikat do MainPage i zamknij okno
        WeakReferenceMessenger.Default.Send(noweZwierze);
        await Navigation.PopAsync();
    }

    // Wydzielona metoda do generowania URL - czyściej i bezpieczniej
    private string GenerujUrlZdjecia(AnimalSpecies gatunek, string rasa)
    {
        string rasaLow = rasa.ToLower();
        string tagi = "animal"; // Domyślny tag

        // Logika dla PSÓW
        if (gatunek == AnimalSpecies.Dog)
        {
            tagi = "dog"; // Baza to pies

            if (rasaLow.Contains("owczar")) tagi = "dog,germanshepherd";
            else if (rasaLow.Contains("labrador")) tagi = "dog,labrador";
            else if (rasaLow.Contains("golden")) tagi = "dog,goldenretriever";
            else if (rasaLow.Contains("husky")) tagi = "dog,husky";
            else if (rasaLow.Contains("jamnik")) tagi = "dog,dachshund";
            else if (rasaLow.Contains("buldog")) tagi = "dog,bulldog";
            else if (rasaLow.Contains("mops")) tagi = "dog,pug";
            else if (rasaLow.Contains("beagle")) tagi = "dog,beagle";
            else if (rasaLow.Contains("chihuahua")) tagi = "dog,chihuahua";
            else if (rasaLow.Contains("rottweiler")) tagi = "dog,rottweiler";
            else if (rasaLow.Contains("doberman")) tagi = "dog,doberman";
            else if (rasaLow.Contains("kundel") || rasaLow.Contains("mieszaniec")) tagi = "dog,mongrel";
        }
        // Logika dla KOTÓW
        else if (gatunek == AnimalSpecies.Cat)
        {
            tagi = "cat"; // Baza to kot

            if (rasaLow.Contains("sfinks")) tagi = "cat,sphynx";
            else if (rasaLow.Contains("pers")) tagi = "cat,persian";
            else if (rasaLow.Contains("bengal")) tagi = "cat,bengal";
            else if (rasaLow.Contains("maine")) tagi = "cat,mainecoon";
            else if (rasaLow.Contains("syjam")) tagi = "cat,siamese";
            else if (rasaLow.Contains("czarny")) tagi = "cat,black";
            else if (rasaLow.Contains("rudy")) tagi = "cat,ginger";
        }
        // INNE
        else
        {
            if (rasaLow.Contains("chomik")) tagi = "hamster";
            else if (rasaLow.Contains("królik")) tagi = "rabbit";
            else if (rasaLow.Contains("papuga")) tagi = "parrot,bird";
            else if (rasaLow.Contains("świnka")) tagi = "guineapig";
        }

        // Generujemy losową liczbę, żeby zdjęcie było unikalne dla tego zwierzaka
        int losowyNumer = new Random().Next(1, 10000);

        // Format URL: 400x400 jest szybsze niż 500x500 i wystarcza na telefon/okno
        return $"https://loremflickr.com/400/400/{tagi}?lock={losowyNumer}";
    }

    private static AnimalSpecies ParseSpecies(string raw)
    {
        string key = (raw ?? string.Empty).Trim().ToLowerInvariant();
        return key switch
        {
            "pies" => AnimalSpecies.Dog,
            "kot" => AnimalSpecies.Cat,
            "inne" => AnimalSpecies.Other,
            _ => Enum.TryParse<AnimalSpecies>(raw, ignoreCase: true, out var parsed) ? parsed : AnimalSpecies.Unknown
        };
    }

    private static AnimalStatus ParseStatus(string raw)
    {
        string key = (raw ?? string.Empty).Trim().ToLowerInvariant();
        return key switch
        {
            "kwarantanna" => AnimalStatus.Quarantine,
            "w leczeniu" => AnimalStatus.Treatment,
            "do adopcji" => AnimalStatus.ForAdoption,
            "adoptowany" => AnimalStatus.Adopted,
            _ => Enum.TryParse<AnimalStatus>(raw, ignoreCase: true, out var parsed) ? parsed : AnimalStatus.Unknown
        };
    }
}