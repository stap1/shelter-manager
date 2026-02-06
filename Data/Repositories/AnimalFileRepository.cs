using System.Collections.ObjectModel;
using ShelterManager.Data;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium zwierząt oparta o plik JSON (przez ShelterDataStore).
/// </summary>
public sealed class AnimalFileRepository : IAnimalRepository
{
    private readonly ShelterDataStore _store;

    public AnimalFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<Zwierze> Animals => _store.Animals;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
