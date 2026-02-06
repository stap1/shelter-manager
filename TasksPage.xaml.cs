using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager;

public partial class TasksPage : ContentPage
{
    private readonly ITaskRepository _taskRepository;

    // Główna lista zadań podpięta pod XAML
    public ObservableCollection<Zadanie> Zadania { get; }

    public TasksPage()
    {
        InitializeComponent();

        _taskRepository = ServiceLocator.GetRequiredService<ITaskRepository>();
        Zadania = _taskRepository.Tasks;

        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _taskRepository.Reload();
    }

    // Obsługa przycisku "DODAJ"
    private void OnAddClicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(EntryNoweZadanie.Text))
        {
            Zadania.Add(new Zadanie
            {
                Tresc = EntryNoweZadanie.Text.Trim(),
                Godzina = DateTime.Now.ToString("HH:mm"),
                CzyZrobione = false
            });

            _taskRepository.SaveChanges();

            EntryNoweZadanie.Text = string.Empty;
            EntryNoweZadanie.Unfocus();
        }
    }

    // Obsługa przycisku "X" (Usuwanie)
    private void OnDeleteTaskClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zadanie zad)
        {
            Zadania.Remove(zad);
            _taskRepository.SaveChanges();
        }
    }

    // Obsługa kliknięcia w cały wiersz (TapGestureRecognizer)
    private void OnTaskTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Zadanie zadanie)
        {
            zadanie.CzyZrobione = !zadanie.CzyZrobione;
            _taskRepository.SaveChanges();
        }
    }
}
