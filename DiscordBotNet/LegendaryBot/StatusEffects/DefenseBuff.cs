using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class DefenseBuff: StatusEffect, IStatsModifier
{


   

    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Buff;



    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return
            new DefensePercentageModifierArgs()
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = -50,
       
            };
    }
}