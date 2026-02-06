using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium klatek oparta o plik JSON (przez ShelterDataStore).
/// </summary>
public sealed class CageFileRepository : ICageRepository
{
    private readonly ShelterDataStore _store;

    public CageFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<Cage> Cages => _store.Cages;

    public void AddCage(Cage cage)
    {
        if (cage is null) return;
        _store.Cages.Add(cage);
        _store.SaveChanges();
    }

    public bool TryRemoveCage(Cage cage)
    {
        if (cage is null) return false;

        // Usuń tylko pusty boks.
        if (cage.OccupiedAnimalIds.Count > 0)
            return false;

        bool removed = _store.Cages.Remove(cage);
        if (removed) _store.SaveChanges();
        return removed;
    }

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
