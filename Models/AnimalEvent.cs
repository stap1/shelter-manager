using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Zdarzenie w rejestrze historii (audit trail) zwierzęcia.
///
/// Zgodnie z dobrymi praktykami (SRP), obiekt trzyma dane zdarzenia.
/// Logika tworzenia wpisów jest w osobnym serwisie (AnimalEventService).
/// </summary>
public sealed class AnimalEvent : BaseModel
{
    private Guid _animalId;
    private AnimalEventType _type = AnimalEventType.Unknown;
    private DateTime _timestamp = DateTime.UtcNow;
    private string _description = string.Empty;

    /// <summary>
    /// Id zwierzęcia, którego dotyczy zdarzenie.
    /// </summary>
    public Guid AnimalId
    {
        get => _animalId;
        set
        {
            if (_animalId != value)
            {
                _animalId = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Typ zdarzenia (enum, zapis jako string w JSON).
    /// </summary>
    [JsonConverter(typeof(AnimalEventTypeJsonConverter))]
    public AnimalEventType Type
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
    /// Czas zdarzenia (UTC).
    /// </summary>
    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            if (_timestamp != value)
            {
                _timestamp = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimestampOpis));
            }
        }
    }

    /// <summary>
    /// Opis zdarzenia widoczny dla użytkownika.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            var v = value ?? string.Empty;
            if (_description != v)
            {
                _description = v;
                OnPropertyChanged();
            }
        }
    }

    // -----------------
    // Pola pochodne do UI
    // -----------------

    [JsonIgnore]
    public string TimestampOpis => Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

    [JsonIgnore]
    public string TypeOpis => Type switch
    {
        AnimalEventType.Created => "Utworzenie",
        AnimalEventType.Edited => "Edycja",
        AnimalEventType.Archived => "Archiwizacja",
        AnimalEventType.Restored => "Przywrócenie",
        AnimalEventType.StatusChanged => "Zmiana statusu",
        AnimalEventType.CageAssigned => "Przydział boksu",
        AnimalEventType.CageMoved => "Zmiana boksu",
        AnimalEventType.CageRemoved => "Zdjęcie z boksu",
        AnimalEventType.AdoptionApproved => "Adopcja",
        AnimalEventType.CareTaskDone => "Wykonanie zadania",
        _ => "Zdarzenie"
    };
}
