namespace ShelterManager.Models;

/// <summary>
/// Transakcja magazynowa (np. zużycie, uzupełnienie).
/// Delta: wartość dodatnia oznacza przyjęcie, ujemna oznacza zużycie.
/// </summary>
public sealed class InventoryTransaction
{
    public Guid ResourceId { get; set; }
    public double Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
