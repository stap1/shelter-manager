using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ShelterManager.Infrastructure;

/// <summary>
/// Pomocnicze metody dla ObservableCollection.
/// 
/// Zamiast Clear()+Add() przestawiamy elementy metodą Move,
/// co ogranicza migotanie UI i zmniejsza liczbę zmian w drzewie wizualnym.
/// </summary>
public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Sortuje kolekcję "w miejscu" (in-place) z użyciem ObservableCollection.Move.
    /// </summary>
    public static void SortBy<T, TKey>(this ObservableCollection<T> collection, Func<T, TKey> keySelector, IComparer<TKey>? comparer = null)
    {
        if (collection is null) return;
        comparer ??= Comparer<TKey>.Default;

        // Lista docelowej kolejności (referencje obiektów).
        var sorted = collection
            .OrderBy(keySelector, comparer)
            .ToList();

        if (sorted.Count != collection.Count) return;

        for (int targetIndex = 0; targetIndex < sorted.Count; targetIndex++)
        {
            var item = sorted[targetIndex];
            int currentIndex = collection.IndexOf(item);
            if (currentIndex < 0) continue;
            if (currentIndex != targetIndex)
                collection.Move(currentIndex, targetIndex);
        }
    }
}
