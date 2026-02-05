# 🐾 ShelterManager - System Zarządzania Schroniskiem

Aplikacja mobilna/desktopowa stworzona w technologii **.NET MAUI** do kompleksowego zarządzania schroniskiem dla zwierząt. Projekt realizuje wymagania dotyczące bezpiecznego przechowywania danych, autoryzacji oraz interaktywnego interfejsu użytkownika.

## 🚀 Funkcje Systemu
* **Ewidencja zwierząt**: Dodawanie, edycja i przeglądanie bazy podopiecznych.
* **System ról i uprawnień**: Rozdzielenie funkcji między Administratora a Pracownika.
* **Dashboard**: Szybki podgląd statystyk (liczba zwierząt, kwarantanna).
* **Magazyn**: Zarządzanie zapasami karmy i leków.
* **Harmonogram**: Planowanie zadań i opieki nad zwierzętami.
* **Mapy boksów**: Wizualizacja obłożenia klatek.

## 🔐 Logowanie i Uprawnienia

| Rola | Hasło | Uprawnienia |
| :--- | :--- | :--- |
| **Administrator** | `admin123` | Pełny dostęp, w tym usuwanie rekordów. |
| **Pracownik** | Brak (logowanie bezpośrednie) | Przeglądanie, edycja, zadania. **Blokada usuwania.** |

## 🛠️ Jak uruchomić projekt?

1.  **Wymagania**:
    * Visual Studio 2022 z zainstalowanym obciążeniem ".NET MAUI".
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