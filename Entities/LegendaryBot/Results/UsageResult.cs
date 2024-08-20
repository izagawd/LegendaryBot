using Entities.LegendaryBot.Moves;
using CharacterPartials_Character =
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace Entities.LegendaryBot.Results;

public enum AttackTargetType : byte
{
    None,
    SingleTarget,
    AOE,
    InBetween
}

public class MoveUsageResult
{
    public MoveUsageResult(MoveUsageContext moveUsageContext, AttackTargetType attackTargetType, string? text)
    {
        MoveUsageContext = moveUsageContext;
        AttackTargetType = attackTargetType;
        Text = text;
    }

    public CharacterPartials_Character User => Move.User;

    /// <summary>
    ///     If not null, this text will be used as the main text
    /// </summary>
    public string? Text { get; }

    public AttackTargetType AttackTargetType { get; }


    public IEnumerable<DamageResult> DamageResults
    {
        get
        {
            foreach (var i in MoveUsageContext.DamageResults) yield return i;
        }
    }

    /// <summary>
    ///     Determines if the usage was from a normal skill use or a follow up use.  this must be set
    /// </summary>
    public MoveUsageType MoveUsageType => MoveUsageContext.MoveUsageType;

    /// <summary>
    ///     The move used to execute this skill
    /// </summary>
    public Move Move => MoveUsageContext.Move;

    public MoveUsageContext MoveUsageContext { get; }
}