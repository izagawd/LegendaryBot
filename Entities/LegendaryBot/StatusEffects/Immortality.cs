using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.StatusEffects;

public class Immortality : StatusEffect
{
    public Immortality(CharacterPartials_Character caster) : base(caster)
    {
    }

    public override string Name => "Immortality";

    public override string Description =>
        "Makes the affected not able to die by preventing their hp from going below one";


    public override bool IsStackable => false;
    public override StatusEffectType EffectType => StatusEffectType.Buff;
}