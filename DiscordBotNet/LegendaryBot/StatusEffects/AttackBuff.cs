using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackBuff : StatusEffect, IStatsModifier
{
    public AttackBuff(Character caster) : base(caster)
    {
    }

    public override string Name => "Attack Buff";
    public override bool IsStackable => false;
    public override string Description => "Increases the caster's attack by 50%";


    public override StatusEffectType EffectType => StatusEffectType.Buff;


    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs(Affected, 50);
    }
}