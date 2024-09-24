namespace DiscordBot;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum Choice
{
    FiveStar,
    FourStarCharacter,
    FourStarBlessing,
    ThreeStarBlessing
}

public class ProbabilityDictionary<T> : IDictionary<T, double>
{
    private readonly Dictionary<T, double> _internalDictionary = new Dictionary<T, double>();

    public void Redistribute(T itemToRemove, IEnumerable<T> items)
    {
        Redistribute(itemToRemove,items.ToArray());
    }
    public void Redistribute(T itemToRemove, params T[] itemsToRedistribute)
    {

        if (itemsToRedistribute.Length == 0)
            throw new Exception($"there must be at least 1 {nameof(itemsToRedistribute)}");
        if (itemsToRedistribute.Contains(itemToRemove))
            throw new Exception($"{nameof(itemToRemove)} must not be in {nameof(itemsToRedistribute)}");
        if (!_internalDictionary.Remove(itemToRemove, out var valueToRedistribute))
        {
            return;
        };

        
        // Calculate the total weight of the items to redistribute to
        double totalRedistributeWeight = itemsToRedistribute.Sum(item => _internalDictionary[item]);

        // Redistribute the value proportionally
        foreach (var item in itemsToRedistribute)
        {
            _internalDictionary[item] += valueToRedistribute * (_internalDictionary[item] / totalRedistributeWeight);
        }
        
    }

    // IDictionary<T, double> Implementation

    public double this[T key]
    {
        get => _internalDictionary[key];
        set => _internalDictionary[key] = value;
    }

    public ICollection<T> Keys => _internalDictionary.Keys;

    public ICollection<double> Values => _internalDictionary.Values;

    public int Count => _internalDictionary.Count;

    public bool IsReadOnly => ((IDictionary<T, double>)_internalDictionary).IsReadOnly;

    public void Add(T key, double value)
    {
        _internalDictionary.Add(key, value);
    }

    public void Add(KeyValuePair<T, double> item)
    {
        ((IDictionary<T, double>)_internalDictionary).Add(item);
    }

    public void Clear()
    {
        _internalDictionary.Clear();
    }

    public bool Contains(KeyValuePair<T, double> item)
    {
        return ((IDictionary<T, double>)_internalDictionary).Contains(item);
    }

    public bool ContainsKey(T key)
    {
        return _internalDictionary.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<T, double>[] array, int arrayIndex)
    {
        ((IDictionary<T, double>)_internalDictionary).CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<T, double>> GetEnumerator()
    {
        return _internalDictionary.GetEnumerator();
    }

    public bool Remove(T key)
    {
        return _internalDictionary.Remove(key);
    }

    public bool Remove(KeyValuePair<T, double> item)
    {
        return ((IDictionary<T, double>)_internalDictionary).Remove(item);
    }

    public bool TryGetValue(T key, out double value)
    {
        return _internalDictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _internalDictionary.GetEnumerator();
    }
}

