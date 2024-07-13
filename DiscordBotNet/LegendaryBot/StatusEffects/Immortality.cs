using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Immortality : StatusEffect
{
    public override string Description =>
        "Makes the affected not able to die by preventing their hp from going below one";


    public override int MaxStacks => 1;
    public override StatusEffectType EffectType => StatusEffectType.Buff;
    public Immortality(Character caster) : base(caster)
    {
    }


}