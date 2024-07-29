using System.Collections.Immutable;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.StatusEffects;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Results;

public enum AttackTargetType
{
    None, SingleTarget, AOE, InBetween, 
}
public class MoveUsageResult
{
    public Character User => Move.User;
    /// <summary>
    /// If not null, this text will be used as the main text
    /// </summary>
    public string? Text { get;  }
    public AttackTargetType AttackTargetType { get; }



    public IEnumerable<DamageResult> DamageResults
    {
        get
        {
            foreach (var i in UsageContext.DamageResults)
            {
                yield return i;
            }
        }
    }

    /// <summary>
    /// Determines if the usage was from a normal skill use or a follow up use.  this must be set
    /// </summary>
    public UsageType UsageType => UsageContext.UsageType;

    /// <summary>
    /// The move used to execute this skill
    /// </summary>
    public Move Move => UsageContext.Move;
  
    public UsageContext UsageContext { get; }
    public MoveUsageResult(UsageContext usageContext, AttackTargetType attackTargetType, string? text)
    {
        UsageContext = usageContext;
        AttackTargetType = attackTargetType;
        Text = text;
    }


}