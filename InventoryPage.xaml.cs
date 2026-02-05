using System.Collections.ObjectModel;
using ShelterManager.Models; // Upewnij się, że to pasuje do Twojej przestrzeni nazw

namespace ShelterManager;

// Jeśli nie masz osobnego pliku w Models, możesz odkomentować tę klasę tutaj:
/*
public class Zasob
{
    public string Nazwa { get; set; } = string.Empty;
    public int Ilosc { get; set; }
}
*/

public partial class InventoryPage : ContentPage
{
    public ObservableCollection<Zasob> Zasoby { get; set; } = new();

    public InventoryPage()
    {
        InitializeComponent();
        GenerujDane();
        
        // Teraz to zadziała, bo w XAML dodaliśmy x:Name="ListaZasobow"
        BindingContext = this; 
        // Alternatywnie: ListaZasobow.ItemsSource = Zasoby; (ale BindingContext jest elegantszy przy {Binding Zasoby} w XAML)
    }

    private void GenerujDane()
    {
        if (Zasoby.Count == 0)
        {
            Zasoby.Add(new Zasob { Nazwa = "Karma sucha (Pies)", Ilosc = 20 });
            Zasoby.Add(new Zasob { Nazwa = "Karma mokra (Kot)", Ilosc = 15 });
            Zasoby.Add(new Zasob { Nazwa = "Podkłady higieniczne", Ilosc = 5 });
            Zasoby.Add(new Zasob { Nazwa = "Szampon", Ilosc = 1 });
            Zasoby.Add(new Zasob { Nazwa = "Smycze", Ilosc = 8 });
        }
    }

    // Obsługa przycisku "-" (w XAML: OnMinusClicked)
    private async void OnMinusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zasob z)
        {
            if (z.Ilosc > 0)
            {
                z.Ilosc--;
                OdswiezListe();

                // AUTOMATYCZNE POWIADOMIENIE (Punkt 9)
                if (z.Ilosc == 0)
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
            z.Ilosc++;
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
                OdswiezListe(); // Ważne przy CollectionView w GridItemsLayout, czasem trzeba odświeżyć
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
            if (int.TryParse(iloscStr, out int ilosc))
            {
                Zasoby.Add(new Zasob { Nazwa = nazwa, Ilosc = ilosc });
            }
        }
    }

    // Pomocnicza metoda do wymuszenia odświeżenia widoku (czasem potrzebna w MAUI przy zmianach wewnątrz obiektów)
    private void OdswiezListe()
    {
        // Trik na odświeżenie CollectionView
        var temp = ListaZasobow.ItemsSource;
        ListaZasobow.ItemsSource = null;
        ListaZasobow.ItemsSource = temp;
    }
}