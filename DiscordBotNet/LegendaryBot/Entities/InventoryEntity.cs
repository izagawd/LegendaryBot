using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Entities;


public static class InventoryEntityExtensions
{

    public static IEnumerable<IInventoryEntity> CloneMultiple(this IInventoryEntity a, int number)
    {
        if (number <= 0)
        {
            throw new Exception("InventoryEntity times a negative number or 0 doesn't make sense");
            
        }
        foreach (var _ in Enumerable.Range(0, number))
        {
            yield return a.Clone();
        }
    }
}
/// <summary>
/// An entity that can be stored in the user's inventory
/// </summary>
public interface IInventoryEntity : ICloneable, IImageHaver
{

    
    
    Type TypeGroup { get;  }
    DateTime DateAcquired { get; set; }
    object ICloneable.Clone()
    {
        return Clone();
    }

    string Description { get; }
    Rarity Rarity { get;  }
    new IInventoryEntity Clone();

    string Name { get; }
 



    UserData? UserData { get; set; }

    string ImageUrl { get; }


    ulong UserDataId { get; set; }

}
