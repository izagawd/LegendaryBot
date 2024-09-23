using System.Collections.Concurrent;
using System.Reflection;
using BasicFunctionality;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using PublicInfo;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using 
    Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace Entities.LegendaryBot.StatusEffects;

public abstract class StatusEffect : INameHaver
{
    private static readonly ConcurrentDictionary<string, Image<Rgba32>> _cachedResizedCombatImages = new();

    static StatusEffect()
    {
    }

    public StatusEffect(Character caster)
    {
        Caster = caster;
    }

    public virtual string IconUrl => $"{Information.ApiDomainName}/battle_images/status_effects/{GetType().Name}.png";

    /// <summary>
    ///     The character currently affected by the status effect
    /// </summary>
    public Character Affected { get; set; } = null!;

    public virtual string Description => "Does the bla bla bla of the bla bla bla";


    /// <summary>
    ///     Returns true if the status effect is executed after the character's turn
    /// </summary>
    public virtual bool ExecuteStatusEffectAfterTurn => true;

    /// <summary>
    ///     Returns true if the status effect is executed before the character's turn
    /// </summary>
    public bool ExecuteStatusEffectBeforeTurn => !ExecuteStatusEffectAfterTurn;

    /// <summary>
    ///     Returns true if the status effect can be on a character more than once
    /// </summary>
    public abstract bool IsStackable { get; }

    public virtual StatusEffectType EffectType => StatusEffectType.Buff;

    /// <summary>
    ///     With an override turn type of enum number 1 or more, the status effect can modify the user's decision for a turn.
    ///     if there is more than one status effect with an override turn type enum number at least 1, it is the status effect
    ///     with the
    ///     highest override turn type enum number that will take effect
    /// </summary>
    public virtual OverrideTurnType OverrideTurnType => OverrideTurnType.None;

    /// <summary>
    ///     The duration of the status effect
    /// </summary>
    public int Duration { get; set; } = 1;

    /// <summary>
    ///     The person who casted the status effect
    /// </summary>
    public Character Caster { get; }

    /// <summary>
    ///     The name of the status effect
    /// </summary>
    public abstract string Name { get; }


    /// <summary>
    ///     Called when this status effect has been added to a character. use this instead of constructor
    ///     before this method is called, the <see cref="Affected" /> and <see cref="Caster" /> variables would have been set
    /// </summary>
    public virtual void OnAdded()
    {
    }


    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {
        var url = IconUrl;

        if (!_cachedResizedCombatImages.TryGetValue(url, out var image))
        {
            image = await ImageFunctions.GetImageFromUrlAsync(IconUrl);

            var backgroundColor = Color.Red;
            if (EffectType == StatusEffectType.Buff) backgroundColor = Color.ParseHex("#67B0D8");
            image.Mutate(ctx =>
            {
                ctx.BackgroundColor(backgroundColor);
                ctx.Resize(new Size(20, 20));
            });

            _cachedResizedCombatImages[url] = image;
        }

        image = image.Clone();

        image.Mutate(ctx =>
        {
            var x = 1;
            var xOffset = 0;
            var duration = Duration.ToString();
            if (duration.Length > 1)
            {
                x = 0;
                xOffset = 3;
            }

            ctx.Fill(Color.Black, new RectangleF(0, 0, 9 + xOffset, 9));
            var font = SystemFonts.CreateFont(Information.GlobalFontName, 9, FontStyle.Bold);

            ctx.DrawText(Duration.ToString(), font, Color.White, new PointF(x, 0));
        });

        return image;
    }


    [DefaultObjectRegisterer]
    private static IEnumerable<object> RegisteringDefaultObjects()
    {
        foreach (var i in TypesFunction.AllTypes.Where(i => i.IsAssignableTo(typeof(StatusEffect)) && !i.IsAbstract))
        {
            var created = (StatusEffect)i.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    [typeof(Character)], null)
                .Invoke(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, [new PlayerMale()],
                    null);
            yield return created;
        }
    }

    /// <summary>
    ///     When status effect optimizing has occured this will be called. it will use the returned status effect.
    /// </summary>
    /// <param name="statusEffect">The status effect to optimize with</param>
    public virtual StatusEffect OptimizeWith(StatusEffect statusEffect)
    {
        if (statusEffect.Duration > Duration)
            Duration = statusEffect.Duration;
        return this;
    }


    public virtual string? OverridenUsage(ref Character target, ref BattleDecision decision,
        MoveUsageType moveUsageType) // the status effect might or might not replace the player's decision
    {
        return null;
    }

    /// <summary>
    /// </summary>
    /// <returns>if the status effect has any additional texts it will return a string if not it returns null</returns>
    public virtual void PassTurn()
    {
        Duration -= 1;
    }


    public override string ToString()
    {
        return Name;
    }
}