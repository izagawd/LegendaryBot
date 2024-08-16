using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackDebuff : StatusEffect, IStatsModifier
{
    public AttackDebuff(Character caster) : base(caster)
    {
    }

    public override string Name => "Attack Debuff";

    public override bool IsStackable => false;

    public override StatusEffectType EffectType => StatusEffectType.Debuff;


    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs(Affected, -50);
    }
}