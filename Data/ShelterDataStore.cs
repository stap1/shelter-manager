using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json;
using ShelterManager.Infrastructure;
using ShelterManager.Models;

namespace ShelterManager.Data;

/// <summary>
/// Jedno źródło prawdy dla danych aplikacji.
/// Odpowiada za odczyt/zapis jednego pliku JSON (np. shelter_db.json)
/// oraz migrację ze starego formatu (shelter_data.json zawierający wyłącznie listę zwierząt).
/// </summary>
public sealed class ShelterDataStore
{
    private readonly object _sync = new();

    // Optymalizacja: jeśli plik nie zmienił się od ostatniego wczytania,
    // Reload() może pominąć ponowną deserializację.
    private DateTime _lastFileWriteUtc = DateTime.MinValue;

    private readonly string _dbFilePath;
    private readonly string _legacyAnimalsFilePath;

    public ObservableCollection<Zwierze> Animals { get; } = new();
    public ObservableCollection<Cage> Cages { get; } = new();
    public ObservableCollection<Zasob> Resources { get; } = new();
    public ObservableCollection<InventoryTransaction> InventoryTransactions { get; } = new();
    public ObservableCollection<Zadanie> Tasks { get; } = new();
    public ObservableCollection<AnimalEvent> AnimalEvents { get; } = new();
    public ObservableCollection<AdoptionApplication> AdoptionApplications { get; } = new();

    public ShelterDataStore()
    {
        _dbFilePath = Path.Combine(FileSystem.AppDataDirectory, "shelter_db.json");
        _legacyAnimalsFilePath = Path.Combine(FileSystem.AppDataDirectory, "shelter_data.json");

        Initialize();
    }

    /// <summary>
    /// Inicjalizacja: wczytanie danych, ewentualna migracja, seed danych startowych.
    /// </summary>
    private void Initialize()
    {
        lock (_sync)
        {
            bool hadDatabase = File.Exists(_dbFilePath);

            // 1) Migracja, jeśli nie mamy nowego pliku, ale mamy stary
            if (!hadDatabase && File.Exists(_legacyAnimalsFilePath))
            {
                hadDatabase = TryMigrateLegacyAnimalsFile();
            }

            // 2) Wczytanie nowego formatu (jeśli istnieje)
            LoadFromFile();

            // Porządek danych: stabilne sortowanie pod UI
            SortCollections();

            // 3) Seed danych startowych TYLKO przy pierwszym uruchomieniu (brak bazy).
            //    Nigdy przy pustej kolekcji - inaczej rekordy usunięte przez użytkownika
            //    "wracałyby" po restarcie.
            if (!hadDatabase)
                EnsureSeedData();

            // 4) Powiązanie klatek z obiektami zwierząt na podstawie LokatorId
            ResolveCageOccupants();

            // 5) Powiązanie zadań z obiektami zwierząt (AnimalId -> Zwierze)
            ResolveTaskAnimals();

            // 6) Powiązanie wniosków adopcyjnych z obiektami zwierząt (AnimalId -> Zwierze)
            ResolveAdoptionApplicationAnimals();
        }
    }

