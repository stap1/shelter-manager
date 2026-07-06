# 🐾 ShelterManager - System Zarządzania Schroniskiem

Aplikacja mobilna/desktopowa w **.NET MAUI** do kompleksowego zarządzania schroniskiem dla zwierząt: ewidencja, archiwum, magazyn, harmonogram opieki, boksy, adopcje, audit trail i raporty. Dane trzymane lokalnie (JSON), bez backendu.

> Projekt zaliczeniowy z przedmiotu .NET MAUI, realizowany w formule grupowej. Architekturę, koncepcję i implementację wykonał Stanisław Przybyłek.

## 🚀 Funkcje Systemu
* **Ewidencja zwierząt**: Dodawanie, edycja i przeglądanie bazy podopiecznych (gatunek, status, zdjęcie).
* **Archiwum zwierząt (soft delete)**: Archiwizacja i przywracanie; trwałe usunięcie tylko dla Administratora.
* **System ról i uprawnień**: Rozdzielenie funkcji między Administratora a Pracownika.
* **Dashboard**: Szybki podgląd statystyk (liczba zwierząt, kwarantanna).
* **Magazyn**: Zasoby z progami niskiego stanu, zużycie z powodem (transakcje) i uzupełnienia.
* **Harmonogram opieki**: Zadania z datą, typem i opcjonalnym powiązaniem ze zwierzęciem (Dzisiaj/Nadchodzące, filtry, alert o zaległych).
* **Boksy/Klatki**: Trwała lista boksów, zarządzanie (dodaj/usuń/pojemność) i przydział zwierząt z walidacją miejsca.
* **Adopcje**: Wnioski adopcyjne z workflow (New/InReview/Approved/Rejected) i automatyczne ustawienie zwierzęcia jako Adopted.
* **Rejestr zdarzeń (audit trail)**: Automatyczne logowanie zdarzeń zwierzęcia i podgląd na jego ekranie.
* **Raporty**: Zwierzęta aktywne vs archiwum, obłożenie boksów, adopcje w okresie, zużycie zasobów, zadania na dziś i zaległe.
* **Lokalna baza (JSON)**: Jeden plik `shelter_db.json` (migracja ze starego `shelter_data.json`).

## 🏗️ Architektura
Warstwowy podział: `Models/` (encje, enumy, tolerancyjne konwertery JSON), `Data/ShelterDataStore` (jedno źródło prawdy - odczyt/zapis JSON z blokadą wątków i migracją), `Data/Repositories/`, `Services/` (logika domenowa: alokacja boksów, workflow adopcji, zdarzenia, raporty), `Converters/` oraz strony XAML z code-behind.

## 🔐 Logowanie (wersja demo)
Lokalne demo bez realnego uwierzytelniania. Hasło administratora jest trzymane w kodzie jako **hash SHA-256** (nie plaintext) i nie jest ujawniane w interfejsie.

| Rola | Logowanie | Uprawnienia |
| :--- | :--- | :--- |
| **Administrator** | hasło demo: `admin123` | Pełny dostęp, w tym trwałe usuwanie (np. z Archiwum). |
| **Pracownik** | bez hasła | Przeglądanie i edycja, archiwizacja, zadania, boksy, magazyn, adopcje. Bez trwałego usuwania. |

## 🛠️ Uruchomienie
1. **Wymagania**: Visual Studio 2022/2026 z obciążeniem „.NET MAUI". Pakiety NuGet (`Newtonsoft.Json`, `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui`) przywrócą się automatycznie.
2. **Pobranie**:
   ```bash
   git clone https://github.com/stap1/shelter-manager.git
   cd shelter-manager
   ```
3. **Start**: otwórz `ShelterManager.sln`, wybierz cel (np. **Windows Machine**) i naciśnij **F5**.

## 📁 Dane
Przechowywane lokalnie w JSON (`shelter_db.json`) w folderze `AppData` urządzenia - trwałość między sesjami, z migracją ze starszego formatu.

## 📄 Licencja
MIT - zobacz [LICENSE](LICENSE).
