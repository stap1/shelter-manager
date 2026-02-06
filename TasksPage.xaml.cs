using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using Microsoft.Maui.Storage;
using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;
using ShelterManager.Services;

namespace ShelterManager;

public partial class TasksPage : ContentPage
{
	private const string OverdueAlertDateKey = "Tasks.OverdueAlertDate";

    private readonly ITaskRepository _taskRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly AnimalEventService _eventService;

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
        _eventService = ServiceLocator.GetRequiredService<AnimalEventService>();

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

		// Proste powiadomienie o zaległych zadaniach (raz dziennie), żeby domknąć wymaganie z Projekt.txt.
		_ = ShowOverdueTasksNotificationIfNeededAsync();
    }

	private async Task ShowOverdueTasksNotificationIfNeededAsync()
	{
		var today = DateTime.Today;
		int overdueCount = _taskRepository.Tasks.Count(t => t.Status == CareTaskStatus.Planned && t.ScheduledAt.Date < today);
		if (overdueCount <= 0)
			return;

		// Nie spamujemy alertem przy każdej nawigacji.
		string todayKey = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		string lastShownKey = Preferences.Get(OverdueAlertDateKey, string.Empty);
		if (string.Equals(lastShownKey, todayKey, StringComparison.Ordinal))
			return;

		int todayCount = _taskRepository.Tasks.Count(t => t.Status == CareTaskStatus.Planned && t.ScheduledAt.Date == today);
		string msg = $"Masz {overdueCount} zaległych zadań opieki.";
		if (todayCount > 0)
			msg += $"\nNa dziś: {todayCount}.";

		await DisplayAlert("Zaległe zadania", msg, "OK");
		Preferences.Set(OverdueAlertDateKey, todayKey);
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
            var wasDone = zad.IsDone;
            zad.IsDone = !zad.IsDone;
            _taskRepository.SaveChanges();

            // Audit trail: wykonanie zadania opieki dla konkretnego zwierzęcia.
            // Rejestrujemy tylko przejście false -> true.
            if (!wasDone && zad.IsDone && zad.AnimalId is Guid animalId)
            {
                var desc = $"Wykonano zadanie: {zad.TypeOpis}. Termin: {zad.ScheduledAtOpis}.";
                if (!string.IsNullOrWhiteSpace(zad.Notes))
                    desc += $" Notatka: {zad.Notes}.";

                _eventService.Log(animalId, AnimalEventType.CareTaskDone, desc);
            }

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
