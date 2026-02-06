using System.Collections.ObjectModel;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class CagesPage : ContentPage
{
    private readonly ICageRepository _cageRepository;

    // Kolekcja z repozytorium, podpięta pod XAML
    public ObservableCollection<Cage> Klatki { get; }

    public CagesPage()
    {
        InitializeComponent();

        // Ważne: BindingContext ustawiamy dopiero po przypisaniu Klatki.
        // W przeciwnym razie binding ItemsSource czyta null i nie odświeża się,
        // bo Klatki jest właściwością tylko do odczytu (brak setter + brak OnPropertyChanged).
        _cageRepository = ServiceLocator.GetRequiredService<ICageRepository>();
        Klatki = _cageRepository.Cages;

        BindingContext = this;

        // Responsywność: dopasuj liczbę kolumn do szerokości (telefon/tablet/desktop).
        SizeChanged += OnPageSizeChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // W razie gdyby inne zakładki zmieniły dane (np. status zwierzęcia),
        // odświeżamy stan z pliku.
        _cageRepository.Reload();
        // Stabilna kolejność rosnąco po numerze.
        Klatki.SortBy(c => c.Numer);
        ApplyResponsiveSpan(Width);
    }

    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        ApplyResponsiveSpan(Width);
    }

    private void ApplyResponsiveSpan(double pageWidth)
    {
        if (pageWidth <= 0 || CagesGridLayout is null) return;

        // Proste progi: na wąskich ekranach 1 kolumna, na szerszych 2-3.
        int span = pageWidth < 650 ? 1 : pageWidth < 950 ? 2 : 3;
        if (CagesGridLayout.Span != span)
            CagesGridLayout.Span = span;
    }

    private async void OnManageCagesClicked(object sender, EventArgs e)
    {
        // Przechodzimy do zarządzania boksami (dodawanie/usuwanie/pojemność)
        await Shell.Current.GoToAsync(nameof(ManageCagesPage));
    }
}
