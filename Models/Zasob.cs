namespace ShelterManager.Models;

public class Zasob : BaseModel
{
    public string Nazwa { get; set; } = string.Empty;
    public string Jednostka { get; set; } = string.Empty;

    private double _ilosc;
    public double Ilosc
    {
        get => _ilosc;
        set
        {
            if (_ilosc != value)
            {
                _ilosc = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    public string Status
    {
        get
        {
            if (Ilosc <= 0) return "BRAK!";
            if (Ilosc < 10) return "MAŁO";
            return "OK";
        }
    }
}