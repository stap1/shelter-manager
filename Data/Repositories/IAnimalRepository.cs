using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium zwierząt (warstwa dostępu do danych).
/// Interfejs ułatwia testowanie i spełnia zasadę DIP (Dependency Inversion Principle).
/// </summary>
public interface IAnimalRepository
{
    ObservableCollection<Zwierze> Animals { get; }
    void SaveChanges();
    void Reload();
}
