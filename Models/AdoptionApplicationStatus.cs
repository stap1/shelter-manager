namespace ShelterManager.Models;

/// <summary>
/// Status wniosku adopcyjnego.
/// Enum eliminuje literówki i upraszcza filtrowanie.
/// </summary>
public enum AdoptionApplicationStatus
{
    New = 0,
    InReview = 1,
    Approved = 2,
    Rejected = 3
}
