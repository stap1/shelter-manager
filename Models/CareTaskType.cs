namespace ShelterManager.Models;

/// <summary>
/// Typ zadania opieki.
/// Enum minimalizuje ryzyko literówek i ułatwia filtrowanie.
/// </summary>
public enum CareTaskType
{
    Other = 0,
    Feeding = 1,
    Walking = 2,
    Cleaning = 3,
    Medication = 4,
    VetVisit = 5,
    Grooming = 6
}