    public void SaveChanges()
    {
        lock (_sync)
        {
            // Utrzymujemy stabilną kolejność w pliku i UI.
            SortCollections();

            var dto = new ShelterDbDto
            {
                // Optymalizacja: nie kopiujemy kolekcji, bo i tak zapis jest pod lockiem.
                Animals = Animals,
                Cages = Cages,
                Resources = Resources,
                InventoryTransactions = InventoryTransactions,
                Tasks = Tasks,
                AnimalEvents = AnimalEvents,
                AdoptionApplications = AdoptionApplications,
                SchemaVersion = 6
            };

            try
            {
                var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
                File.WriteAllText(_dbFilePath, json);

                if (File.Exists(_dbFilePath))
                    _lastFileWriteUtc = File.GetLastWriteTimeUtc(_dbFilePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd zapisu bazy: {ex.Message}");
            }
        }
    }

    public void Reload()
    {
        lock (_sync)
        {
            // Jeśli plik nie zmienił się od poprzedniego wczytania,
            // omijamy ciężką deserializację i tylko odświeżamy powiązania.
            DateTime currentWriteUtc = File.Exists(_dbFilePath)
                ? File.GetLastWriteTimeUtc(_dbFilePath)
                : DateTime.MinValue;

            if (currentWriteUtc != _lastFileWriteUtc)
            {
                LoadFromFile();
                _lastFileWriteUtc = currentWriteUtc;
            }

            // Uwaga: NIE seedujemy przy Reload - seed jest wyłącznie pierwszorazowy
            // (w Initialize). Inaczej opróżnione kolekcje "odtwarzałyby się".
            SortCollections();
            ResolveCageOccupants();
            ResolveTaskAnimals();
            ResolveAdoptionApplicationAnimals();
        }
    }

    private void LoadFromFile()
    {
        if (!File.Exists(_dbFilePath))
        {
            Animals.Clear();
            Cages.Clear();
            Resources.Clear();
            InventoryTransactions.Clear();
            Tasks.Clear();
            AnimalEvents.Clear();
            AdoptionApplications.Clear();
            return;
        }

        try
        {
            var json = File.ReadAllText(_dbFilePath);
            var dto = JsonConvert.DeserializeObject<ShelterDbDto>(json);

            _lastFileWriteUtc = File.GetLastWriteTimeUtc(_dbFilePath);

            Animals.Clear();
            Cages.Clear();
            Resources.Clear();
            InventoryTransactions.Clear();
            Tasks.Clear();
            AnimalEvents.Clear();
            AdoptionApplications.Clear();

            if (dto is null) return;

            foreach (var a in dto.Animals ?? new()) Animals.Add(a);
            foreach (var c in dto.Cages ?? new()) Cages.Add(c);
            foreach (var r in dto.Resources ?? new()) Resources.Add(r);
            foreach (var tx in dto.InventoryTransactions ?? new()) InventoryTransactions.Add(tx);
            foreach (var t in dto.Tasks ?? new()) Tasks.Add(t);
            foreach (var ev in dto.AnimalEvents ?? new()) AnimalEvents.Add(ev);
            foreach (var a in dto.AdoptionApplications ?? new()) AdoptionApplications.Add(a);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd wczytywania bazy: {ex.Message}");
        }
    }

    /// <summary>
    /// Stabilne sortowanie kolekcji pod UI.
    /// </summary>
    private void SortCollections()
    {
        // Boksy rosnąco po numerze
        Cages.SortBy(c => c.Numer);

        // Magazyn alfabetycznie po nazwie (nie skacze przy zmianie ilości)
        Resources.SortBy(r => r.Nazwa ?? string.Empty, StringComparer.CurrentCultureIgnoreCase);
    }

    private bool TryMigrateLegacyAnimalsFile()
    {
        try
        {
            var json = File.ReadAllText(_legacyAnimalsFilePath);

            // Stary plik zawierał listę/ObservableCollection zwierząt.
            // Deserializujemy do listy, żeby pokryć oba przypadki.
            var animals = JsonConvert.DeserializeObject<List<Zwierze>>(json)
                          ?? JsonConvert.DeserializeObject<ObservableCollection<Zwierze>>(json)?.ToList()
                          ?? new List<Zwierze>();

            var dto = new ShelterDbDto
            {
                Animals = new ObservableCollection<Zwierze>(animals),
                Cages = new ObservableCollection<Cage>(),
                Resources = new ObservableCollection<Zasob>(),
                InventoryTransactions = new ObservableCollection<InventoryTransaction>(),
                Tasks = new ObservableCollection<Zadanie>(),
                AnimalEvents = new ObservableCollection<AnimalEvent>(),
                AdoptionApplications = new ObservableCollection<AdoptionApplication>(),
                SchemaVersion = 6
            };

            var newJson = JsonConvert.SerializeObject(dto, Formatting.Indented);
            File.WriteAllText(_dbFilePath, newJson);
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Błąd migracji shelter_data.json -> shelter_db.json: {ex.Message}");
            return false;
        }
    }

    private void EnsureSeedData()
    {
        bool changed = false;

        // Zwierzęta: zachowujemy dotychczasowe zachowanie aplikacji.
        if (Animals.Count == 0)
        {
            Animals.Add(new Zwierze
            {
                Imie = "Burek",
                Gatunek = AnimalSpecies.Dog,
                Rasa = "Owczarek",
                Status = AnimalStatus.Quarantine,
                Zdjecie = "https://loremflickr.com/400/400/dog,germanshepherd?lock=1",
                Wiek = "Nieznany",
                HistoriaMedyczna = "Brak wpisów"
            });
            Animals.Add(new Zwierze
            {
                Imie = "Mruczek",
                Gatunek = AnimalSpecies.Cat,
                Rasa = "Dachowiec",
                Status = AnimalStatus.Quarantine,
                Zdjecie = "https://loremflickr.com/400/400/cat?lock=2",
                Wiek = "Nieznany",
                HistoriaMedyczna = "Brak wpisów"
            });
            Animals.Add(new Zwierze
            {
                Imie = "Reksio",
                Gatunek = AnimalSpecies.Dog,
                Rasa = "Labrador",
                Status = AnimalStatus.ForAdoption,
                Zdjecie = "https://loremflickr.com/400/400/dog,labrador?lock=3",
                Wiek = "Nieznany",
                HistoriaMedyczna = "Brak wpisów"
            });

            changed = true;
        }

        // Boksy: domyślnie 10 boksów. Jeśli ich nie ma, tworzymy.
        // Lista boksów jest przechowywana w danych (shelter_db.json), zamiast generować ją "na żywo".
        if (Cages.Count == 0)
        {
            var mieszkancy = Animals
                .Where(z => z.Status != AnimalStatus.Adopted && !z.IsArchived)
                .ToList();

            for (int i = 1; i <= 10; i++)
            {
                var cage = new Cage { Numer = i, Capacity = 1 };

                if (i <= mieszkancy.Count)
                    cage.OccupiedAnimalIds.Add(mieszkancy[i - 1].Id);

                Cages.Add(cage);
            }

            changed = true;
        }

        // Magazyn: dane startowe
        if (Resources.Count == 0)
        {
            Resources.Add(new Zasob { Nazwa = "Karma sucha (Pies)", Ilosc = 20, Jednostka = "szt.", LowStockThreshold = 10 });
            Resources.Add(new Zasob { Nazwa = "Karma mokra (Kot)", Ilosc = 15, Jednostka = "szt.", LowStockThreshold = 10 });
            Resources.Add(new Zasob { Nazwa = "Podkłady higieniczne", Ilosc = 5, Jednostka = "szt.", LowStockThreshold = 5 });
            Resources.Add(new Zasob { Nazwa = "Szampon", Ilosc = 1, Jednostka = "szt.", LowStockThreshold = 2 });
            Resources.Add(new Zasob { Nazwa = "Smycze", Ilosc = 8, Jednostka = "szt.", LowStockThreshold = 5 });

            changed = true;
        }

        // Zadania: dane startowe
        if (Tasks.Count == 0)
        {
            var today = DateTime.Today;
            Tasks.Add(new Zadanie
            {
                ScheduledAt = today.AddHours(8),
                Type = CareTaskType.Feeding,
                Notes = "Karmienie psów (Sektor A)",
                Status = CareTaskStatus.Planned
            });
            Tasks.Add(new Zadanie
            {
                ScheduledAt = today.AddHours(9).AddMinutes(30),
                Type = CareTaskType.Walking,
                Notes = "Spacer (wybierz zwierzę w edycji, jeśli dotyczy)",
                Status = CareTaskStatus.Planned
            });
            Tasks.Add(new Zadanie
            {
                ScheduledAt = today.AddHours(12),
                Type = CareTaskType.Medication,
                Notes = "Podanie leków (np. Burek)",
                Status = CareTaskStatus.Planned
            });

            changed = true;
        }

        // Po seedzie zapisujemy, żeby kolejne uruchomienia już tego nie robiły.
        if (changed)
            SaveChanges();
    }

    private void ResolveCageOccupants()
    {
        // Powiązanie OccupiedAnimalIds z obiektami zwierząt na potrzeby UI.
        // Lista OccupiedAnimals jest [JsonIgnore], więc po wczytaniu wymaga ponownego wypełnienia.
        var byId = Animals.ToDictionary(a => a.Id, a => a);
        bool changed = false;

        foreach (var cage in Cages)
        {
            cage.OccupiedAnimals.Clear();

            foreach (var animalId in cage.OccupiedAnimalIds.ToList())
            {
                if (!byId.TryGetValue(animalId, out var animal))
                {
                    // Usuwamy "wiszące" Id (zwierzę usunięte z bazy).
                    cage.OccupiedAnimalIds.Remove(animalId);
                    changed = true;
                    continue;
                }

                // Zasady biznesowe: Adopted/Archived nie powinny zajmować boksów.
                if (animal.Status == AnimalStatus.Adopted || animal.IsArchived)
                {
                    cage.OccupiedAnimalIds.Remove(animalId);
                    changed = true;
                    continue;
                }

                cage.OccupiedAnimals.Add(animal);
            }

            // Zabezpieczenie: jeśli ktoś ustawi pojemność < 1 w JSON, normalizujemy.
            if (cage.Capacity < 1)
                cage.Capacity = 1;
        }

        // Jeśli podczas wiązania wykryliśmy nieprawidłowe przydziały, zapisujemy poprawki.
        if (changed)
            SaveChanges();
    }

    private void ResolveTaskAnimals()
    {
        // Powiązanie AnimalId z obiektem zwierzęcia na potrzeby UI.
        var byId = Animals.ToDictionary(a => a.Id, a => a);
        bool changed = false;

        foreach (var task in Tasks)
        {
            if (task.AnimalId is not Guid animalId)
            {
                task.SetResolvedAnimal(null);
                continue;
            }

            if (!byId.TryGetValue(animalId, out var animal))
            {
                // Usuwamy "wiszące" Id.
                task.AnimalId = null;
                task.SetResolvedAnimal(null);
                changed = true;
                continue;
            }

            // Zasady biznesowe: Adopted/Archived nie powinny być przypinane do harmonogramu.
            if (animal.Status == AnimalStatus.Adopted || animal.IsArchived)
            {
                task.AnimalId = null;
                task.SetResolvedAnimal(null);
                changed = true;
                continue;
            }

            task.SetResolvedAnimal(animal);
        }

        if (changed)
            SaveChanges();
    }

    private void ResolveAdoptionApplicationAnimals()
    {
        // Powiązanie AnimalId z obiektem zwierzęcia na potrzeby UI.
        // W przeciwieństwie do zadań, wnioski adopcyjne mogą dotyczyć zwierzęcia Adopted
        // (np. zatwierdzony wniosek). Dlatego nie czyścimy powiązania dla Adopted/Archived.
        var byId = Animals.ToDictionary(a => a.Id, a => a);

        foreach (var app in AdoptionApplications)
        {
            if (byId.TryGetValue(app.AnimalId, out var animal))
                app.SetResolvedAnimal(animal);
            else
                app.SetResolvedAnimal(null);
        }
    }
}
