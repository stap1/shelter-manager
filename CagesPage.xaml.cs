using Newtonsoft.Json;
using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager;

public partial class CagesPage : ContentPage
{
    // Używamy ObservableCollection, żeby UI wiedziało o zmianach
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

        // 2. Wybierz tylko te, które fizycznie są w schronisku (nie adoptowane)
        var mieszkancy = wszystkieZwierzeta
            .Where(z => z.Status != "Adoptowany")
            .ToList();

        // 3. Generujemy 10 boksów
        for (int i = 1; i <= 10; i++)
        {
            // Tworzymy nową klatkę
            var klatka = new Klatka
            {
                // WAŻNE: Wpisujemy sam numer "1", bo w XAML mamy formatowanie "BOKS {0}"
                Numer = $"{i}",
                Lokator = null
                // UWAGA: Nie musimy ustawiać CzyZajeta = false, bo nasz Model 
                // sam to wie (Lokator jest null -> CzyZajeta jest false)
            };

            // Jeśli mamy zwierzaka dla tej klatki, to go wkładamy
            if (i <= mieszkancy.Count)
            {
                // Przypisujemy lokatora. 
                // Dzięki "sprytnemu" Modelowi Klatka.cs, flaga CzyZajeta ustawi się sama na true!
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