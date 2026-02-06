namespace ShelterManager.Models;

/// <summary>
/// Typ zdarzenia w rejestrze (audit trail) zwierzęcia.
///
/// Enum zamiast stringów eliminuje literówki i ułatwia filtrowanie.
/// </summary>
public enum AnimalEventType
{
    Unknown = 0,
    Created = 1,
    Edited = 2,
    Archived = 3,
    Restored = 4,
    StatusChanged = 5,
    CageAssigned = 6,
    CageMoved = 7,
    CageRemoved = 8,
    AdoptionApproved = 9,
    CareTaskDone = 10
}
