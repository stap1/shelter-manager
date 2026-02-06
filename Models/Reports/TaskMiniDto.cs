namespace ShelterManager.Models.Reports;

/// <summary>
/// Skrócona reprezentacja zadania do raportów.
/// </summary>
public sealed class TaskMiniDto
{
    public string WhenText { get; init; } = string.Empty;
    public string TypeText { get; init; } = string.Empty;
    public string AnimalText { get; init; } = string.Empty;
    public string NotesText { get; init; } = string.Empty;
}
