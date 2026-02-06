using ShelterManager.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace ShelterManager;

public partial class EditAnimalPage : ContentPage
{
	private readonly Zwierze _zwierze;
	private readonly List<AnimalStatusOption> _statusy;

    // Konstruktor przyjmuje zwierzaka, którego kliknąłeś
    public EditAnimalPage(Zwierze zwierzeDoEdycji)
    {
        InitializeComponent();
		_zwierze = zwierzeDoEdycji;

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
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
		// Zapisujemy wybrany status z pickera do modelu.
		if (PickerStatus.SelectedItem is AnimalStatusOption opt)
			_zwierze.Status = opt.Value;

        // Ponieważ użyliśmy BindingContext, dane w obiekcie 'Zwierze' już się zaktualizowały!
        // Musimy tylko dać znać MainPage, żeby zapisał zmiany do pliku JSON.
        
        // Wysyłamy wiadomość "Odswiez" - treść nie jest ważna, ważny jest sygnał
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
}