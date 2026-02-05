using System.Collections.ObjectModel;

namespace ShelterManager;

public class Zadanie
{
    public string Tresc { get; set; } = string.Empty;
    public bool CzyZrobione { get; set; }
}

public partial class TasksPage : ContentPage
{
    public ObservableCollection<Zadanie> Zadania { get; set; } = new();

    public TasksPage()
    {
        InitializeComponent();
        
        Zadania.Add(new Zadanie { Tresc = "08:00 - Karmienie psów (Sektor A)", CzyZrobione = false });
        Zadania.Add(new Zadanie { Tresc = "09:30 - Spacer z Azorem", CzyZrobione = false });
        Zadania.Add(new Zadanie { Tresc = "12:00 - Podanie leków: Burek", CzyZrobione = false });

        ListaZadan.ItemsSource = Zadania;
    }

    private void OnAddTaskClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(EntryZadanie.Text))
        {
            Zadania.Insert(0, new Zadanie { Tresc = EntryZadanie.Text, CzyZrobione = false });
            EntryZadanie.Text = string.Empty;
        }
    }

    private async void OnTaskCompleted(object sender, CheckedChangedEventArgs e)
    {
        // Bezpieczne sprawdzenie typu sender
        if (e.Value && sender is CheckBox checkbox)
        {
            if (checkbox.BindingContext is Zadanie zadanie)
            {
                await Task.Delay(1000);
                Zadania.Remove(zadanie);
            }
        }
    }
}