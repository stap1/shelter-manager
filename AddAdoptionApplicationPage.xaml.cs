using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class AddAdoptionApplicationPage : ContentPage
{
    private readonly IAnimalRepository _animals;
    private readonly IAdoptionApplicationRepository _applications;

    private readonly bool _canManage = LoginPage.RolaUzytkownika is ("Admin" or "Pracownik");

    private sealed class AnimalOption
    {
        public Zwierze Animal { get; }
        public string Display { get; }

        public AnimalOption(Zwierze animal)
        {
            Animal = animal;
            Display = $"{animal.Imie} ({animal.GatunekOpis}, {animal.Rasa})";
        }
    }

    private List<AnimalOption> _options = new();

    public AddAdoptionApplicationPage()
    {
        InitializeComponent();

        _animals = ServiceLocator.GetRequiredService<IAnimalRepository>();
        _applications = ServiceLocator.GetRequiredService<IAdoptionApplicationRepository>();

        LoadAnimals();
    }

    private void LoadAnimals()
    {
        // Wniosek ma sens głównie dla zwierząt oznaczonych jako Do adopcji.
        // Dodatkowo wykluczamy Adopted i Archiwum.
        _options = _animals.Animals
            .Where(a => !a.IsArchived && a.Status == AnimalStatus.ForAdoption)
            .OrderBy(a => a.Imie)
            .Select(a => new AnimalOption(a))
            .ToList();

        PickerAnimal.ItemsSource = _options;
        PickerAnimal.ItemDisplayBinding = new Binding(nameof(AnimalOption.Display));

        if (_options.Count > 0)
            PickerAnimal.SelectedIndex = 0;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (!_canManage)
        {
            await DisplayAlert("Brak uprawnień", "Tylko Administrator lub Pracownik może dodawać wnioski.", "OK");
            return;
        }

        if (PickerAnimal.SelectedItem is not AnimalOption selected)
        {
            await DisplayAlert("Błąd", "Wybierz zwierzę.", "OK");
            return;
        }

        var applicant = EntryApplicant.Text?.Trim() ?? string.Empty;
        var contact = EntryContact.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(applicant) || string.IsNullOrWhiteSpace(contact))
        {
            await DisplayAlert("Błąd", "Uzupełnij wnioskodawcę i kontakt.", "OK");
            return;
        }

        // Walidacja biznesowa: nie przyjmujemy wniosków dla Adopted/Archived.
        var animal = selected.Animal;
        if (animal.IsArchived || animal.Status == AnimalStatus.Adopted)
        {
            await DisplayAlert("Błąd", "Nie można utworzyć wniosku dla Adopted/Archived.", "OK");
            return;
        }

        var app = new AdoptionApplication
        {
            AnimalId = animal.Id,
            ApplicantName = applicant,
            Contact = contact,
            Status = AdoptionApplicationStatus.New,
            Notes = EditorNotes.Text?.Trim() ?? string.Empty
        };

        // Powiązanie dla UI (opcjonalne) - data store i tak to odtworzy po zapisie.
        app.SetResolvedAnimal(animal);

        _applications.Applications.Add(app);
        _applications.SaveChanges();

        await DisplayAlert("OK", "Dodano wniosek.", "OK");
        await Navigation.PopAsync();
    }

    private async void OnCancelClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}
