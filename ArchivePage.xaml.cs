using System.Collections.ObjectModel;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using ShelterManager.Services;

namespace ShelterManager;

public partial class ArchivePage : ContentPage
{
    private readonly IAnimalRepository _animalRepository;
    private readonly CageAllocationService _cageAllocationService;
    private readonly AnimalEventService _eventService;

    // Kolekcja z repozytorium (zawiera także aktywne i zarchiwizowane rekordy).
    public ObservableCollection<Zwierze> Zwierzeta { get; }

    // Kolekcja widoczna w UI (lista archiwum, opcjonalnie filtrowana wyszukiwarką).
    public ObservableCollection<Zwierze> FilteredArchivedAnimals { get; set; } = new();

    // Używane w XAML do ukrywania opcji "Usuń trwale".
    public bool IsAdmin => LoginPage.RolaUzytkownika == "Admin";

    public ArchivePage()
    {
        InitializeComponent();
        BindingContext = this;

        _animalRepository = ServiceLocator.GetRequiredService<IAnimalRepository>();
        _cageAllocationService = ServiceLocator.GetRequiredService<CageAllocationService>();
        _eventService = ServiceLocator.GetRequiredService<AnimalEventService>();

        Zwierzeta = _animalRepository.Animals;
        OdswiezWidok();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Odświeżamy dane z pliku, bo archiwizacja może zostać wykonana na innej zakładce.
        _animalRepository.Reload();
        OnPropertyChanged(nameof(IsAdmin));

        OdswiezWidok();
    }

    private void OdswiezWidok()
    {
        // Zachowujemy obecny tekst wyszukiwania.
        string obecnyTekst = SearchBarArchiwum?.Text ?? string.Empty;
        OnSearchTextChanged(this, new TextChangedEventArgs("", obecnyTekst));
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string fraza = e.NewTextValue?.ToLowerInvariant() ?? string.Empty;

        var baza = Zwierzeta
            .Where(z => z.IsArchived)
            .OrderByDescending(z => z.ArchivedAt ?? DateTime.MinValue)
            .ToList();

        if (string.IsNullOrWhiteSpace(fraza))
        {
            FilteredArchivedAnimals = new ObservableCollection<Zwierze>(baza);
        }
        else
        {
            var wyniki = baza
                .Where(z => z.Imie.ToLowerInvariant().Contains(fraza))
                .ToList();

            FilteredArchivedAnimals = new ObservableCollection<Zwierze>(wyniki);
        }

        OnPropertyChanged(nameof(FilteredArchivedAnimals));
    }

    private async void OnRestoreClicked(object sender, EventArgs e)
    {
        // Decyzja projektowa: przywracanie mogą robić Administrator i Pracownik.
        if (LoginPage.RolaUzytkownika is not ("Admin" or "Pracownik"))
        {
            await DisplayAlert("Brak uprawnień", "Musisz być zalogowany jako Administrator lub Pracownik.", "OK");
            return;
        }

        if (sender is not MenuFlyoutItem menu || menu.CommandParameter is not Zwierze zwierz)
            return;

        bool potwierdzenie = await DisplayAlert(
            "Przywracanie",
            $"Czy chcesz przywrócić z archiwum: {zwierz.Imie}?",
            "Przywróć",
            "Anuluj");

        if (!potwierdzenie)
            return;

        zwierz.IsArchived = false;
        zwierz.ArchivedAt = null;

        // Audit trail
        _eventService.Log(zwierz.Id, AnimalEventType.Restored, $"Przywrócono z archiwum: {zwierz.Imie}.");

        _animalRepository.SaveChanges();
        OdswiezWidok();
    }

    private async void OnHardDeleteClicked(object sender, EventArgs e)
    {
        if (LoginPage.RolaUzytkownika != "Admin")
        {
            await DisplayAlert("Brak uprawnień", "Tylko Administrator może usuwać trwale rekordy z archiwum.", "OK");
            return;
        }

        if (sender is not MenuFlyoutItem menu || menu.CommandParameter is not Zwierze zwierz)
            return;

        bool potwierdzenie = await DisplayAlert(
            "Potwierdzenie",
            $"Czy na pewno chcesz trwale usunąć: {zwierz.Imie}?\n\nTej operacji nie można cofnąć.",
            "Usuń trwale",
            "Anuluj");

        if (!potwierdzenie)
            return;

        // Zabezpieczenie spójności: usuwamy zwierzę z boksów, jeśli gdzieś "wisi".
        _cageAllocationService.RemoveAnimalFromCage(zwierz.Id);

        // Sprzątamy audit trail, żeby nie zostawiać osieroconych wpisów.
        _eventService.RemoveAllForAnimal(zwierz.Id);

        Zwierzeta.Remove(zwierz);
        _animalRepository.SaveChanges();

        OdswiezWidok();
    }
}
