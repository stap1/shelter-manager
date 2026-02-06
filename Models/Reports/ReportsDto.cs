namespace ShelterManager.Models.Reports;

/// <summary>
/// DTO: raport zbiorczy dla zakładki Raporty.
/// Zasada: UI tylko wyświetla dane, a całe liczenie jest w ReportService.
/// </summary>
public sealed class ReportsDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }

    public string PeriodText => string.Format("{0:dd.MM.yyyy} - {1:dd.MM.yyyy}", StartDate, EndDate);
    public string GeneratedAtText { get; init; } = string.Empty;

    public int ActiveAnimalsCount { get; init; }
    public int ArchivedAnimalsCount { get; init; }

    public CageOccupancySummaryDto Cages { get; init; } = new();
    public int AdoptionsApprovedInPeriodCount { get; init; }

    public double TotalConsumedAmountInPeriod { get; init; }
    public string TotalConsumedText { get; init; } = string.Empty;
    public System.Collections.ObjectModel.ObservableCollection<ResourceConsumptionItemDto> ResourceConsumption { get; init; } = new();

    public TasksSummaryDto Tasks { get; init; } = new();
}
