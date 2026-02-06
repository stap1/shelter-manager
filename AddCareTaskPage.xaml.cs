using ShelterManager.Data.Repositories;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager;

public partial class AddCareTaskPage : ContentPage
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAnimalRepository _animalRepository;

    private readonly List<Option<Guid?>> _animalOptions = new();
    private readonly List<Option<CareTaskType>> _typeOptions = new();

    public AddCareTaskPage()
    {
        InitializeComponent();

        _taskRepository = ServiceLocator.GetRequiredService<ITaskRepository>();
        _animalRepository = ServiceLocator.GetRequiredService<IAnimalRepository>();

        DatePickerDate.Date = DateTime.Today;
        TimePickerTime.Time = new TimeSpan(9, 0, 0);

        BuildOptions();
    }

    private void BuildOptions()
    {
        _animalOptions.Clear();
        _typeOptions.Clear();

        _typeOptions.Add(new Option<CareTaskType> { Label = "Inne", Value = CareTaskType.Other });
        _typeOptions.Add(new Option<CareTaskType> { Label = "Karmienie", Value = CareTaskType.Feeding });
        _typeOptions.Add(new Option<CareTaskType> { Label = "Spacer", Value = CareTaskType.Walking });
        _typeOptions.Add(new Option<CareTaskType> { Label = "Sprzątanie", Value = CareTaskType.Cleaning });
        _typeOptions.Add(new Option<CareTaskType> { Label = "Leki", Value = CareTaskType.Medication });
        _typeOptions.Add(new Option<CareTaskType> { Label = "Weterynarz", Value = CareTaskType.VetVisit });
        _typeOptions.Add(new Option<CareTaskType> { Label = "Pielęgnacja", Value = CareTaskType.Grooming });

        _animalOptions.Add(new Option<Guid?> { Label = "Bez zwierzęcia", Value = null });
        foreach (var a in _animalRepository.Animals.Where(z => !z.IsArchived && z.Status != AnimalStatus.Adopted).OrderBy(z => z.Imie))
        {
            _animalOptions.Add(new Option<Guid?> { Label = a.Imie, Value = a.Id });
        }

        PickerType.ItemsSource = _typeOptions;
        PickerAnimal.ItemsSource = _animalOptions;

        if (PickerType.SelectedIndex < 0) PickerType.SelectedIndex = 0;
        if (PickerAnimal.SelectedIndex < 0) PickerAnimal.SelectedIndex = 0;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var type = (PickerType.SelectedItem as Option<CareTaskType>)?.Value ?? CareTaskType.Other;
        var animalId = (PickerAnimal.SelectedItem as Option<Guid?>)?.Value;
        var notes = (EditorNotes.Text ?? string.Empty).Trim();

        // Składamy Date + Time w jedno DateTime.
        var date = DatePickerDate.Date;
        var time = TimePickerTime.Time;
        var scheduled = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);

        var zad = new Zadanie
        {
            ScheduledAt = scheduled,
            Type = type,
            AnimalId = animalId,
            Notes = string.IsNullOrWhiteSpace(notes) ? "(brak opisu)" : notes,
            Status = CareTaskStatus.Planned,
            CompletedAt = null
        };

        _taskRepository.Tasks.Add(zad);
        _taskRepository.SaveChanges();

        await Navigation.PopAsync();
    }

    private sealed class Option<T>
    {
        public string Label { get; init; } = string.Empty;
        public T Value { get; init; } = default!;
        public override string ToString() => Label;
    }
}
