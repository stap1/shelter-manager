using System.Collections.ObjectModel;

namespace ShelterManager.Models.Reports;

/// <summary>
/// Podsumowanie obłożenia wszystkich boksów.
/// UI ma jedynie prezentować wartości, a nie je wyliczać.
/// </summary>
public sealed class CageOccupancySummaryDto
{
    public int TotalCages { get; init; }
    public int TotalCapacity { get; init; }
    public int TotalOccupied { get; init; }

    /// <summary>
    /// Wartość 0..100.
    /// </summary>
    public double OccupancyPercent { get; init; }

    public string OccupancyPercentText => $"{OccupancyPercent:0.#}%";

    public ObservableCollection<CageOccupancyItemDto> Cages { get; init; } = new();
}
