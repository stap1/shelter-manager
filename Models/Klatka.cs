namespace ShelterManager.Models;

public class Klatka : BaseModel
{
    private string _numer = string.Empty;
    private bool _czyZajeta;
    private string _mieszkaniec = "Pusta";
    private string _kolorStatusu = "Gray"; // Dodatkowy bajer wizualny

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

    public string Mieszkaniec
    {
        get => _mieszkaniec;
        set { if (_mieszkaniec != value) { _mieszkaniec = value; OnPropertyChanged(); } }
    }

    // To pozwoli nam kolorować klatkę: Czerwona (zajęta) lub Zielona (wolna)
    public string KolorStatusu
    {
        get => _kolorStatusu;
        set { if (_kolorStatusu != value) { _kolorStatusu = value; OnPropertyChanged(); } }
    }
}