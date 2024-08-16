using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot.Entities;

public static class InventoryEntityExtensions
{
}

/// <summary>
///     An entity that can be stored in the user's inventory
/// </summary>
public interface IInventoryEntity : INameHaver
{
    bool CanBeTraded { get; }
    int TypeId { get; }
    string DisplayString { get; }
    Type TypeGroup { get; }
    DateTime DateAcquired { get; set; }

    string Description { get; }
    Rarity Rarity { get; }


    UserData? UserData { get; set; }

    string ImageUrl { get; }


    long UserDataId { get; set; }
}