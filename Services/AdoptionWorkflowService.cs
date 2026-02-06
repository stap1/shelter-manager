using ShelterManager.Data.Repositories;
using ShelterManager.Models;

namespace ShelterManager.Services;

/// <summary>
/// Serwis domenowy obsługujący workflow wniosków adopcyjnych.
/// 
/// Zasada SRP: UI (strony) tylko wywołuje operacje, a reguły biznesowe + efekty uboczne
/// (zmiana statusu zwierzęcia, zdjęcie z boksu, wpis do historii) są w jednym miejscu.
/// </summary>
public sealed class AdoptionWorkflowService
{
    private readonly IAdoptionApplicationRepository _applicationRepository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly CageAllocationService _cageAllocationService;

    public AdoptionWorkflowService(
        IAdoptionApplicationRepository applicationRepository,
        IAnimalRepository animalRepository,
        ITaskRepository taskRepository,
        CageAllocationService cageAllocationService)
    {
        _applicationRepository = applicationRepository;
        _animalRepository = animalRepository;
        _taskRepository = taskRepository;
        _cageAllocationService = cageAllocationService;
    }

    public OperationResult SetStatus(AdoptionApplication application, AdoptionApplicationStatus newStatus)
    {
        if (application is null)
            return OperationResult.Fail("Brak wniosku.");

        // Prosta ochrona przed "przestawianiem" zakończonych spraw.
        if (application.Status is AdoptionApplicationStatus.Approved or AdoptionApplicationStatus.Rejected)
        {
            if (application.Status != newStatus)
                return OperationResult.Fail("Nie można zmienić statusu zakończonego wniosku.");
        }

        if (newStatus == AdoptionApplicationStatus.Approved)
            return Approve(application);

        // Pozostałe przejścia: New <-> InReview, Rejected.
        application.Status = newStatus;
        _applicationRepository.SaveChanges();
        return OperationResult.Ok();
    }

    private OperationResult Approve(AdoptionApplication application)
    {
        // 1) Walidacje biznesowe
        var animal = _animalRepository.Animals.FirstOrDefault(a => a.Id == application.AnimalId);
        if (animal is null)
            return OperationResult.Fail("Nie znaleziono zwierzęcia powiązanego z wnioskiem.");

        if (animal.IsArchived)
            return OperationResult.Fail("Nie można zatwierdzić wniosku dla zarchiwizowanego zwierzęcia.");

        if (animal.Status == AnimalStatus.Adopted)
            return OperationResult.Fail("Zwierzę ma już status Adopted.");

        // 2) Efekty uboczne wymagane w zadaniu
        // 2a) Status wniosku
        application.Status = AdoptionApplicationStatus.Approved;

        // 2b) Aktualizacja zwierzęcia
        animal.Status = AnimalStatus.Adopted;

        // 2c) Zdjęcie z boksu (spójność danych)
        _cageAllocationService.RemoveAnimalFromCage(animal.Id);

        // 2d) Odpięcie od harmonogramu opieki (reguła: Adopted nie powinien być przypisany do zadań)
        bool tasksChanged = false;
        foreach (var t in _taskRepository.Tasks)
        {
            if (t.AnimalId == animal.Id)
            {
                t.AnimalId = null;
                t.SetResolvedAnimal(null);
                tasksChanged = true;
            }
        }

        // 2e) Wpis do historii (wykorzystujemy istniejące pole HistoriaMedyczna)
        AppendHistoryEntry(animal, $"Adopcja zatwierdzona. Wnioskodawca: {application.ApplicantName}. Kontakt: {application.Contact}.");

        // 3) Persystencja
        // Uwaga: repozytoria finalnie zapisują jeden plik, ale zachowujemy czytelność.
        _animalRepository.SaveChanges();
        if (tasksChanged)
            _taskRepository.SaveChanges();
        _applicationRepository.SaveChanges();

        return OperationResult.Ok();
    }

    private static void AppendHistoryEntry(Zwierze animal, string entry)
    {
        // Uwaga: HistoriaMedyczna jest stringiem, więc dopisujemy kolejne linie.
        // Format: [YYYY-MM-DD HH:mm] ...
        var stamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm");
        var line = $"[{stamp}] {entry}";

        if (string.IsNullOrWhiteSpace(animal.HistoriaMedyczna) || animal.HistoriaMedyczna == "Brak wpisów")
            animal.HistoriaMedyczna = line;
        else
            animal.HistoriaMedyczna = animal.HistoriaMedyczna.TrimEnd() + "\n" + line;
    }
}

/// <summary>
/// Prosty wynik operacji (bez wyjątków w UI).
/// </summary>
public sealed record OperationResult(bool Success, string? Error)
{
    public static OperationResult Ok() => new(true, null);
    public static OperationResult Fail(string error) => new(false, error);
}
