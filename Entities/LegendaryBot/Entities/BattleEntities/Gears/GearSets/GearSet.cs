using BasicFunctionality;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Entities.BattleEntities.Gears.GearSets;

public abstract class GearSet : INameHaver
{
    public static GearSet GetDefaultByTypeId(int typeId)
    {
       return  TypesFunction.GetDefaultObjectsAndSubclasses<GearSet>()
            .First(i => i.TypeId == typeId);
    }
    public Character Owner { get; set; } = null!;
    public bool CanUseFourPiece { get; set; }
    public abstract int TypeId { get; protected init; }

    public abstract string TwoPieceDescription { get; }

    public abstract string FourPieceDescription { get; }
    public abstract string Name { get; }
}