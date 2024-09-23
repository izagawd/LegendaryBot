using Entities.Models;

namespace Entities.LegendaryBot.Entities;

public static class InventoryEntityExtensions
{
}

/// <summary>
///     An entity that can be stored in the user's inventory
/// </summary>
public interface IInventoryEntity : INameHaver
{

    int TypeId { get; }

    Type TypeGroup { get; }
    DateTime DateAcquired { get; set; }

    string Description { get; }
    Rarity Rarity { get; }


    UserData? UserData { get; set; }

    string ImageUrl { get; }


    long UserDataId { get; set; }
}