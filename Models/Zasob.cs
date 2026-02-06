namespace ShelterManager.Models;

public class Zasob : BaseModel
{
    public string Nazwa { get; set; } = string.Empty;
    public string Jednostka { get; set; } = string.Empty;

    private double _lowStockThreshold = 10;

    /// <summary>
    /// Próg niskiego stanu. Powiadomienie jest generowane, gdy ilość spadnie poniżej tej wartości.
    /// Domyślnie: 10.
    /// </summary>
    public double LowStockThreshold
    {
        get => _lowStockThreshold;
        set
        {
            var normalized = value;
            if (normalized < 0) normalized = 0;

            if (Math.Abs(_lowStockThreshold - normalized) > 0.0001)
            {
                _lowStockThreshold = normalized;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsLowStock));
            }
        }
    }

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
                OnPropertyChanged(nameof(IsLowStock));
            }
        }
    }

    /// <summary>
    /// Czy ilość spadła poniżej progu.
    /// </summary>
    public bool IsLowStock => Ilosc > 0 && Ilosc < LowStockThreshold;

    public string Status
    {
        get
        {
            if (Ilosc <= 0) return "BRAK!";
            if (Ilosc < LowStockThreshold) return "MAŁO";
            return "OK";
        }
    }
}