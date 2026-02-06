using System.Collections.ObjectModel;
using ShelterManager.Data;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium zdarzeń oparta o ShelterDataStore (jeden plik JSON).
/// </summary>
public sealed class AnimalEventFileRepository : IAnimalEventRepository
{
    private readonly ShelterDataStore _store;

    public AnimalEventFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<AnimalEvent> AnimalEvents => _store.AnimalEvents;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
