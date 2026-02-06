using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class TasksPage : ContentPage
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAnimalRepository _animalRepository;

    private TaskViewMode _viewMode = TaskViewMode.Today;

    // Lista podpięta pod UI (filtry, Dzisiaj/Nadchodzące)
    public ObservableCollection<Zadanie> WidoczneZadania { get; } = new();

    private readonly List<Option<CareTaskStatus?>> _statusOptions = new();
    private readonly List<Option<CareTaskType?>> _typeOptions = new();
    private readonly List<Option<Guid?>> _animalOptions = new();

    public TasksPage()
    {
        InitializeComponent();

        _taskRepository = ServiceLocator.GetRequiredService<ITaskRepository>();
        _animalRepository = ServiceLocator.GetRequiredService<IAnimalRepository>();

        // Reagujemy na zmiany kolekcji (np. po dodaniu z innej strony)
        _taskRepository.Tasks.CollectionChanged += OnTasksCollectionChanged;

        BindingContext = this;

        BuildFilterOptions();
        ApplyViewButtonStyles();
        RefreshVisibleTasks();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _taskRepository.Reload();
        _animalRepository.Reload();
        BuildFilterOptions();
        RefreshVisibleTasks();
    }

    private void OnTasksCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Gdy repozytorium doda/usunie zadania, odśwież widok.
        RefreshVisibleTasks();
    }

    private void BuildFilterOptions()
    {
        _statusOptions.Clear();
        _typeOptions.Clear();
        _animalOptions.Clear();

        _statusOptions.Add(new Option<CareTaskStatus?> { Label = "Wszystkie", Value = null });
        _statusOptions.Add(new Option<CareTaskStatus?> { Label = "Planowane", Value = CareTaskStatus.Planned });
        _statusOptions.Add(new Option<CareTaskStatus?> { Label = "Wykonane", Value = CareTaskStatus.Done });
        _statusOptions.Add(new Option<CareTaskStatus?> { Label = "Anulowane", Value = CareTaskStatus.Cancelled });

        _typeOptions.Add(new Option<CareTaskType?> { Label = "Wszystkie", Value = null });
        foreach (var t in Enum.GetValues(typeof(CareTaskType)).Cast<CareTaskType>())
        {
            _typeOptions.Add(new Option<CareTaskType?> { Label = ToTypeLabel(t), Value = t });
        }

        _animalOptions.Add(new Option<Guid?> { Label = "Wszystkie zwierzęta", Value = null });
        _animalOptions.Add(new Option<Guid?> { Label = "Bez zwierzęcia", Value = Guid.Empty });
        foreach (var a in _animalRepository.Animals.Where(z => !z.IsArchived && z.Status != AnimalStatus.Adopted).OrderBy(z => z.Imie))
        {
            _animalOptions.Add(new Option<Guid?> { Label = a.Imie, Value = a.Id });
        }

        PickerStatus.ItemsSource = _statusOptions;
        PickerType.ItemsSource = _typeOptions;
        PickerAnimal.ItemsSource = _animalOptions;

        // Zachowujemy wybory, jeśli da się je utrzymać.
        if (PickerStatus.SelectedIndex < 0) PickerStatus.SelectedIndex = 0;
        if (PickerType.SelectedIndex < 0) PickerType.SelectedIndex = 0;
        if (PickerAnimal.SelectedIndex < 0) PickerAnimal.SelectedIndex = 0;
    }

    private void RefreshVisibleTasks()
    {
        var today = DateTime.Today;

        var statusFilter = (PickerStatus.SelectedItem as Option<CareTaskStatus?>)?.Value;
        var typeFilter = (PickerType.SelectedItem as Option<CareTaskType?>)?.Value;
        var animalFilter = (PickerAnimal.SelectedItem as Option<Guid?>)?.Value;

        IEnumerable<Zadanie> query = _taskRepository.Tasks;

        query = _viewMode switch
        {
            // DZIŚ: zadania na dzisiaj + zaległe (tylko jeśli dalej Planned)
            TaskViewMode.Today => query.Where(t => t.ScheduledAt.Date == today || (t.ScheduledAt.Date < today && t.Status == CareTaskStatus.Planned)),

            // NADCHODZĄCE: od jutra w górę
            TaskViewMode.Upcoming => query.Where(t => t.ScheduledAt.Date > today),

            _ => query
        };

        if (statusFilter is not null)
            query = query.Where(t => t.Status == statusFilter.Value);

        if (typeFilter is not null)
            query = query.Where(t => t.Type == typeFilter.Value);

        if (animalFilter is not null)
        {
            if (animalFilter.Value == Guid.Empty)
                query = query.Where(t => !t.AnimalId.HasValue);
            else
                query = query.Where(t => t.AnimalId == animalFilter.Value);
        }

        query = query.OrderBy(t => t.ScheduledAt);

        WidoczneZadania.Clear();
        foreach (var t in query)
            WidoczneZadania.Add(t);
    }

    private void OnFilterChanged(object sender, EventArgs e) => RefreshVisibleTasks();

    private void OnTodayClicked(object sender, EventArgs e)
    {
        _viewMode = TaskViewMode.Today;
        ApplyViewButtonStyles();
        RefreshVisibleTasks();
    }

    private void OnUpcomingClicked(object sender, EventArgs e)
    {
        _viewMode = TaskViewMode.Upcoming;
        ApplyViewButtonStyles();
        RefreshVisibleTasks();
    }

    private void ApplyViewButtonStyles()
    {
        if (_viewMode == TaskViewMode.Today)
        {
            BtnToday.BackgroundColor = Color.FromArgb("#2196F3");
            BtnToday.TextColor = Colors.White;

            BtnUpcoming.BackgroundColor = Color.FromArgb("#E3F2FD");
            BtnUpcoming.TextColor = Color.FromArgb("#1A237E");
        }
        else
        {
            BtnToday.BackgroundColor = Color.FromArgb("#E3F2FD");
            BtnToday.TextColor = Color.FromArgb("#1A237E");

            BtnUpcoming.BackgroundColor = Color.FromArgb("#2196F3");
            BtnUpcoming.TextColor = Colors.White;
        }
    }

    private async void OnAddTaskClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new AddCareTaskPage());

    private void OnDeleteTaskClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zadanie zad)
        {
            _taskRepository.Tasks.Remove(zad);
            _taskRepository.SaveChanges();
            RefreshVisibleTasks();
        }
    }

    private void OnCancelTaskClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Zadanie zad)
        {
            zad.Cancel();
            _taskRepository.SaveChanges();
            RefreshVisibleTasks();
        }
    }

    private void OnToggleDoneClicked(object sender, EventArgs e)
    {
        // Na Windows (WinUI) CheckBox w wirtualizowanych listach bywa źródłem losowych crashy.
        // Używamy przycisku jako stabilnego przełącznika.
        if (sender is Button btn && btn.CommandParameter is Zadanie zad)
        {
            zad.IsDone = !zad.IsDone;
            _taskRepository.SaveChanges();
            RefreshVisibleTasks();
        }
    }

    private static string ToTypeLabel(CareTaskType t) => t switch
    {
        CareTaskType.Feeding => "Karmienie",
        CareTaskType.Walking => "Spacer",
        CareTaskType.Cleaning => "Sprzątanie",
        CareTaskType.Medication => "Leki",
        CareTaskType.VetVisit => "Weterynarz",
        CareTaskType.Grooming => "Pielęgnacja",
        _ => "Inne"
    };

    private enum TaskViewMode
    {
        Today,
        Upcoming
    }

    private sealed class Option<T>
    {
        public string Label { get; init; } = string.Empty;
        public T Value { get; init; } = default!;
        public override string ToString() => Label;
    }
}
