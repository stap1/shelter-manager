using CommunityToolkit.Mvvm.Messaging;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using ShelterManager.Services;

namespace ShelterManager;

public partial class EditAnimalPage : ContentPage
{
    private readonly Zwierze _zwierze;
    private readonly List<AnimalStatusOption> _statusy;
    private readonly List<CageOption> _boksy;

    private readonly IAnimalRepository _animalRepository;
    private readonly ICageRepository _cageRepository;
    private readonly CageAllocationService _cageAllocationService;

    // Konstruktor przyjmuje zwierzaka, którego kliknąłeś
    public EditAnimalPage(Zwierze zwierzeDoEdycji)
    {
        InitializeComponent();
        _zwierze = zwierzeDoEdycji;

        _animalRepository = ServiceLocator.GetRequiredService<IAnimalRepository>();
        _cageRepository = ServiceLocator.GetRequiredService<ICageRepository>();
        _cageAllocationService = ServiceLocator.GetRequiredService<CageAllocationService>();

        // Ustawiamy tego zwierzaka jako źródło danych dla pól (Entry)
        BindingContext = _zwierze;

        // Status jest enumem - budujemy listę opcji z czytelnymi etykietami (PL).
        _statusy = new List<AnimalStatusOption>
        {
            new(AnimalStatus.Quarantine, "Kwarantanna"),
            new(AnimalStatus.Treatment, "W leczeniu"),
            new(AnimalStatus.ForAdoption, "Do adopcji"),
            new(AnimalStatus.Adopted, "Adoptowany")
        };

        PickerStatus.ItemsSource = _statusy;
        PickerStatus.ItemDisplayBinding = new Binding(nameof(AnimalStatusOption.Display));

        PickerStatus.SelectedItem = _statusy.FirstOrDefault(s => s.Value == _zwierze.Status) ?? _statusy[0];

        // --- Boksy ---
        _boksy = BuildCageOptions();
        PickerCage.ItemsSource = _boksy;
        PickerCage.ItemDisplayBinding = new Binding(nameof(CageOption.Display));

        var currentCage = _cageAllocationService.FindCageOfAnimal(_zwierze.Id);
        PickerCage.SelectedItem = _boksy.FirstOrDefault(b => b.CageId == currentCage?.Id)
                                  ?? _boksy.First();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        // Zapisujemy wybrany status z pickera do modelu.
        if (PickerStatus.SelectedItem is AnimalStatusOption opt)
            _zwierze.Status = opt.Value;

        // Jeśli zwierzę jest Adopted lub Archived, nie może być przydzielone do boksu.
        // W takim wypadku automatycznie zdejmujemy przydział.
        if (_zwierze.Status == AnimalStatus.Adopted || _zwierze.IsArchived)
        {
            _cageAllocationService.RemoveAnimalFromCage(_zwierze.Id);
            PickerCage.SelectedItem = _boksy.First(); // Brak przydziału
        }
        else
        {
            // Obsługa przydziału do boksu na podstawie wyboru w Picker.
            if (PickerCage.SelectedItem is CageOption cageOpt)
            {
                if (cageOpt.CageId is null)
                {
                    _cageAllocationService.RemoveAnimalFromCage(_zwierze.Id);
                }
                else
                {
                    var res = _cageAllocationService.AssignAnimalToCage(_zwierze.Id, cageOpt.CageId.Value);
                    if (!res.Success)
                    {
                        await DisplayAlert("Błąd", res.Error ?? "Nie udało się przydzielić zwierzęcia do boksu.", "OK");
                        return;
                    }
                }
            }
        }

        // Ponieważ użyliśmy BindingContext, dane w obiekcie 'Zwierze' już się zaktualizowały!
        // Musimy tylko dać znać MainPage, żeby zapisał zmiany do pliku JSON.

        // Wysyłamy wiadomość "Odswiez" - treść nie jest ważna, ważny jest sygnał
        // Zapis zmian modelu.
        _animalRepository.SaveChanges();
        _cageRepository.SaveChanges();

        // Odśwież liczniki na MainPage.
        WeakReferenceMessenger.Default.Send("ZapiszMnie");

        await Navigation.PopAsync(); // Wracamy do listy
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    /// <summary>
    /// Prosty "wrapper" na potrzeby UI: Picker wyświetla Display, a w danych trzymamy enum.
    /// </summary>
    private sealed record AnimalStatusOption(AnimalStatus Value, string Display);

    private sealed record CageOption(Guid? CageId, string Display);

    private List<CageOption> BuildCageOptions()
    {
        var list = new List<CageOption>
        {
            new CageOption(null, "Brak przydziału")
        };

        foreach (var cage in _cageRepository.Cages.OrderBy(c => c.Numer))
        {
            list.Add(new CageOption(cage.Id, $"Boks {cage.Numer} ({cage.OccupiedCount}/{cage.Capacity})"));
        }

        return list;
    }
}