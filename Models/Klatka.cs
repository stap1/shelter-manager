namespace ShelterManager.Models;

public class Klatka : BaseModel
{
    private string _numer = string.Empty;
    private bool _czyZajeta;
    private Zwierze? _lokator; // Zmieniamy string na obiekt Zwierze!

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

    // Teraz klatka przechowuje całego zwierzaka, więc mamy dostęp do Zdjecia i Rasy
    public Zwierze? Lokator
    {
        get => _lokator;
        set
        {
            if (_lokator != value)
            {
                _lokator = value;
                // Automatycznie ustawiamy flagę, jeśli ktoś tu mieszka
                CzyZajeta = _lokator != null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CzyZajeta));
            }
        }
    }
}