using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data;

/// <summary>
/// DTO do serializacji całej "bazy" do jednego pliku JSON.
/// Zgodnie z zasadą SRP (Single Responsibility Principle), klasa zawiera tylko dane.
/// </summary>
internal sealed class ShelterDbDto
{
    public int SchemaVersion { get; set; } = 6;

    public ObservableCollection<Zwierze> Animals { get; set; } = new();
    public ObservableCollection<Cage> Cages { get; set; } = new();
    public ObservableCollection<Zasob> Resources { get; set; } = new();
    public ObservableCollection<InventoryTransaction> InventoryTransactions { get; set; } = new();
    public ObservableCollection<Zadanie> Tasks { get; set; } = new();

    // Rejestr zdarzeń zwierząt (audit trail)
    public ObservableCollection<AnimalEvent> AnimalEvents { get; set; } = new();

    public ObservableCollection<AdoptionApplication> AdoptionApplications { get; set; } = new();
}
