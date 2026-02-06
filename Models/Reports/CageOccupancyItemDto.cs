namespace ShelterManager.Models.Reports;

/// <summary>
/// DTO do prezentacji obłożenia pojedynczego boksu.
/// </summary>
public sealed class CageOccupancyItemDto
{
    public int Numer { get; init; }
    public int Capacity { get; init; }
    public int Occupied { get; init; }

    public string OccupancyText => $"{Occupied}/{Capacity}";
}
