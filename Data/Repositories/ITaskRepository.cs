using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium zadań.
/// </summary>
public interface ITaskRepository
{
    ObservableCollection<Zadanie> Tasks { get; }
    void SaveChanges();
    void Reload();
}
