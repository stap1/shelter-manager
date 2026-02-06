namespace ShelterManager.Models;

public class Zwierze : BaseModel
{
    // Pola prywatne (schowki na dane)
    private string _imie = string.Empty;
    private AnimalSpecies _gatunek = AnimalSpecies.Unknown;
    private string _rasa = string.Empty;
    private AnimalStatus _status = AnimalStatus.Unknown;
    private string _zdjecie = string.Empty;
    
    // --- NOWE POLA ---
    private string _wiek = string.Empty;
    private string _historiaMedyczna = string.Empty;

    // Soft-delete (Archiwum) - przydaje się w wymaganiach o archiwizacji.
    // Domyślnie false, żeby nie psuć istniejących danych.
    private bool _isArchived;

    // Data archiwizacji (UTC). Null oznacza, że zwierzę nie jest w archiwum.
    private DateTime? _archivedAt;

    // Właściwości publiczne z powiadomieniami (OnPropertyChanged)
    
    public string Imie
    {
        get => _imie;
        set
        {
            if (_imie != value)
            {
                _imie = value;
                OnPropertyChanged(); 
            }
        }
    }

    public string Rasa
    {
        get => _rasa;
        set
        {
            if (_rasa != value)
            {
                _rasa = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gatunek zwierzęcia.
    /// Zapisywany w JSON jako enum name, ale wczytuje też legacy PL ("Pies"/"Kot"/"Inne").
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(AnimalSpeciesJsonConverter))]
    public AnimalSpecies Gatunek
    {
        get => _gatunek;
        set
        {
            if (_gatunek != value)
            {
                _gatunek = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GatunekOpis));
            }
        }
    }

    /// <summary>
    /// Tekst do UI (nie zapisujemy do JSON).
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public string GatunekOpis => Gatunek switch
    {
        AnimalSpecies.Dog => "Pies",
        AnimalSpecies.Cat => "Kot",
        AnimalSpecies.Other => "Inne",
        _ => "Nieznany"
    };

    /// <summary>
    /// Status zwierzęcia.
    /// Zapisywany w JSON jako enum name, ale wczytuje też legacy PL ("Kwarantanna" itd.).
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(AnimalStatusJsonConverter))]
    public AnimalStatus Status
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
    /// Tekst do UI (nie zapisujemy do JSON).
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public string StatusOpis => Status switch
    {
        AnimalStatus.Quarantine => "Kwarantanna",
        AnimalStatus.Treatment => "W leczeniu",
        AnimalStatus.ForAdoption => "Do adopcji",
        AnimalStatus.Adopted => "Adoptowany",
        _ => "Nieznany"
    };

    public string Zdjecie
    {
        get => _zdjecie;
        set
        {
            if (_zdjecie != value)
            {
                _zdjecie = value;
                OnPropertyChanged();
            }
        }
    }

    // --- NOWE WŁAŚCIWOŚCI (Dla punktów 1 i 6) ---

    public string Wiek
    {
        get => _wiek;
        set
        {
            if (_wiek != value)
            {
                _wiek = value;
                OnPropertyChanged();
            }
        }
    }

    public string HistoriaMedyczna
    {
        get => _historiaMedyczna;
        set
        {
            if (_historiaMedyczna != value)
            {
                _historiaMedyczna = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Czy zwierzę jest zarchiwizowane (soft delete).
    /// Uwaga: logika wyświetlania archiwum może być dopięta osobno.
    /// </summary>
    public bool IsArchived
    {
        get => _isArchived;
        set
        {
            if (_isArchived != value)
            {
                _isArchived = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Kiedy zwierzę zostało zarchiwizowane (soft delete).
    /// Używamy UTC, żeby dane były jednoznaczne niezależnie od strefy czasowej.
    /// </summary>
    public DateTime? ArchivedAt
    {
        get => _archivedAt;
        set
        {
            if (_archivedAt != value)
            {
                _archivedAt = value;
                OnPropertyChanged();
            }
        }
    }
}