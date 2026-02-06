using ShelterManager.Models;
using System.Collections.ObjectModel;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium klatek/boksów.
/// </summary>
public interface ICageRepository
{
    ObservableCollection<Cage> Cages { get; }

    /// <summary>
    /// Dodaje nowy boks.
    /// </summary>
    void AddCage(Cage cage);

    /// <summary>
    /// Usuwa boks, o ile jest pusty.
    /// Zwraca false, jeśli boks jest zajęty.
    /// </summary>
    bool TryRemoveCage(Cage cage);

    /// <summary>
    /// Zapis zmian w pliku.
    /// </summary>
    void SaveChanges();
    void Reload();
}
