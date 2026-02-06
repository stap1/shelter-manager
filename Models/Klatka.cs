namespace ShelterManager.Models;

public class Klatka : BaseModel
{
    private string _numer = string.Empty;
    private bool _czyZajeta;
    private Zwierze? _lokator; // Obiekt zwierzęcia dla UI

    /// <summary>
    /// Id lokatora trzymamy w pliku JSON, żeby nie duplikować danych zwierzęcia w różnych miejscach.
    /// Dzięki temu, gdy edytujesz zwierzę, klatka nadal wskazuje na to samo Id.
    /// </summary>
    public Guid? LokatorId { get; set; }

    public string Numer
    {
        get => _numer;
        set { if (_numer != value) { _numer = value; OnPropertyChanged(); } }
    }

    public bool CzyZajeta
    {
        get => _czyZajeta;
        set { if (_czyZajeta != value) { _czyZajeta = value; OnPropertyChanged(); } }
    }

    // Lokator (obiekt) jest używany w UI, ale do zapisu do JSON używamy LokatorId.
    // JsonIgnore to "standard" dla pól pochodnych / wyliczanych w modelu.
    [Newtonsoft.Json.JsonIgnore]
    public Zwierze? Lokator
    {
        get => _lokator;
        set
        {
            if (_lokator != value)
            {
                _lokator = value;
                LokatorId = _lokator?.Id;
                // Automatycznie ustawiamy flagę, jeśli ktoś tu mieszka
                CzyZajeta = _lokator != null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CzyZajeta));
            }
        }
    }
}