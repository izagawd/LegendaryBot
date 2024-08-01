using System.Collections;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;

namespace DiscordBotNet.Database.Models;
public struct UserDataInventoryCombined : IInventoryEntityContainer<IInventoryEntity>
{


    private UserData _userData;



    public UserDataInventoryCombined(UserData userData)
    {
        _userData = userData;
    }

    public void MergeDuplicates()
    {
        _userData.Items.MergeDuplicates();
    }

    public IEnumerator<IInventoryEntity> GetEnumerator()
    {
        foreach (var i in _userData.Characters)
        {
            yield return i;
;        }

        foreach (var i in _userData.Items)
        {
            yield return i;
        }

        foreach (var i in _userData.Gears)
        {
            yield return i;
        }
        foreach (var i in _userData.Blessings)
        {
            yield return i;
        }
        
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(IInventoryEntity inventoryEntity)
    {
        switch (inventoryEntity)
        {
            case Character character:
                _userData.Characters.Add(character);
                break;
            case Item inventoryItem:
                _userData.Items.Add(inventoryItem);
                break;
            case Gear gear:
                _userData.Gears.Add(gear);
                break;
            case Blessing blessing:
                _userData.Blessings.Add(blessing);
                break;
            default:
                throw new ArgumentException($"Unsupported type {inventoryEntity.GetType()}");
        }
    }

    public void AddRange(IEnumerable<IInventoryEntity> inventoryEntities)
    {
        foreach (var i in inventoryEntities)
        {
            Add(i);
        }
    }
    public void Clear()
    {
        _userData.Characters.Clear();
        _userData.Items.Clear();
        _userData.Gears.Clear();
        _userData.Blessings.Clear();
    }

    public bool Contains(IInventoryEntity inventoryEntity)
    {
        return inventoryEntity switch
        {
            Character character => _userData.Characters.Contains(character),
            Item item => _userData.Items.Contains(item),
            Gear gear => _userData.Gears.Contains(gear),
            Blessing blessing => _userData.Blessings.Contains(blessing),
            _ => false
        };
    }

    public bool Remove(IInventoryEntity inventoryEntity)
    {
        switch (inventoryEntity)
        {
            case Character character:
                return _userData.Characters.Remove(character);
            
            case Item item:
                return _userData.Items.Remove(item);
            
            case Gear gear:
                return _userData.Gears.Remove(gear);
            
            case Blessing blessing:
                return _userData.Blessings.Remove(blessing);
            default:
                throw new Exception("Weird");
            
        }
    }
    public void CopyTo(IInventoryEntity[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

 

    public int Count => _userData.Items.Count + _userData.Characters.Count + _userData.Blessings.Count +
                        _userData.Gears.Count;
    public bool IsReadOnly => false;
    public int IndexOf(IInventoryEntity inventoryEntity)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, IInventoryEntity inventoryEntity)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    public IInventoryEntity this[int index]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}