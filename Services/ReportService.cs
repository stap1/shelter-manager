using System.Collections.ObjectModel;
using ShelterManager.Data.Repositories;
using ShelterManager.Models;
using ShelterManager.Models.Reports;

namespace ShelterManager.Services;

/// <summary>
/// Serwis raportów.
/// 
/// Zasada: cała logika liczenia i filtrowania jest tutaj,
/// a UI jedynie renderuje gotowy DTO (ReportsDto).
/// </summary>
public sealed class ReportService
{
    private readonly IAnimalRepository _animalRepository;
    private readonly ICageRepository _cageRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IInventoryTransactionRepository _transactionRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IAnimalEventRepository _eventRepository;

    public ReportService(
        IAnimalRepository animalRepository,
        ICageRepository cageRepository,
        IResourceRepository resourceRepository,
        IInventoryTransactionRepository transactionRepository,
        ITaskRepository taskRepository,
        IAnimalEventRepository eventRepository)
    {
        _animalRepository = animalRepository;
        _cageRepository = cageRepository;
        _resourceRepository = resourceRepository;
        _transactionRepository = transactionRepository;
        _taskRepository = taskRepository;
        _eventRepository = eventRepository;
    }

    /// <summary>
    /// Generuje raport dla zakresu dat wybranych przez użytkownika (daty w czasie lokalnym).
    /// Zakres jest liczony jako [start 00:00, end 23:59:59.999] w czasie lokalnym,
    /// a następnie konwertowany do UTC dla danych przechowywanych z UtcNow.
    /// </summary>
    public ReportsDto BuildReport(DateTime startLocalDate, DateTime endLocalDate)
    {
        // Normalizacja zakresu
        var startDate = startLocalDate.Date;
        var endDate = endLocalDate.Date;
        if (endDate < startDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        // Konwersja do UTC (endExclusive)
        var startUtc = new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
        var endExclusiveUtc = new DateTime(endDate.Year, endDate.Month, endDate.Day, 0, 0, 0, DateTimeKind.Local).AddDays(1).ToUniversalTime();

        // Zwierzęta
        int activeAnimals = _animalRepository.Animals.Count(a => !a.IsArchived);
        int archivedAnimals = _animalRepository.Animals.Count(a => a.IsArchived);

        // Boksy
        var cagesDto = BuildCageOccupancyDto();

        // Adopcje w okresie (na bazie audit trail)
        int adoptionsApproved = _eventRepository.AnimalEvents
            .Count(e => e.Type == AnimalEventType.AdoptionApproved && e.Timestamp >= startUtc && e.Timestamp < endExclusiveUtc);

        // Zużycie zasobów w okresie
        var (consumptionList, totalConsumed) = BuildResourceConsumption(startUtc, endExclusiveUtc);

        // Zadania na dziś i zaległe (liczone w czasie lokalnym, bo ScheduledAt jest używane w UI lokalnie)
        var tasksDto = BuildTasksSummaryDto();

        return new ReportsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            GeneratedAtText = DateTime.Now.ToString("dd.MM.yyyy HH:mm"),

            ActiveAnimalsCount = activeAnimals,
            ArchivedAnimalsCount = archivedAnimals,

            Cages = cagesDto,
            AdoptionsApprovedInPeriodCount = adoptionsApproved,

            TotalConsumedAmountInPeriod = totalConsumed,
            TotalConsumedText = totalConsumed.ToString("0.##"),
            ResourceConsumption = consumptionList,

            Tasks = tasksDto
        };
    }

    private CageOccupancySummaryDto BuildCageOccupancyDto()
    {
        var cages = _cageRepository.Cages.OrderBy(c => c.Numer).ToList();

        int totalCages = cages.Count;
        int totalCapacity = cages.Sum(c => Math.Max(0, c.Capacity));
        int totalOccupied = cages.Sum(c => c.OccupiedAnimalIds?.Count ?? 0);

        double percent = 0;
        if (totalCapacity > 0)
            percent = (double)totalOccupied / totalCapacity * 100.0;

        var items = new ObservableCollection<CageOccupancyItemDto>(
            cages.Select(c => new CageOccupancyItemDto
            {
                Numer = c.Numer,
                Capacity = c.Capacity,
                Occupied = c.OccupiedAnimalIds?.Count ?? 0
            }));

        return new CageOccupancySummaryDto
        {
            TotalCages = totalCages,
            TotalCapacity = totalCapacity,
            TotalOccupied = totalOccupied,
            OccupancyPercent = percent,
            Cages = items
        };
    }

    private (ObservableCollection<ResourceConsumptionItemDto> items, double totalConsumed) BuildResourceConsumption(DateTime startUtc, DateTime endExclusiveUtc)
    {
        var negative = _transactionRepository.Transactions
            .Where(t => t.Timestamp >= startUtc && t.Timestamp < endExclusiveUtc)
            .Where(t => t.Delta < 0)
            .ToList();

        if (negative.Count == 0)
            return (new ObservableCollection<ResourceConsumptionItemDto>(), 0);

        var resourcesById = _resourceRepository.Resources.ToDictionary(r => r.Id, r => r);

        var grouped = negative
            .GroupBy(t => t.ResourceId)
            .Select(g => new
            {
                ResourceId = g.Key,
                Consumed = g.Sum(x => -x.Delta)
            })
            .OrderByDescending(x => x.Consumed)
            .ToList();

        double totalConsumed = grouped.Sum(x => x.Consumed);

        var items = new ObservableCollection<ResourceConsumptionItemDto>(
            grouped.Select(x =>
            {
                if (resourcesById.TryGetValue(x.ResourceId, out var res))
                {
                    return new ResourceConsumptionItemDto
                    {
                        ResourceName = res.Nazwa,
                        Unit = res.Jednostka,
                        ConsumedAmount = x.Consumed
                    };
                }

                return new ResourceConsumptionItemDto
                {
                    ResourceName = "Nieznany zasób",
                    Unit = string.Empty,
                    ConsumedAmount = x.Consumed
                };
            }));

        return (items, totalConsumed);
    }

    private TasksSummaryDto BuildTasksSummaryDto()
    {
        var today = DateTime.Today;

        var planned = _taskRepository.Tasks
            .Where(t => t.Status == CareTaskStatus.Planned)
            .ToList();

        var todayTasks = planned
            .Where(t => t.ScheduledAt.Date == today)
            .OrderBy(t => t.ScheduledAt)
            .ToList();

        var overdueTasks = planned
            .Where(t => t.ScheduledAt.Date < today)
            .OrderBy(t => t.ScheduledAt)
            .ToList();

        // Ograniczamy listy dla czytelności w UI.
        const int maxItems = 20;

        var todayDtos = new ObservableCollection<TaskMiniDto>(todayTasks.Take(maxItems).Select(ToTaskMiniDto));
        var overdueDtos = new ObservableCollection<TaskMiniDto>(overdueTasks.Take(maxItems).Select(ToTaskMiniDto));

        return new TasksSummaryDto
        {
            TodayPlannedCount = todayTasks.Count,
            OverduePlannedCount = overdueTasks.Count,
            TodayTasks = todayDtos,
            OverdueTasks = overdueDtos
        };
    }

    private static TaskMiniDto ToTaskMiniDto(Zadanie t)
    {
        return new TaskMiniDto
        {
            WhenText = t.ScheduledAt.ToString("dd.MM HH:mm"),
            TypeText = t.TypeOpis,
            AnimalText = t.AnimalOpis,
            NotesText = string.IsNullOrWhiteSpace(t.Notes) ? "" : t.Notes
        };
    }
}
