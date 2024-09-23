using System.Collections.Concurrent;
using BasicFunctionality;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Results;
using PublicInfo;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Entities.LegendaryBot.Moves;

public class MoveUsageContext
{
    public List<DamageResult> DamageResults;

    public MoveUsageContext(Move move, MoveUsageType moveUsageType)
    {
        MoveUsageType = moveUsageType;
        Move = move;
        DamageResults = [];
    }

    public MoveUsageType MoveUsageType { get; }
    public Move Move { get; }
}

public abstract class Move : INameHaver
{
    public Move(Character user)
    {
        User = user;
    }

    /// <summary>
    ///     The maximum amount this move can be enhanced to
    /// </summary>


    public virtual string IconUrl => $"{Information.ApiDomainName}/battle_images/moves/{GetType().Name}.png";



    /// <summary>
    ///     The character who owns this move
    /// </summary>
    public Character User { get; }

    public BattleSimulator? CurrentBattle => User.CurrentBattle;

    public abstract string Name { get; }


    /// <summary>
    ///     Gets the description of the Move, based on the MoveType
    /// </summary>
    /// <summary>
    ///     Gets description based on character. The description is mostly affected by the character's level
    /// </summary>
    public abstract string GetDescription(Character character);

    /// <summary>
    ///     Gets all the possible targets this move can be used on based on the owner of the move
    /// </summary>
    public abstract IEnumerable<Character> GetPossibleTargets();

    /// <summary>
    ///     This is where the custom functionality of a move is created
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="moveUsageContext"></param>
    /// <param name="attackTargetType"></param>
    /// <param name="text"></param>
    protected abstract void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text);

    /// <summary>
    ///     This is where the general functionality of a move is done. It does some checks before UtilizeImplementation is
    ///     called
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="moveUsageType"></param>
    public virtual MoveUsageResult Utilize(Character target, MoveUsageType moveUsageType)
    {
        if (CurrentBattle is null)
            throw Character.NoBattleExc;
        var usageContext = new MoveUsageContext(this, moveUsageType);
        UtilizeImplementation(target, usageContext, out var attackTargetType, out var text);
        var moveUsageResult = new MoveUsageResult(usageContext, attackTargetType, text);
        CurrentBattle.InvokeBattleEvent(new CharacterPostUseMoveEventArgs(moveUsageResult));
        return moveUsageResult;
    }

    /// <summary>
    ///  Checks if this move can be used normally, which is, when the player is clicking to use it
    /// </summary>
    public virtual bool CanBeUsedNormally()
    {
        return GetPossibleTargets().Any() && !User.IsOverriden;
    }

    public override string ToString()
    {
        return Name;
    }
}