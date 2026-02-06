using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace ShelterManager.Models;

/// <summary>
/// Model boksu/klatki.
/// 
/// Wymagania:
/// - Id (dziedziczone z BaseModel)
/// - Numer
/// - Capacity (pojemność)
/// - OccupiedAnimalIds (lista Guid)
/// 
/// Kompatybilność wstecz:
/// wcześniejszy model "Klatka" trzymał pojedynczego lokatora w polu LokatorId.
/// Przy deserializacji starego pliku JSON mapujemy LokatorId do listy OccupiedAnimalIds.
/// </summary>
public sealed class Cage : BaseModel
{
    private int _numer;
    private int _capacity = 1;
    private ObservableCollection<Guid> _occupiedAnimalIds = new();

    public Cage()
    {
        // Gdy lista się zmienia, odświeżamy pochodne właściwości w UI.
        _occupiedAnimalIds.CollectionChanged += OccupiedAnimalIds_CollectionChanged;
    }

    public int Numer
    {
        get => _numer;
        set
        {
            if (_numer != value)
            {
                _numer = value;
                OnPropertyChanged();
            }
        }
    }

    public int Capacity
    {
        get => _capacity;
        set
        {
            int normalized = value < 1 ? 1 : value;
            if (_capacity != normalized)
            {
                _capacity = normalized;
                OnPropertyChanged();
                RaiseOccupancyChanged();
            }
        }
    }

    /// <summary>
    /// Lista Id zwierząt znajdujących się w boksie.
    /// </summary>
    public ObservableCollection<Guid> OccupiedAnimalIds
    {
        get => _occupiedAnimalIds;
        set
        {
            if (ReferenceEquals(_occupiedAnimalIds, value)) return;

            // odpinamy stary handler (musi być ta sama metoda)
            _occupiedAnimalIds.CollectionChanged -= OccupiedAnimalIds_CollectionChanged;

            _occupiedAnimalIds = value ?? new ObservableCollection<Guid>();
            _occupiedAnimalIds.CollectionChanged += OccupiedAnimalIds_CollectionChanged;
            OnPropertyChanged();
            RaiseOccupancyChanged();
        }
    }

    /// <summary>
    /// Pochodne dane dla UI (nie zapisujemy do JSON).
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<Zwierze> OccupiedAnimals { get; } = new();

    [JsonIgnore]
    public int OccupiedCount => OccupiedAnimalIds?.Count ?? 0;

    [JsonIgnore]
    public bool IsEmpty => OccupiedCount == 0;

    [JsonIgnore]
    public string OccupancyText => $"{OccupiedCount}/{Capacity}";

    [JsonIgnore]
    public Zwierze? PrimaryAnimal => OccupiedAnimals.Count > 0 ? OccupiedAnimals[0] : null;

    /// <summary>
    /// Legacy: stare JSON-y mogły zawierać LokatorId (pojedynczy lokator).
    /// Ten setter pozwala bezpiecznie załadować stare dane do nowego modelu.
    /// </summary>
    [JsonProperty("LokatorId")]
    private Guid? LegacyLokatorId
    {
        set
        {
            if (value is Guid id && id != Guid.Empty)
            {
                if (!OccupiedAnimalIds.Contains(id))
                    OccupiedAnimalIds.Add(id);
            }
        }
    }

    private void RaiseOccupancyChanged()
    {
        OnPropertyChanged(nameof(OccupiedCount));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(OccupancyText));
        OnPropertyChanged(nameof(PrimaryAnimal));
    }

    private void OccupiedAnimalIds_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RaiseOccupancyChanged();
    }
}
