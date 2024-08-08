using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class Immortality : StatusEffect
{
    public override string Name => "Immortality";
    public override string Description =>
        "Makes the affected not able to die by preventing their hp from going below one";


    public override bool IsStackable => false;
    public override StatusEffectType EffectType => StatusEffectType.Buff;


    public Immortality(Character caster) : base(caster)
    {
    }
}