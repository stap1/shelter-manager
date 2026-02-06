using System.Collections.ObjectModel;

namespace ShelterManager.Models.Reports;

/// <summary>
/// DTO: podsumowanie zadań na dziś i zaległych.
/// </summary>
public sealed class TasksSummaryDto
{
    public int TodayPlannedCount { get; init; }
    public int OverduePlannedCount { get; init; }

    public ObservableCollection<TaskMiniDto> TodayTasks { get; init; } = new();
    public ObservableCollection<TaskMiniDto> OverdueTasks { get; init; } = new();
}
