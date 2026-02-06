using System.Collections.ObjectModel;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class InventoryPage : ContentPage
{
    private readonly IResourceRepository _resourceRepository;

    // Kolekcja z repozytorium, podpięta pod XAML
    public ObservableCollection<Zasob> Zasoby { get; }

    public InventoryPage()
    {
        InitializeComponent();

        _resourceRepository = ServiceLocator.GetRequiredService<IResourceRepository>();
        Zasoby = _resourceRepository.Resources;

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _resourceRepository.Reload();
    }

    // Obsługa przycisku "-" (w XAML: OnMinusClicked)
    private async void OnMinusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zasob z)
        {
            if (z.Ilosc > 0)
            {
                z.Ilosc -= 1;
                _resourceRepository.SaveChanges();
                OdswiezListe();

                // AUTOMATYCZNE POWIADOMIENIE (Punkt 9)
                if (z.Ilosc <= 0)
                {
                    try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100)); } catch { }
                    await DisplayAlert("⚠️ NISKI STAN!", $"Produkt: {z.Nazwa} właśnie się skończył! Uzupełnij zapasy.", "OK");
                }
            }
        }
    }

    // Obsługa przycisku "+" (w XAML: OnPlusClicked)
    private void OnPlusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zasob z)
        {
            z.Ilosc += 1;
            _resourceRepository.SaveChanges();
            OdswiezListe();
        }
    }

    // Obsługa przycisku "Usuń" (w XAML: OnDeleteClicked)
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zasob z)
        {
            bool answer = await DisplayAlert("Usuwanie", $"Czy usunąć {z.Nazwa} z magazynu?", "Tak", "Nie");
            if (answer)
            {
                Zasoby.Remove(z);
                _resourceRepository.SaveChanges();
                OdswiezListe();
            }
        }
    }

    // Obsługa dużego przycisku "+" (Dodawanie nowego typu zasobu)
    private async void OnAddTypeClicked(object sender, EventArgs e)
    {
        string nazwa = await DisplayPromptAsync("Nowy produkt", "Podaj nazwę produktu:");
        if (!string.IsNullOrWhiteSpace(nazwa))
        {
            string iloscStr = await DisplayPromptAsync("Ilość", "Podaj ilość początkową:", initialValue: "0", keyboard: Keyboard.Numeric);
            if (double.TryParse(iloscStr, out double ilosc))
            {
                Zasoby.Add(new Zasob { Nazwa = nazwa.Trim(), Ilosc = ilosc, Jednostka = "szt." });
                _resourceRepository.SaveChanges();
            }
        }
    }

    // Pomocnicza metoda do wymuszenia odświeżenia widoku (czasem potrzebna w MAUI przy zmianach wewnątrz obiektów)
    private void OdswiezListe()
    {
        var temp = ListaZasobow.ItemsSource;
        ListaZasobow.ItemsSource = null;
        ListaZasobow.ItemsSource = temp;
    }
}
