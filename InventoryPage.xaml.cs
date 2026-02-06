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

        // Responsywność: dopasuj liczbę kolumn (Span) do szerokości okna.
        SizeChanged += OnPageSizeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _resourceRepository.Reload();

        // Stabilna kolejność po nazwie (łatwiejsze wyszukiwanie na małych ekranach).
        Zasoby.SortBy(z => z.Nazwa ?? string.Empty, StringComparer.CurrentCultureIgnoreCase);
        ApplyResponsiveSpan(Width);
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        ApplyResponsiveSpan(Width);
    }

    private void ApplyResponsiveSpan(double pageWidth)
    {
        if (pageWidth <= 0 || InventoryGridLayout is null) return;

        int span = pageWidth < 650 ? 1 : pageWidth < 950 ? 2 : 3;
        if (InventoryGridLayout.Span != span)
            InventoryGridLayout.Span = span;
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
            // Zasob implementuje INotifyPropertyChanged, więc UI odświeża się automatycznie.

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
            // Zasob implementuje INotifyPropertyChanged, więc UI odświeża się automatycznie.
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
                // Usuwamy zasób oraz powiązane transakcje, aby w raportach nie zostawały "osierocone" wpisy.
                Zasoby.Remove(z);

                var toRemove = _transactionRepository.Transactions
                    .Where(t => t.ResourceId == z.Id)
                    .ToList();

                foreach (var t in toRemove)
                    _transactionRepository.Transactions.Remove(t);

                _resourceRepository.SaveChanges();
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
        }
    }

    // Obsługa dużego przycisku "+" (Dodawanie nowego typu zasobu)
    private async void OnAddTypeClicked(object sender, EventArgs e)
    {
        string nazwa = await DisplayPromptAsync("Nowy artykuł", "Podaj nazwę artykułu:");
        if (string.IsNullOrWhiteSpace(nazwa))
            return;

        nazwa = nazwa.Trim();

        // Walidacja: unikamy duplikatów nazwy (UX) i literówek.
        var existing = Zasoby.FirstOrDefault(r => string.Equals(r.Nazwa, nazwa, StringComparison.CurrentCultureIgnoreCase));
        if (existing is not null)
        {
            bool addToExisting = await DisplayAlert(
                "Już istnieje",
                $"Artykuł '{nazwa}' już jest w magazynie. Czy dodać ilość do istniejącej pozycji?",
                "Tak",
                "Nie");

            if (!addToExisting)
                return;

            string deltaStr = await DisplayPromptAsync("Ilość", "O ile zwiększyć stan?", initialValue: "1", keyboard: Keyboard.Numeric);
            if (!double.TryParse(deltaStr, out double delta) || delta <= 0)
            {
                await DisplayAlert("Błąd", "Podaj poprawną ilość (> 0).", "OK");
                return;
            }

            existing.Ilosc += delta;
            _transactionRepository.Transactions.Add(new InventoryTransaction
            {
                ResourceId = existing.Id,
                Delta = delta,
                Reason = "Uzupełnienie ręczne (dodanie pozycji istniejącej)",
                Timestamp = DateTime.UtcNow
            });

            _resourceRepository.SaveChanges();
            return;
        }

        string jednostka = await DisplayPromptAsync("Jednostka", "Podaj jednostkę (np. szt., kg, l):", initialValue: "szt.");
        if (string.IsNullOrWhiteSpace(jednostka))
            jednostka = "szt.";
        jednostka = jednostka.Trim();

        string iloscStr = await DisplayPromptAsync("Ilość", "Podaj ilość początkową:", initialValue: "0", keyboard: Keyboard.Numeric);
        if (!double.TryParse(iloscStr, out double ilosc) || ilosc < 0)
        {
            await DisplayAlert("Błąd", "Podaj poprawną ilość (>= 0).", "OK");
            return;
        }

        string progStr = await DisplayPromptAsync(
            "Próg niskiego stanu",
            "Przy jakiej ilości ma pojawić się ostrzeżenie? (0 = wyłącz)",
            initialValue: "10",
            keyboard: Keyboard.Numeric);

        double prog = 10;
        if (!string.IsNullOrWhiteSpace(progStr))
        {
            if (!double.TryParse(progStr, out prog) || prog < 0)
            {
                await DisplayAlert("Błąd", "Podaj poprawną wartość progu (>= 0).", "OK");
                return;
            }
        }

        var nowy = new Zasob
        {
            Nazwa = nazwa,
            Ilosc = ilosc,
            Jednostka = jednostka,
            LowStockThreshold = prog
        };

        Zasoby.Add(nowy);

        if (ilosc > 0)
        {
            _transactionRepository.Transactions.Add(new InventoryTransaction
            {
                ResourceId = nowy.Id,
                Delta = ilosc,
                Reason = "Dodanie nowego artykułu (stan początkowy)",
                Timestamp = DateTime.UtcNow
            });
        }

        // Stabilna kolejność alfabetyczna.
        Zasoby.SortBy(z => z.Nazwa ?? string.Empty, StringComparer.CurrentCultureIgnoreCase);
        _resourceRepository.SaveChanges();
    }
}
