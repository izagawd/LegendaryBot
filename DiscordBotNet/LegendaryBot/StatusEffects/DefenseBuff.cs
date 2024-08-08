using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseBuff: StatusEffect, IStatsModifier
{
    public override string Name => "Defense Buff";
    public override bool IsStackable => false;
   



    public override StatusEffectType EffectType => StatusEffectType.Buff;



    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return
            new DefensePercentageModifierArgs()
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = 50,
       
            };
    }

    public DefenseBuff(Character caster) : base(caster)
    {
    }
}