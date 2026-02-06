using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium klatek/boksów.
/// </summary>
public interface ICageRepository
{
    ObservableCollection<Klatka> Cages { get; }
    void SaveChanges();
    void Reload();
}
