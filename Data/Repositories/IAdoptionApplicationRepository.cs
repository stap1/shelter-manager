using System.Collections.ObjectModel;
using ShelterManager.Models;

namespace ShelterManager.Data.Repositories;

/// <summary>
/// Repozytorium wniosków adopcyjnych.
/// UI nie powinno znać formatu persystencji (DIP - Dependency Inversion Principle).
/// </summary>
public interface IAdoptionApplicationRepository
{
    ObservableCollection<AdoptionApplication> Applications { get; }

    void SaveChanges();
    void Reload();
}
