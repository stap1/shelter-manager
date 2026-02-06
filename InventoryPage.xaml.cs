using System.Collections.ObjectModel;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class InventoryPage : ContentPage
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;

    // Kolekcja z repozytorium, podpięta pod XAML
    public ObservableCollection<Zasob> Zasoby { get; }

    public InventoryPage()
    {
        InitializeComponent();

        _resourceRepository = ServiceLocator.GetRequiredService<IResourceRepository>();
        _transactionRepository = ServiceLocator.GetRequiredService<IInventoryTransactionRepository>();
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
            if (z.Ilosc <= 0)
            {
                await DisplayAlert("Informacja", "Nie możesz zużyć zasobu, który ma 0.", "OK");
                return;
            }

            // Zużycie z powodem: ilość + reason. Zapisujemy transakcję.
            string iloscStr = await DisplayPromptAsync(
                "Zużycie",
                $"Ile zużyć z '{z.Nazwa}'?",
                initialValue: "1",
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(iloscStr)) return;
            if (!double.TryParse(iloscStr, out double iloscZuzycia) || iloscZuzycia <= 0)
            {
                await DisplayAlert("Błąd", "Podaj poprawną ilość (> 0).", "OK");
                return;
            }

            if (iloscZuzycia > z.Ilosc)
            {
                await DisplayAlert("Błąd", "Nie możesz zużyć więcej niż aktualny stan magazynowy.", "OK");
                return;
            }

            string powod = await DisplayPromptAsync(
                "Powód zużycia",
                "Podaj powód (np. karmienie, sprzątanie, zabieg):",
                initialValue: "Zużycie");

            if (string.IsNullOrWhiteSpace(powod))
                powod = "Zużycie";

            double before = z.Ilosc;
            z.Ilosc -= iloscZuzycia;

            _transactionRepository.Transactions.Add(new InventoryTransaction
            {
                ResourceId = z.Id,
                Delta = -iloscZuzycia,
                Reason = powod.Trim(),
                Timestamp = DateTime.UtcNow
            });

            _resourceRepository.SaveChanges();
            OdswiezListe();

            // Powiadomienie: gdy spadnie poniżej progu, nie tylko do 0.
            double threshold = z.LowStockThreshold;
            if (threshold > 0 && before >= threshold && z.Ilosc < threshold)
            {
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(100)); } catch { }
                if (z.Ilosc <= 0)
                {
                    await DisplayAlert("⚠️ NISKI STAN!", $"Produkt: {z.Nazwa} właśnie się skończył! Uzupełnij zapasy.", "OK");
                }
                else
                {
                    await DisplayAlert("⚠️ NISKI STAN!", $"Produkt: {z.Nazwa} spadł poniżej progu ({threshold}). Aktualnie: {z.Ilosc}.", "OK");
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

            // Drobna historia zmian bez dodatkowych promptów.
            _transactionRepository.Transactions.Add(new InventoryTransaction
            {
                ResourceId = z.Id,
                Delta = 1,
                Reason = "Uzupełnienie ręczne",
                Timestamp = DateTime.UtcNow
            });

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

    // Edycja progu niskiego stanu (w XAML: OnEditThresholdClicked)
    private async void OnEditThresholdClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zasob z)
        {
            string progStr = await DisplayPromptAsync(
                "Próg niskiego stanu",
                $"Ustaw próg dla '{z.Nazwa}' (0 = wyłącz ostrzeżenia):",
                initialValue: z.LowStockThreshold.ToString(),
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(progStr)) return;
            if (!double.TryParse(progStr, out double prog) || prog < 0)
            {
                await DisplayAlert("Błąd", "Podaj poprawną wartość (>= 0).", "OK");
                return;
            }

            z.LowStockThreshold = prog;
            _resourceRepository.SaveChanges();
            OdswiezListe();
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
                string progStr = await DisplayPromptAsync("Próg niskiego stanu", "Przy jakiej ilości ma pojawić się ostrzeżenie?", initialValue: "10", keyboard: Keyboard.Numeric);
                double prog = 10;
                if (!string.IsNullOrWhiteSpace(progStr) && double.TryParse(progStr, out var parsed) && parsed >= 0)
                    prog = parsed;

                Zasoby.Add(new Zasob
                {
                    Nazwa = nazwa.Trim(),
                    Ilosc = ilosc,
                    Jednostka = "szt.",
                    LowStockThreshold = prog
                });
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
