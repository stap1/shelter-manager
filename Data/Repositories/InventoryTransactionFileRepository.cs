using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Implementacja repozytorium transakcji magazynowych oparta o plik JSON (przez ShelterDataStore).
/// </summary>
public sealed class InventoryTransactionFileRepository : IInventoryTransactionRepository
{
    private readonly ShelterDataStore _store;

    public InventoryTransactionFileRepository(ShelterDataStore store)
    {
        _store = store;
    }

    public ObservableCollection<InventoryTransaction> Transactions => _store.InventoryTransactions;

    public void SaveChanges() => _store.SaveChanges();

    public void Reload() => _store.Reload();
}
