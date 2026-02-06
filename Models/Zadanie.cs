using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Zadanie opieki (harmonogram).
/// 
/// Uwaga o kompatybilności wstecz:
/// w starszych wersjach Zadanie miało pola Tresc/Godzina/CzyZrobione.
/// Poniżej mapujemy je na nowe pola przez prywatne settery z JsonProperty.
/// </summary>
public sealed class Zadanie : BaseModel
{
    private DateTime _scheduledAt = DateTime.Now;
    private CareTaskType _type = CareTaskType.Other;
    private Guid? _animalId;
    private CareTaskStatus _status = CareTaskStatus.Planned;
    private DateTime? _completedAt;
    private string _notes = string.Empty;

    private Zwierze? _animal;

    /// <summary>
    /// Planowany termin wykonania.
    /// </summary>
    public DateTime ScheduledAt
    {
        get => _scheduledAt;
        set
        {
            if (_scheduledAt != value)
            {
                _scheduledAt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ScheduledAtOpis));
            }
        }
    }

    /// <summary>
    /// Typ czynności opieki.
    /// </summary>
    public CareTaskType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TypeOpis));
            }
        }
    }

    /// <summary>
    /// Id zwierzęcia powiązanego z zadaniem (opcjonalne).
    /// </summary>
    public Guid? AnimalId
    {
        get => _animalId;
        set
        {
            if (_animalId != value)
            {
                _animalId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AnimalOpis));
            }
        }
    }

    /// <summary>
    /// Status zadania.
    /// </summary>
    public CareTaskStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;

                // Spójność: CompletedAt ma sens tylko dla Done.
                if (_status == CareTaskStatus.Done)
                    _completedAt ??= DateTime.UtcNow;
                else
                    _completedAt = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusOpis));
                OnPropertyChanged(nameof(IsDone));
                OnPropertyChanged(nameof(IsCancelled));
            }
        }
    }

    /// <summary>
    /// Data zakończenia (uzupełniana automatycznie po oznaczeniu jako Done).
    /// </summary>
    public DateTime? CompletedAt
    {
        get => _completedAt;
        set
        {
            if (_completedAt != value)
            {
                _completedAt = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Dodatkowy opis/powód.
    /// </summary>
    public string Notes
    {
        get => _notes;
        set
        {
            var v = value ?? string.Empty;
            if (_notes != v)
            {
                _notes = v;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Obiekt zwierzęcia dla UI (nie zapisujemy w JSON).
    /// Uzupełnia go ShelterDataStore.
    /// </summary>
    [JsonIgnore]
    public Zwierze? Animal
    {
        get => _animal;
        private set
        {
            if (!ReferenceEquals(_animal, value))
            {
                _animal = value;
                OnPropertyChanged(nameof(Animal));
                OnPropertyChanged(nameof(AnimalOpis));
            }
        }
    }

    /// <summary>
    /// Metoda pomocnicza do ustawienia Animal z poziomu data store.
    /// </summary>
    public void SetResolvedAnimal(Zwierze? animal) => Animal = animal;

    // -----------------
    // Pola pochodne do UI
    // -----------------

    [JsonIgnore]
    public string ScheduledAtOpis => ScheduledAt.ToString("dd.MM.yyyy HH:mm");

    [JsonIgnore]
    public string TypeOpis => Type switch
    {
        CareTaskType.Feeding => "Karmienie",
        CareTaskType.Walking => "Spacer",
        CareTaskType.Cleaning => "Sprzątanie",
        CareTaskType.Medication => "Leki",
        CareTaskType.VetVisit => "Weterynarz",
        CareTaskType.Grooming => "Pielęgnacja",
        _ => "Inne"
    };

    [JsonIgnore]
    public string StatusOpis => Status switch
    {
        CareTaskStatus.Planned => "Planowane",
        CareTaskStatus.Done => "Wykonane",
        CareTaskStatus.Cancelled => "Anulowane",
        _ => "Planowane"
    };

    [JsonIgnore]
    public string AnimalOpis
    {
        get
        {
            if (Animal is not null) return Animal.Imie;
            return AnimalId.HasValue ? "Nieznane zwierzę" : "Bez zwierzęcia";
        }
    }

    [JsonIgnore]
    public bool IsDone
    {
        get => Status == CareTaskStatus.Done;
        set
        {
            // UI (CheckBox) steruje stanem.
            if (value)
                MarkDone();
            else
                SetPlanned();
        }
    }

    [JsonIgnore]
    public bool IsCancelled => Status == CareTaskStatus.Cancelled;

    public void MarkDone()
    {
        Status = CareTaskStatus.Done;
        CompletedAt ??= DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = CareTaskStatus.Cancelled;
        CompletedAt = null;
    }

    public void SetPlanned()
    {
        Status = CareTaskStatus.Planned;
        CompletedAt = null;
    }

    // -----------------
    // Legacy mapping (stare JSON)
    // -----------------

    [JsonProperty("Tresc")]
    private string? LegacyTresc
    {
        set
        {
            // W starym modelu tresc była główną informacją.
            // W nowym przechowujemy to w Notes.
            if (!string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(Notes))
                Notes = value.Trim();
        }
    }

    [JsonProperty("Godzina")]
    private string? LegacyGodzina
    {
        set
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            // Spróbuj wczytać HH:mm i ustawić czas na dzisiaj.
            if (TimeSpan.TryParse(value.Trim(), out var ts))
            {
                var d = DateTime.Today.Add(ts);
                ScheduledAt = d;
            }
        }
    }

    [JsonProperty("CzyZrobione")]
    private bool? LegacyCzyZrobione
    {
        set
        {
            if (value == true)
                MarkDone();
        }
    }
}
