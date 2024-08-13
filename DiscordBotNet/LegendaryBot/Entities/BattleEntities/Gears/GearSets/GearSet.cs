using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.GearSets;




public abstract class GearSet  : INameHaver
{
    public Character Owner { get; set; }
    public bool CanUseFourPiece { get; set; }
    public abstract int TypeId { get; protected  init; }
    public abstract string Name { get; }

    public abstract string TwoPieceDescription { get; }

    public abstract string FourPieceDescription { get; }
}