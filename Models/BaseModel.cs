using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShelterManager.Models;

public abstract class BaseModel : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DataUtworzenia { get; set; } = DateTime.Now;

    // ZMIANA 1: Dodajemy '?' - event może być pusty (nikt nie nasłuchuje)
    public event PropertyChangedEventHandler? PropertyChanged;

    // ZMIANA 2: Dodajemy '?' przy stringu
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}