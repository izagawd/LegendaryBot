using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Moves;

public abstract class Skill : Special
{
    public virtual bool IsPassive => false;
    public sealed override bool CanBeUsedNormally()
    {
        return base.CanBeUsedNormally() && !IsPassive;
    }

    public Skill(CharacterPartials_Character user) : base(user)
    {
        
    }
}