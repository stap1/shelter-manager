using ShelterManager.Models;
using System.Collections.ObjectModel;

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
