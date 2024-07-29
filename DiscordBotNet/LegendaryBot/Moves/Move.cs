using System.Collections.Concurrent;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Results;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Moves;

public class UsageContext
{
    public UsageType UsageType { get; }
    public Move Move { get; }


    public List<DamageResult> DamageResults;
    public UsageContext(Move move, UsageType usageType)
    {
        UsageType = usageType;
        Move = move;
        DamageResults = [];
    }
    
}

public abstract class Move 
{
    /// <summary>
    /// The maximum amount this move can be enhanced to
    /// </summary>
 
    
    public virtual string IconUrl { get; }

    public Move()
    {
        IconUrl = $"{Website.DomainName}/battle_images/moves/{GetType().Name}.png";
        Name =  BasicFunctionality.Englishify(GetType().Name);
    }
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
    public  Character User { get; set; }


    /// <summary>
    /// Gets all the possible targets this move can be used on based on the owner of the move
    /// </summary>

    public abstract IEnumerable<Character> GetPossibleTargets();

    public BattleSimulator CurrentBattle => User?.CurrentBattle;

    /// <summary>
    /// This is where the custom functionality of a move is created
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="usageContext"></param>
    /// <param name="attackTargetType"></param>
    /// <param name="text"></param>
    protected abstract void UtilizeImplementation(Character target, UsageContext usageContext,
        out AttackTargetType attackTargetType, out string? text);

    /// <summary>
    /// This is where the general functionality of a move is done. It does some checks before UtilizeImplementation is called
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="usageType"></param>
    public virtual MoveUsageResult Utilize(Character target, UsageType usageType)
    {

        var usageContext = new UsageContext(this, usageType);
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



    public virtual string Name { get; }


}