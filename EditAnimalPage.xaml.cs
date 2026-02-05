using ShelterManager.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace ShelterManager;

public partial class EditAnimalPage : ContentPage
{
    // Konstruktor przyjmuje zwierzaka, którego kliknąłeś
    public EditAnimalPage(Zwierze zwierzeDoEdycji)
    {
        InitializeComponent();
        
        // Ustawiamy tego zwierzaka jako źródło danych dla pól (Entry)
        BindingContext = zwierzeDoEdycji;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
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
}