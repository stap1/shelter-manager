using System.Collections.ObjectModel;
using ShelterManager.Data;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium zadań oparta o plik JSON (przez ShelterDataStore).
/// </summary>
public sealed class TaskFileRepository : ITaskRepository
{
    private readonly ShelterDataStore _store;

    public TaskFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<Zadanie> Tasks => _store.Tasks;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
