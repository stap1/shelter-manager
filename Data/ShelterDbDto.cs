using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager.Data;

/// <summary>
/// DTO do serializacji całej "bazy" do jednego pliku JSON.
/// Zgodnie z zasadą SRP (Single Responsibility Principle), klasa zawiera tylko dane.
/// </summary>
internal sealed class ShelterDbDto
{
    public int SchemaVersion { get; set; } = 3;

    public ObservableCollection<Zwierze> Animals { get; set; } = new();
    public ObservableCollection<Cage> Cages { get; set; } = new();
    public ObservableCollection<Zasob> Resources { get; set; } = new();
    public ObservableCollection<InventoryTransaction> InventoryTransactions { get; set; } = new();
    public ObservableCollection<Zadanie> Tasks { get; set; } = new();
}
