using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium zasobów magazynowych.
/// </summary>
public interface IResourceRepository
{
    ObservableCollection<Zasob> Resources { get; }
    void SaveChanges();
    void Reload();
}
