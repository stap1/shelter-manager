using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager;

public partial class TasksPage : ContentPage
{
    // Główna lista zadań podpięta pod XAML
    public ObservableCollection<Zadanie> Zadania { get; set; } = new();

    public TasksPage()
    {
        InitializeComponent();
        BindingContext = this;

        // Przykładowe dane startowe
        Zadania.Add(new Zadanie { Tresc = "Karmienie psów (Sektor A)", Godzina = "08:00", CzyZrobione = true });
        Zadania.Add(new Zadanie { Tresc = "Spacer z Azorem", Godzina = "09:30", CzyZrobione = false });
        Zadania.Add(new Zadanie { Tresc = "Podanie leków: Burek", Godzina = "12:00", CzyZrobione = false });
    }

    // Obsługa przycisku "DODAJ"
    private void OnAddClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(EntryNoweZadanie.Text))
        {
            // Dodajemy nowe zadanie na początek listy (Insert 0), żeby było na górze, 
            // lub na koniec (Add) - jak wolisz. Tu używam Add.
            Zadania.Add(new Zadanie
            {
                Tresc = EntryNoweZadanie.Text,
                Godzina = DateTime.Now.ToString("HH:mm"),
                CzyZrobione = false
            });

            EntryNoweZadanie.Text = string.Empty;
            EntryNoweZadanie.Unfocus(); // Chowa klawiaturę
        }
    }

    // Obsługa przycisku "X" (Usuwanie)
    private void OnDeleteTaskClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zadanie zad)
        {
            Zadania.Remove(zad);
        }
    }

    // NOWA METODA: Obsługa kliknięcia w cały wiersz (TapGestureRecognizer)
    private void OnTaskTapped(object sender, TappedEventArgs e)
    {
        // Sprawdzamy, czy kliknięty element (Border) ma przypisane zadanie
        if (sender is VisualElement element && element.BindingContext is Zadanie zadanie)
        {
            // Zmieniamy status na przeciwny (True na False i odwrotnie)
            // Dzięki INotifyPropertyChanged w modelu, widok sam się odświeży!
            zadanie.CzyZrobione = !zadanie.CzyZrobione;
        }
    }
}