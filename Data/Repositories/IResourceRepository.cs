using System.Collections.ObjectModel;
using ShelterManager.Models;

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
