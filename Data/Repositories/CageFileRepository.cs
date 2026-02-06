using System.Collections.ObjectModel;
using ShelterManager.Data;
using ShelterManager.Models;

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

    public ObservableCollection<Klatka> Cages => _store.Cages;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
