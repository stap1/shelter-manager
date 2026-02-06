using ShelterManager.Data.Repositories;
using ShelterManager.Models;
using System.Linq;

namespace ShelterManager.Services;

/// <summary>
/// Serwis odpowiedzialny za przydzielanie zwierząt do boksów.
///
/// Wymagania:
/// - nie przekraczaj Capacity,
/// - nie przydzielaj zwierząt Adopted ani Archived,
/// - zapisuj przydział w danych (OccupiedAnimalIds w Cage).
///
/// Uwaga: serwis operuje na kolekcjach repozytoriów (ObservableCollection),
/// więc UI dostaje zmiany "na żywo".
/// </summary>
public sealed class CageAllocationService
{
    private readonly IAnimalRepository _animalRepository;
    private readonly ICageRepository _cageRepository;

    public CageAllocationService(IAnimalRepository animalRepository, ICageRepository cageRepository)
    {
        _animalRepository = animalRepository;
        _cageRepository = cageRepository;
    }

    /// <summary>
    /// Przydziela zwierzę do wybranego boksu.
    /// Jeśli zwierzę było już w innym boksie, zostanie z niego usunięte.
    /// </summary>
    public AllocationResult AssignAnimalToCage(Guid animalId, Guid cageId)
    {
        var animal = FindAnimal(animalId);
        if (animal is null)
            return AllocationResult.Fail("Nie znaleziono zwierzęcia.");

        if (!IsEligibleForAllocation(animal))
            return AllocationResult.Fail("Nie można przydzielić zwierzęcia o statusie Adopted lub zarchiwizowanego.");

        var targetCage = FindCage(cageId);
        if (targetCage is null)
            return AllocationResult.Fail("Nie znaleziono boksu.");

        // Jeżeli już jest w tym boksie, traktujemy jako OK.
        if (targetCage.OccupiedAnimalIds.Contains(animalId))
            return AllocationResult.Ok();

        // Usuwamy z innych boksów (zwierzę nie może być w dwóch miejscach).
        RemoveAnimalFromAllCagesInternal(animalId);

        // Walidacja pojemności.
        if (targetCage.OccupiedAnimalIds.Count >= targetCage.Capacity)
            return AllocationResult.Fail("Brak miejsca w wybranym boksie (osiągnięto Capacity).");

        // Najpierw podpinamy obiekt do listy UI (OccupiedAnimals), potem Id.
        AddToCageInternal(targetCage, animal);

        _cageRepository.SaveChanges();
        return AllocationResult.Ok();
    }

    /// <summary>
    /// Przenosi zwierzę do innego boksu.
    /// </summary>
    public AllocationResult MoveAnimal(Guid animalId, Guid targetCageId)
    {
        // Move to tak naprawdę Assign + walidacje.
        return AssignAnimalToCage(animalId, targetCageId);
    }

    /// <summary>
    /// Usuwa zwierzę z boksu (jeśli było przydzielone).
    /// </summary>
    public AllocationResult RemoveAnimalFromCage(Guid animalId)
    {
        var animal = FindAnimal(animalId);
        if (animal is null)
            return AllocationResult.Fail("Nie znaleziono zwierzęcia.");

        bool changed = RemoveAnimalFromAllCagesInternal(animalId);
        if (changed)
            _cageRepository.SaveChanges();

        return AllocationResult.Ok();
    }

    /// <summary>
    /// Zwraca aktualny boks, w którym znajduje się zwierzę (jeśli jest przydzielone).
    /// </summary>
    public Cage? FindCageOfAnimal(Guid animalId)
        => _cageRepository.Cages.FirstOrDefault(c => c.OccupiedAnimalIds.Contains(animalId));

    private Zwierze? FindAnimal(Guid id)
        => _animalRepository.Animals.FirstOrDefault(a => a.Id == id);

    private Cage? FindCage(Guid id)
        => _cageRepository.Cages.FirstOrDefault(c => c.Id == id);

    private static bool IsEligibleForAllocation(Zwierze animal)
        => animal.Status != AnimalStatus.Adopted && !animal.IsArchived;

    private bool RemoveAnimalFromAllCagesInternal(Guid animalId)
    {
        bool changed = false;

        foreach (var cage in _cageRepository.Cages)
        {
            if (!cage.OccupiedAnimalIds.Contains(animalId))
                continue;

            // Najpierw zdejmujemy obiekt z listy UI.
            var existing = cage.OccupiedAnimals.FirstOrDefault(a => a.Id == animalId);
            if (existing is not null)
                cage.OccupiedAnimals.Remove(existing);

            cage.OccupiedAnimalIds.Remove(animalId);
            changed = true;
        }

        return changed;
    }

    private static void AddToCageInternal(Cage cage, Zwierze animal)
    {
        if (!cage.OccupiedAnimals.Any(a => a.Id == animal.Id))
            cage.OccupiedAnimals.Add(animal);

        if (!cage.OccupiedAnimalIds.Contains(animal.Id))
            cage.OccupiedAnimalIds.Add(animal.Id);
    }
}

/// <summary>
/// Prosty wynik operacji, żeby UI mogło pokazać komunikat zamiast łapać wyjątki.
/// </summary>
public sealed record AllocationResult(bool Success, string? Error)
{
    public static AllocationResult Ok() => new(true, null);
    public static AllocationResult Fail(string error) => new(false, error);
}
