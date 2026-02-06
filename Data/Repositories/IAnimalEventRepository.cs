using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium rejestru zdarzeń zwierząt (audit trail).
///
/// Zasada DIP: UI i serwisy pracują na interfejsie, a nie na konkretnej implementacji plikowej.
/// </summary>
public interface IAnimalEventRepository
{
    ObservableCollection<AnimalEvent> AnimalEvents { get; }

    void SaveChanges();
    void Reload();
}
