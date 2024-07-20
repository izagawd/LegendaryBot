using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Entities;


public static class InventoryEntityExtensions
{

}
/// <summary>
/// An entity that can be stored in the user's inventory
/// </summary>
public interface IInventoryEntity 
{

    string DisplayString { get; }
    Type TypeGroup { get;  }
    DateTime DateAcquired { get; set; }

    string Description { get; }
    Rarity Rarity { get;  }


    string Name { get; }
 



    UserData? UserData { get; set; }

    string ImageUrl { get; }


    ulong UserDataId { get; set; }

}
