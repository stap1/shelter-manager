using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Wniosek adopcyjny powiązany ze zwierzęciem.
/// 
/// Zasada SRP: klasa przechowuje dane + minimalne właściwości pochodne do UI.
/// Workflow realizujemy w serwisie domenowym (AdoptionWorkflowService).
/// </summary>
public sealed class AdoptionApplication : BaseModel
{
    private Guid _animalId;
    private string _applicantName = string.Empty;
    private string _contact = string.Empty;
    private AdoptionApplicationStatus _status = AdoptionApplicationStatus.New;
    private string _notes = string.Empty;

    private Zwierze? _animal;

    /// <summary>
    /// Id zwierzęcia, którego dotyczy wniosek.
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
                OnPropertyChanged(nameof(AnimalOpis));
            }
        }
    }

    /// <summary>
    /// Imię i nazwisko wnioskodawcy.
    /// </summary>
    public string ApplicantName
    {
        get => _applicantName;
        set
        {
            var v = value ?? string.Empty;
            if (_applicantName != v)
            {
                _applicantName = v;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Dane kontaktowe (telefon/email).
    /// </summary>
    public string Contact
    {
        get => _contact;
        set
        {
            var v = value ?? string.Empty;
            if (_contact != v)
            {
                _contact = v;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Status workflow.
    /// </summary>
    [JsonConverter(typeof(AdoptionApplicationStatusJsonConverter))]
    public AdoptionApplicationStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusOpis));
            }
        }
    }

    /// <summary>
    /// Notatki pracownika (np. wynik rozmowy, uwagi).
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
    /// Powiązany obiekt zwierzęcia dla UI (nie zapisujemy w JSON).
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
    /// Metoda pomocnicza dla ShelterDataStore.
    /// </summary>
    public void SetResolvedAnimal(Zwierze? animal) => Animal = animal;

    // -----------------
    // Pola pochodne do UI
    // -----------------

    [JsonIgnore]
    public string AnimalOpis => Animal?.Imie ?? "Nieznane zwierzę";

    [JsonIgnore]
    public string StatusOpis => Status switch
    {
        AdoptionApplicationStatus.New => "Nowy",
        AdoptionApplicationStatus.InReview => "W weryfikacji",
        AdoptionApplicationStatus.Approved => "Zatwierdzony",
        AdoptionApplicationStatus.Rejected => "Odrzucony",
        _ => "Nowy"
    };

    [JsonIgnore]
    public string CreatedAtOpis => DataUtworzenia.ToString("dd.MM.yyyy HH:mm");
}
