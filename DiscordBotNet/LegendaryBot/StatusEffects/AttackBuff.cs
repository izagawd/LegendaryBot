using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.StatusEffects;

public class AttackBuff : StatusEffect, IStatsModifier
{


    public override string Description => "Increases the caster's attack by 50%";


    public override int MaxStacks => 1;

    public override StatusEffectType EffectType => StatusEffectType.Buff;





    public IEnumerable<StatsModifierArgs> GetAllStatsModifierArgs()
    {
        yield return new AttackPercentageModifierArgs
            {
                CharacterToAffect = Affected,
                ValueToChangeWith = 50,
  
            };
    }
}