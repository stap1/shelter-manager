using ShelterManager.Data.Repositories;
using ShelterManager.Models;

namespace ShelterManager.Services;

/// <summary>
/// Serwis domenowy do rejestrowania zdarzeń zwierząt (audit trail).
///
/// Zasady:
/// - zapisujemy minimalny opis dla użytkownika,
/// - Timestamp w UTC, a w UI prezentujemy LocalTime,
/// - serwis zapisuje zmiany przez repozytorium (jeden plik JSON).
/// </summary>
public sealed class AnimalEventService
{
    private readonly IAnimalEventRepository _eventRepository;

    public AnimalEventService(IAnimalEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Dodaje wpis do rejestru zdarzeń dla konkretnego zwierzęcia.
    /// </summary>
    public void Log(Guid animalId, AnimalEventType type, string description, DateTime? timestampUtc = null)
    {
        var ev = new AnimalEvent
        {
            AnimalId = animalId,
            Type = type,
            Timestamp = timestampUtc ?? DateTime.UtcNow,
            Description = description ?? string.Empty
        };

        _eventRepository.AnimalEvents.Add(ev);
        _eventRepository.SaveChanges();
    }

    /// <summary>
    /// Usuwa wszystkie zdarzenia dla zwierzęcia (przy trwałym usunięciu rekordu).
    /// </summary>
    public void RemoveAllForAnimal(Guid animalId)
    {
        var toRemove = _eventRepository.AnimalEvents.Where(e => e.AnimalId == animalId).ToList();
        if (toRemove.Count == 0) return;

        foreach (var ev in toRemove)
            _eventRepository.AnimalEvents.Remove(ev);

        _eventRepository.SaveChanges();
    }
}
