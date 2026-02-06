namespace ShelterManager.Models.Reports;

/// <summary>
/// DTO: zużycie konkretnego zasobu w wybranym okresie.
/// </summary>
public sealed class ResourceConsumptionItemDto
{
    public string ResourceName { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;

    /// <summary>
    /// Łączna ilość zużyta (zawsze dodatnia).
    /// </summary>
    public double ConsumedAmount { get; init; }

    public string ConsumedText => string.IsNullOrWhiteSpace(Unit)
        ? $"{ConsumedAmount:0.##}"
        : $"{ConsumedAmount:0.##} {Unit}";
}
