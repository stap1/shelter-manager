namespace ShelterManager.Models;

/// <summary>
/// Gatunek zwierzęcia.
///
/// Unknown na pozycji 0 jest celowy: jeżeli w starym pliku JSON nie było pola "Gatunek",
/// deserializacja ustawi wartość domyślną (0) zamiast przypadkowo przypisać np. Dog.
/// </summary>
public enum AnimalSpecies
{
    Unknown = 0,
    Dog = 1,
    Cat = 2,
    Other = 3
}
