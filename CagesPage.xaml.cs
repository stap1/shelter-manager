using System.Collections.ObjectModel;
using ShelterManager.Models;
using Newtonsoft.Json;

namespace ShelterManager;

public partial class CagesPage : ContentPage
{
    public ObservableCollection<Klatka> Klatki { get; set; } = new();
    string filePath = Path.Combine(FileSystem.AppDataDirectory, "shelter_data.json");

    public CagesPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        GenerujKlatki();
    }

    private void GenerujKlatki()
    {
        Klatki.Clear();
        
        // 1. Pobierz zwierzęta z bazy
        var wszystkieZwierzeta = WczytajZwierzeta();

        // 2. Wybierz tylko te, które fizycznie są w schronisku
        var mieszkancy = wszystkieZwierzeta
            .Where(z => z.Status != "Adoptowany")
            .ToList();

        // 3. Generujemy 10 boksów
        for (int i = 1; i <= 10; i++)
        {
            var klatka = new Klatka
            {
                Numer = $"Boks {i}",
                Status = "Wolna",
                Lokator = null
            };

            // Jeśli mamy zwierzaka dla tej klatki, to go wkładamy
            if (i <= mieszkancy.Count)
            {
                klatka.Status = "Zajęta";
                klatka.Lokator = mieszkancy[i - 1];
            }

            Klatki.Add(klatka);
        }
    }

    private List<Zwierze> WczytajZwierzeta()
    {
        if (!File.Exists(filePath)) return new List<Zwierze>();
        try
        {
            var json = File.ReadAllText(filePath);
            var lista = JsonConvert.DeserializeObject<List<Zwierze>>(json);
            return lista ?? new List<Zwierze>();
        }
        catch
        {
            return new List<Zwierze>();
        }
    }
}

// --- POPRAWIONY MODEL (Rozwiązanie problemów) ---
public class Klatka
{
    // Dajemy = ""; żeby C# nie krzyczał, że "nie ustawiono wartości"
    public string Numer { get; set; } = ""; 
    
    public string Status { get; set; } = "Wolna";

    // Dodajemy znak zapytania '?' przy Zwierze. 
    // To mówi programowi: "To pole może być puste (null), nie panikuj".
    public Zwierze? Lokator { get; set; } 
}