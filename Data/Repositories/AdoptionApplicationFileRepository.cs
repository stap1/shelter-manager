using System.Collections.ObjectModel;
using ShelterManager.Data;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium wniosków adopcyjnych oparta o ShelterDataStore.
/// </summary>
public sealed class AdoptionApplicationFileRepository : IAdoptionApplicationRepository
{
    private readonly ShelterDataStore _store;

    public AdoptionApplicationFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<AdoptionApplication> Applications => _store.AdoptionApplications;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
