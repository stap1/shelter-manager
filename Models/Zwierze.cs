namespace ShelterManager.Models;

public class Zwierze : BaseModel
{
    // Pola prywatne (schowki na dane)
    private string _imie = string.Empty;
    private string _rasa = string.Empty;
    private string _status = string.Empty;
    private string _zdjecie = string.Empty;
    
    // --- NOWE POLA ---
    private string _wiek = string.Empty;
    private string _historiaMedyczna = string.Empty;

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

    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }

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
}