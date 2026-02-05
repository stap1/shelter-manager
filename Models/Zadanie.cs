using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShelterManager.Models;

public class Zadanie : INotifyPropertyChanged
{
    public string Tresc { get; set; } = "";
    public string Godzina { get; set; } = "";

    // Prywatne pole do przechowywania wartości
    private bool _czyZrobione;

    // Publiczna właściwość z powiadamianiem
    public bool CzyZrobione
    {
        get => _czyZrobione;
        set
        {
            if (_czyZrobione != value)
            {
                _czyZrobione = value;
                // To magiczna linijka, która mówi widokowi: "Hej, zmieniłem się! Odśwież checkboxa!"
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}