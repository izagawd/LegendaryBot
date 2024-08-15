using System.Collections.Concurrent;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Results;
using Functionality;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

public class MoveUsageContext
{
    public MoveUsageType MoveUsageType { get; }
    public Move Move { get; }


    public List<DamageResult> DamageResults;
    public MoveUsageContext(Move move, MoveUsageType moveUsageType)
    {
        MoveUsageType = moveUsageType;
        Move = move;
        DamageResults = [];
    }
    
}

public abstract class Move  : INameHaver
{

    /// <summary>
    /// The maximum amount this move can be enhanced to
    /// </summary>
 
    
    public virtual string IconUrl => $"{Website.DomainName}/battle_images/moves/{GetType().Name}.png";


    protected static ConcurrentDictionary<string, Image<Rgba32>> _croppedCombatImages { get; } = new();


    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {
        var url = IconUrl;
        if (!_croppedCombatImages.TryGetValue(url, out var image))
        {
            image = await BasicFunctionality.GetImageFromUrlAsync(IconUrl);
            image.Mutate(i => i
                .Resize(25, 25)
                .Draw(Color.Black, 3, new RectangleF(0, 0, 24,24)));
            _croppedCombatImages[url] = image;
        }




        return image;
    }

    /// <summary>
    /// Gets the description of the Move, based on the MoveType
    /// </summary>


    /// <summary>
    /// Gets description based on character. The description is mostly affected by the character's level
    /// </summary>
    /// <param name="level"></param>
    public abstract string GetDescription(Character character);
    
    
    /// <summary>
    /// The character who owns this move
    /// </summary>
    public  Character User { get; }

    public Move(Character user)
    {
        User = user;
    }
    /// <summary>
    /// Gets all the possible targets this move can be used on based on the owner of the move
    /// </summary>

    public abstract IEnumerable<Character> GetPossibleTargets();

    public BattleSimulator CurrentBattle => User.CurrentBattle;

    /// <summary>
    /// This is where the custom functionality of a move is created
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="moveUsageContext"></param>
    /// <param name="attackTargetType"></param>
    /// <param name="text"></param>
    protected abstract void UtilizeImplementation(Character target, MoveUsageContext moveUsageContext,
        out AttackTargetType attackTargetType, out string? text);

    /// <summary>
    /// This is where the general functionality of a move is done. It does some checks before UtilizeImplementation is called
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="moveUsageType"></param>
    public virtual MoveUsageResult Utilize(Character target, MoveUsageType moveUsageType)
    {

        var usageContext = new MoveUsageContext(this, moveUsageType);
        UtilizeImplementation(target,usageContext , out var attackTargetType, out var text);
        var moveUsageResult = new MoveUsageResult(usageContext,attackTargetType , text);
        CurrentBattle.InvokeBattleEvent(new CharacterPostUseMoveEventArgs(moveUsageResult));
        return moveUsageResult;
    }
    /// <summary>
    /// Checks if this move can be used based on the owner
    /// </summary>

    public virtual bool CanBeUsed()
    {
        return GetPossibleTargets().Any() && !User.IsOverriden;
    }
    public override string ToString()
    {

        return Name;
    }
    
    public abstract string Name { get; }
}