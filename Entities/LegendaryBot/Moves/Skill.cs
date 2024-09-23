
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.Moves;

public abstract class Skill : Special
{
    public virtual bool IsPassive => false;
    public sealed override bool CanBeUsedNormally()
    {
        return base.CanBeUsedNormally() && !IsPassive;
    }

    public Skill(Character user) : base(user)
    {
        
    }
}