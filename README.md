# 🐾 ShelterManager - System Zarządzania Schroniskiem

Aplikacja mobilna/desktopowa stworzona w technologii **.NET MAUI** do kompleksowego zarządzania schroniskiem dla zwierząt. Projekt realizuje wymagania dotyczące bezpiecznego przechowywania danych, autoryzacji oraz interaktywnego interfejsu użytkownika.

## 🚀 Funkcje Systemu
* **Ewidencja zwierząt**: Dodawanie, edycja i przeglądanie bazy podopiecznych (gatunek, status, zdjęcie).
* **Archiwum zwierząt (soft delete)**: Archiwizacja i przywracanie; trwałe usunięcie tylko dla Administratora.
* **System ról i uprawnień**: Rozdzielenie funkcji między Administratora a Pracownika.
* **Dashboard**: Szybki podgląd statystyk (liczba zwierząt, kwarantanna).
* **Magazyn**: Zasoby z progami niskiego stanu, zużycie z powodem (transakcje) i uzupełnienia.
* **Harmonogram opieki**: Zadania z datą, typem i opcjonalnym powiązaniem ze zwierzęciem (widoki Dzisiaj/Nadchodzące, filtry, alert o zaległych).
* **Boksy/Klatki**: Trwała lista boksów, zarządzanie (dodaj/usuń/pojemność) i przydział zwierząt z walidacją miejsca.
* **Adopcje**: Wnioski adopcyjne z workflow (New/InReview/Approved/Rejected) oraz automatyczne ustawienie zwierzęcia jako Adopted.
* **Rejestr zdarzeń (audit trail)**: Automatyczne logowanie zdarzeń zwierzęcia i podgląd na ekranie zwierzęcia.
* **Raporty**: Zwierzęta aktywne vs archiwum, obłożenie boksów, adopcje w okresie, zużycie zasobów w okresie, zadania na dziś i zaległe.
* **Lokalna baza danych (JSON)**: Jeden plik `shelter_db.json` (migracja ze starego `shelter_data.json`).

## 🔐 Logowanie i Uprawnienia

| Rola | Hasło | Uprawnienia |
| :--- | :--- | :--- |
| **Administrator** | `admin123` | Pełny dostęp, w tym trwałe usuwanie (np. z Archiwum) i zarządzanie danymi. |
| **Pracownik** | Brak (logowanie bezpośrednie) | Przeglądanie i edycja danych, archiwizacja/przywracanie zwierząt, zadania, boksy, magazyn, adopcje. **Bez trwałego usuwania.** |

## 🛠️ Jak uruchomić projekt?

1.  **Wymagania**:
    * Visual Studio 2022/2026 z zainstalowanym obciążeniem ".NET MAUI".
    * Zainstalowany pakiet NuGet: `Newtonsoft.Json`, `CommunityToolkit.Mvvm`.

2.  **Instalacja**:
    ```bash
    git clone [LINK_DO_TWOJEGO_REPOZYTORIUM]
    cd ShelterManager
    ```

3.  **Uruchomienie**:
    * Otwórz plik `ShelterManager.sln` w Visual Studio.
    * Wybierz platformę docelową (np. **Windows Machine** lub **Android Emulator**).
    * Naciśnij **F5**, aby skompilować i uruchomić aplikację.

## 📁 Struktura danych
Dane aplikacji są przechowywane lokalnie w formacie JSON w folderze `AppData` urządzenia, co zapewnia trwałość informacji między sesjami.