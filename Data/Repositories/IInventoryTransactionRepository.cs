using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium transakcji magazynowych.
/// Utrzymujemy osobną kolekcję, aby nie mieszać logiki zasobów z logiką historii zmian.
/// </summary>
public interface IInventoryTransactionRepository
{
    ObservableCollection<InventoryTransaction> Transactions { get; }
    void SaveChanges();
    void Reload();
}
