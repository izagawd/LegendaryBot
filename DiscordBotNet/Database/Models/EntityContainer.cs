using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.Items;

namespace DiscordBotNet.Database.Models;



public class EntityContainer : IList<Entity>
{
    [NotMapped] private List<Entity> _list = new();
    public IEnumerator<Entity> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public EntityContainer(){}

    public EntityContainer(IEnumerable<Entity> entities)
    {
         AddRange(entities);
    }

    public void AddRange(IEnumerable<Entity> enumerable)
    {
       _list.AddRange(enumerable);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    /// <summary>
    /// Gets the amount of stacks of a specific item
    /// </summary>
    public int GetItemStacks(Type itemType)
    {
        if (!itemType.IsSubclassOf(typeof(Item))) throw new Exception("expected item type");
        return _list.Where(i => i.GetType() == itemType).Select(i => ((Item)i).Stacks).Sum();
    }
    /// <summary>
    /// Gets the amount of stacks of a specific item
    /// </summary>
    public int GetItemStacks<TItem>() where TItem : Item
    {
        return GetItemStacks(typeof(TItem));
    }
    
    /// <summary>
    /// Add an amount of stacks for a specific item
    /// </summary>
    public void AddItemStacks(Type itemType, int stacksToAdd)
    {
        if (!itemType.IsSubclassOf(typeof(Item))) throw new Exception("expected item type");
        var first =_list.FirstOrDefault(i => i.GetType() == itemType) as Item;
        if (first is null)
        {
            first = (Item)Activator.CreateInstance(itemType)!;
            first.Stacks = 0;
        }
        first.Stacks += stacksToAdd;
    }
    public void AddItemStacks<TItem>(int stacksToAdd) where TItem : Item
    {
        
        AddItemStacks(typeof(TItem) ,stacksToAdd);
    }
    public void RemoveItemStacks<TItem>(int stacksToRemove) where TItem : Item
    {
        RemoveItemStacks(typeof(TItem),stacksToRemove);
    }
    /// <summary>
    /// Removes an indicated amount of stacks from the inputted item
    /// </summary>
    public void RemoveItemStacks(Type itemType, int stacksToRemove) 
    {
        if (!itemType.IsSubclassOf(typeof(Item))) throw new Exception("expected item type");
        foreach (var i in _list.Where(i => i.GetType() == itemType).Cast<Item>().ToArray())
        {
            if (i.Stacks >= stacksToRemove)
            {
                i.Stacks -= stacksToRemove;
                stacksToRemove = 0;
            } else if (stacksToRemove > i.Stacks)
            {
                stacksToRemove -= i.Stacks;
                i.Stacks = 0;
            }
            if (i.Stacks <= 0)
            {
                _list.Remove(i);
            }
            if(stacksToRemove <= 0)
                break;
        }
    }

 
    public void Add(Entity entity)
    {
       _list.Add(entity);
        
    }
    
    public void Arrange()
    {
        ArrangeItemStacks();
        ArrangeDupeCharacters();
    }

    public void ArrangeDupeCharacters()
    {
        var characters = _list.OfType<Character>()
            .ToArray();
        _list.RemoveAll(i => characters.Contains(i));
        foreach (var i in characters)
        {
            var already = (Character?) _list.FirstOrDefault(j => j.GetType() == i.GetType());
            if (already is not null)
            {
                already.DupeCount += i.DupeCount + 1;
            }
            else
            {
                Add(i);
            }
        }
    }
    public void ArrangeItemStacks()
    {
        var items = _list.OfType<Item>()
            .ToArray();
        _list.RemoveAll(i => items.Contains(i));
        foreach (var i in items)
        {
            var already = (Item?) _list.FirstOrDefault(j => j.GetType() == i.GetType());
            if (already is not null)
            {
                already.Stacks += i.Stacks;
            }
            else
            {
                Add(i);
            }
        }
    }
    public void Clear()
    {
        _list.Clear();
    }

    public bool Contains(Entity item)
    {
        return _list.Contains(item);
    }

    public void CopyTo(Entity[] array, int arrayIndex)
    {
        _list.CopyTo(array,arrayIndex);
    }

    public bool Remove(Entity item)
    {
        return _list.Remove(item);
    }

    public int Count => _list.Count;
    public bool IsReadOnly => false;
    public int IndexOf(Entity item)
    {
        return _list.IndexOf(item);
    }

    public void Insert(int index, Entity item)
    {
        _list.Insert(index,item);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    public Entity this[int index]
    {
        get => _list[index];
        set => _list[index] = value;
    }
}