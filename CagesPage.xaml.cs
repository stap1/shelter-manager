using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using System.Collections.ObjectModel;

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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // W razie gdyby inne zakładki zmieniły dane (np. status zwierzęcia),
        // odświeżamy stan z pliku.
        _cageRepository.Reload();
    }

    private async void OnManageCagesClicked(object sender, EventArgs e)
    {
        // Przechodzimy do zarządzania boksami (dodawanie/usuwanie/pojemność)
        await Shell.Current.GoToAsync(nameof(ManageCagesPage));
    }
}
