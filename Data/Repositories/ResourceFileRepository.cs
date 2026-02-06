using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium zasobów oparta o plik JSON (przez ShelterDataStore).
/// </summary>
public sealed class ResourceFileRepository : IResourceRepository
{
    private readonly ShelterDataStore _store;

    public ResourceFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<Zasob> Resources => _store.Resources;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
