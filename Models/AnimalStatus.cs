namespace ShelterManager.Models;

/// <summary>
/// Status zwierzęcia w schronisku.
/// Enum usuwa ryzyko literówek (np. "Kwarantanna" vs "Kwarantana").
/// </summary>
public enum AnimalStatus
{
    Unknown = 0,
    Quarantine = 1,
    Treatment = 2,
    ForAdoption = 3,
    Adopted = 4
}
