using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Reflection;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Barrier = DiscordBotNet.LegendaryBot.StatusEffects.Barrier;


namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

/// <summary>
/// Don't forget to load the character with LoadAsync before using it in combat.
/// Characters can also be loaded at once if they are in a CharacterTeam and LoadAsync is called
/// from the CharacterTeam
/// </summary>
public abstract partial  class Character : BattleEntity, ISetup
{
        private static Type[] _characterTypes = Assembly.GetExecutingAssembly().GetTypes()
        .Where(i => i.IsSubclassOf(typeof(Character)) && !i.IsAbstract).ToArray();
        
        
        
        
    [NotMapped]
    public int ExpIncreaseScale { get; set; } = 1;
    public static Type[] CharacterTypes => _characterTypes.ToArray();
    public static Character[] ThreeStarCharacterExamples => _threeStarCharacterExamples.ToArray();
    public static Character[] OneStarCharacterExamples => _oneStarCharacterExamples.ToArray();
    public static Character[] TwoStarCharacterExamples => _twoStarCharacterExamples.ToArray();
    public static Character[] FourStarCharacterExamples => _fourStarCharacterExamples.ToArray();

    public static Character[] FiveStarCharacterExamples => _fiveStarCharacterExamples.ToArray();


 
    private static List<Character> _oneStarCharacterExamples = [];
    private static List<Character> _twoStarCharacterExamples = [];
    private static List<Character> _threeStarCharacterExamples = [];
    private static List<Character> _fourStarCharacterExamples = [];
    private static List<Character> _fiveStarCharacterExamples = [];

    public static Character[] CharacterExamples => _characterExamples.ToArray();

    private static List<Character> _characterExamples = [];
    static Character()
    {

        var types = CharacterTypes;
        foreach (var i in types)
        {
           
            var instance = Activator.CreateInstance(i);
            if (instance is Character characterInstance)
            {
                _characterExamples.Add(characterInstance);
            }
        }


        foreach (var character in _characterExamples)
        {
            switch (character.Rarity)
            {
                case Rarity.OneStar:
                    _oneStarCharacterExamples.Add(character);
                    break;
                case Rarity.TwoStar:
                    _twoStarCharacterExamples.Add(character);
                    break;
                case Rarity.ThreeStar:
                    _threeStarCharacterExamples.Add(character);
                    break;
                case Rarity.FourStar:
                    _fourStarCharacterExamples.Add(character);
                    break;
                case Rarity.FiveStar:
                    _fiveStarCharacterExamples.Add(character);
                    break;
                // Add more cases if needed for higher rarities
            }
        }
    }


    public void AddStatusEffects(IEnumerable<StatusEffect> statusEffects, int? effectiveness = null,
        bool announce = true)
    {
  
        var statusEffectsAsArray = statusEffects.ToArray();
        if (!announce)
        {
            foreach (var i in statusEffects)
            {
                AddStatusEffect(i, effectiveness, false);
            }
            return;
        }

        List<StatusEffect> resisted = [];
        List<StatusEffect> succeeded = [];
        List<StatusEffect> failed = [];

        foreach (var i in statusEffectsAsArray)
        {
            var result = AddStatusEffect(i, effectiveness, false);
            switch (result)
            {
                case StatusEffectInflictResult.Resisted:
                    resisted.Add(i);
                    break;
                case StatusEffectInflictResult.Succeeded:
                    succeeded.Add(i);
                    break;
                default:
                    failed.Add(i);
                    break;
            }
        }
        
        
        
        if(succeeded.Count > 0)
            CurrentBattle.AddAdditionalBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Succeeded
                ,succeeded.ToArray()));
        if(resisted.Count > 0)
            CurrentBattle.AddAdditionalBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Resisted
                ,resisted.ToArray()));
        if(failed.Count > 0)
            CurrentBattle.AddAdditionalBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Failed
                ,failed.ToArray()));
        
    }
    /// <param name="statusEffect">The status effect to add</param>
    /// <param name="effectiveness">the effectiveness of the caster. Null to ignore effect resistance</param>
    /// <returns>true if the status effect was successfully added</returns>
    public StatusEffectInflictResult AddStatusEffect(StatusEffect statusEffect,int? effectiveness = null, bool announce =  true)
    {
        var inflictResult = StatusEffectInflictResult.Failed;
        if (statusEffect is null) return StatusEffectInflictResult.Failed;
        if (IsDead) return StatusEffectInflictResult.Failed;
        var arrayOfType =
            _statusEffects.Where(i => i.GetType() == statusEffect.GetType())
                .ToArray();
        statusEffect.Affected = this;
        if (arrayOfType.Length < statusEffect.MaxStacks)
        {
            bool added = false;
            if (effectiveness is not null && statusEffect.EffectType == StatusEffectType.Debuff)
            {
                var percentToResistance =Resistance -effectiveness;
                
                if (percentToResistance < 0) percentToResistance = 0;
                if (!BasicFunctionality.RandomChance((int)percentToResistance))
                {
                    added = _statusEffects.Add(statusEffect);
                    
                }
                
            }
            else
            {
                added = _statusEffects.Add(statusEffect);
                
            }
            inflictResult = StatusEffectInflictResult.Resisted;
            if (added) 
                inflictResult =  StatusEffectInflictResult.Succeeded;
            if (announce)
            {
                CurrentBattle.AddAdditionalBattleText(new StatusEffectInflictBattleText(this,inflictResult, statusEffect));
            }
            return inflictResult;


        }
        if (!statusEffect.IsStackable && arrayOfType.Any())
        {
            var onlyStatus = arrayOfType.First();

            onlyStatus.OptimizeWith(statusEffect);
            CurrentBattle.AddAdditionalBattleText(new StatusEffectInflictBattleText(this,StatusEffectInflictResult.Succeeded, statusEffect));


            return StatusEffectInflictResult.Succeeded;
        }
        inflictResult = StatusEffectInflictResult.Failed;
        if (announce)
        {
            CurrentBattle.AddAdditionalBattleText(new StatusEffectInflictBattleText(this,inflictResult, statusEffect));
        }
        return inflictResult;
    }
    /// <summary>
    /// Dispells (removes) a debuff from the character
    /// </summary>
    /// <param name="statusEffect">The status effect to remove</param>
    /// <param name="effectiveness">If not null, will do some rng based on effectiveness to see whether or not to dispell debuff</param>
    /// <returns>true if status effect was successfully dispelled</returns>
    public bool DispellStatusEffect(StatusEffect statusEffect, int? effectiveness = null)
    {
        if (effectiveness is null || statusEffect.EffectType == StatusEffectType.Debuff)
            return _statusEffects.Remove(statusEffect);

        if (!BattleFunctionality.CheckForResist(effectiveness.Value,Resistance))
        {
            return _statusEffects.Remove(statusEffect);
        }
        return false;
        
    }
    [NotMapped]
    public virtual bool IsInStandardBanner => true;

    /// <summary>
    /// Increases combat readiness
    /// </summary>
    /// <param name="increaseAmount"> the amount to increase</param>
    /// <param name="announceIncrease">whetheer or not to announce the fact that combat readiness was increased</param>
    /// <returns>The amount of combat readiness increased</returns>
    public int IncreaseCombatReadiness(int increaseAmount, bool announceIncrease = true)
    {
        if (increaseAmount < 0) throw new ArgumentException("Increase amount should be at least 0");
        CombatReadiness += increaseAmount;
        if(announceIncrease && increaseAmount > 0)
            CurrentBattle.AddAdditionalBattleText(new CombatReadinessChangeBattleText(this, increaseAmount));
        return increaseAmount;
    }

    
    /// <param name="decreaseAmount">amount to decrease</param>
    /// <param name="effectiveness">Use if it is resistable</param>
    /// <param name="announceDecrease">Whether or not to announce the fact that combat readiness was decreased</param>
    /// <returns>the amount decreased</returns>
    public int DecreaseCombatReadiness(int decreaseAmount, int? effectiveness = 0, bool announceDecrease = true)
    {
        
        if(decreaseAmount < 0)throw new ArgumentException("Decrease amount should be at least 0");
        if (effectiveness is not null)
        {
            var percentToResist = Resistance - effectiveness.Value;
            
            if (BasicFunctionality.RandomChance(percentToResist))
            {
                if(announceDecrease)
                    CurrentBattle.AddAdditionalBattleText($"{NameWithAlphabetIdentifier} resisted combat readiness decrease!");
                return 0;
            }
        }

        CombatReadiness -= decreaseAmount;
        if(announceDecrease && decreaseAmount > 0)
            CurrentBattle.AddAdditionalBattleText(new CombatReadinessChangeBattleText(this, -decreaseAmount));
        return decreaseAmount;
    }

    /// <summary>
    /// Blessing currently equipped by character. Character of blessing must be set to this if useed in battle
    /// </summary>
    public  Blessing? Blessing { get; set; }


    public Barrier? Shield => _statusEffects.OfType<Barrier>().FirstOrDefault();

    [NotMapped]
    public IEnumerable<Move> MoveList
    {
        get
        {
            yield return BasicAttack;
            if (Skill is not null) yield return Skill;
            if (Ultimate is not null) yield return Ultimate;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">the type of stats modifier you want</typeparam>
    public IEnumerable<T> GetAllStatsModifierArgs<T>() where T : StatsModifierArgs
    {
       
        if (CurrentBattle is not null)
        {
            return CurrentBattle
                .GetAllStatsModifierArgsInBattle()
                .OfType<T>()
                .Where(i => i.CharacterToAffect == this);
        }

        return [];
    }
    

    [NotMapped]
    private int _health = 1;
    [NotMapped]
    private float _combatReadiness;
    
    public bool IsDead => Health <= 0;
    [NotMapped]
    public CharacterTeam Team { get; set; }
    [NotMapped]
    public float CombatReadiness
    {
        get => _combatReadiness;
        set
        {
            _combatReadiness = value;
            if (_combatReadiness > 100) _combatReadiness = 100;
            if (_combatReadiness < 0) _combatReadiness = 0;
        }
    } 

    [NotMapped]
    public virtual int Health { 
        get => _health;
        set
        {
            if (_health <= 0 )return;
            var tempMaxHealth = MaxHealth;

            if (value <= 0 && _statusEffects.Any(i => i is Immortality))
                value = 1;
            _health = value;
            
            if (_health <= 0)
            {
                _health = 0;
                CurrentBattle.AddAdditionalBattleText(new DeathBattleText(this));
                _statusEffects.Clear();
                CurrentBattle.InvokeBattleEvent(new CharacterDeathEventArgs(this));
                
            }
            if(_health > tempMaxHealth) _health = tempMaxHealth;
        }
    }

    /// <summary>
    /// Revives a character from the dead.
    /// </summary>
    public void Revive()
    {
        if(!IsDead) return;
        _health = 1;
        CurrentBattle.AddAdditionalBattleText(new ReviveBattleText(this));
        CurrentBattle.InvokeBattleEvent(new CharacterReviveEventArgs(this));
    }

    private bool _shouldTakeExtraTurn;
    [NotMapped]
    public bool ShouldTakeExtraTurn
    {
        get => _shouldTakeExtraTurn;
        set
        {
            if (value)
                throw new Exception("You cannot set this property to true. only false. " +
                                    $"If u want to make this property true, use the {nameof(GrantExtraTurn)} method instead");
            _shouldTakeExtraTurn = value;
        }
        
    }
    public CharacterBuild EquippedCharacterBuild { get;  set; }
    

    public List<CharacterBuild> CharacterBuilds { get; protected set; } = [];
    public virtual float GetMaxHealthValue(int points)
    {
        return CalculateStat(3000, 20000, points);
    }
    public virtual float GetAttackValue(int points)
    {
        return CalculateStat(700, 5000, points);
    }

    public virtual float GetSpeedValue(int points)
    {
        return CalculateStat(100, 300, points);
    }

    public virtual float GetEffectivenessValue(int points)
    {

        return CalculateStat(0, 300, points);
    }
    public virtual float GetResistanceValue(int points)
    {
        return CalculateStat(0, 300, points);
    }
    public virtual float GetCriticalChanceValue(int points)
    {
        return CalculateStat(20, 100, points);
    }
    public virtual float GetCriticalDamageValue(int points)
    {
        return CalculateStat(150, 350, points);
    }
    public virtual float GetDefenseValue(int points)
    {
        return CalculateStat(300, 2000, points);
    }



    public void Setup()
    {
        EquippedCharacterBuild = new CharacterBuild(){BuildName = "Build 1"};
        CharacterBuilds.Add(EquippedCharacterBuild);
        CharacterBuilds.Add(new CharacterBuild(){BuildName = "Build 2"});
        CharacterBuilds.Add(new CharacterBuild(){BuildName = "Build 3"});
        CharacterBuilds.Add(new CharacterBuild(){BuildName = "Build 4"});

    }
    /// <summary>
    /// Grants a character an extra turn
    /// </summary>
    public void GrantExtraTurn()
    {
        if(IsDead) return;
        _shouldTakeExtraTurn = true;
        CurrentBattle.AddAdditionalBattleText(new ExtraTurnBattleText(this));
    }
    public override string ImageUrl =>$"{Website.DomainName}/battle_images/characters/{GetType().Name}.png";
    public float ShieldPercentage
    {
        get
        {
            var shield = Shield;
            if (shield is null) return 0;
            
            return (float)(shield.GetShieldValue(this) * 1.0 / MaxHealth * 100.0);
        }
    }

    public float GetStatFromType(StatType statType)
    {
        switch (statType)
        {
            case StatType.Attack:
                return Attack;
            case StatType.Defense:
                return Defense;
            case StatType.Effectiveness:
                return Effectiveness;
            case StatType.Resistance:
                return Resistance;
            case StatType.Speed:
                return Speed;
            case StatType.CriticalChance:
                return CriticalChance;
            case StatType.CriticalDamage:
                return CriticalDamage;
            case StatType.MaxHealth:
                return MaxHealth;
            default:
                throw new Exception($"Stattype {statType} does not exist");
        }
    }

    [NotMapped]
    public Alphabet AlphabetIdentifier => CurrentBattle.GetAlphabetIdentifier(this);
    public float HealthPercentage => (float)(Health * 1.0 / MaxHealth * 100.0);
    
    /// <summary>
    /// Load build if this character isnt already loaded, or dont load the build if u set stats manually <br/>
    /// eg TotalAttack = 5000;
    /// </summary>
    /// <param name="loadBuild"></param>
    /// <returns></returns>
    public virtual async Task<Image<Rgba32>> GetDetailsImageAsync(bool loadBuild)
    {
        using var characterImageInfo = await GetInfoAsync();
        if(loadBuild)
            LoadBuild();
        
        var image = new Image<Rgba32>(850, 900);
        
        characterImageInfo.Mutate(i => i.Resize(500,150));
        var characterImageSize = characterImageInfo.Size;
        IImageProcessingContext imageCtx = null!;
        image.Mutate(i => imageCtx = i);
        var characterBuild = EquippedCharacterBuild;
        if (characterBuild is null)
        {
            characterBuild = new CharacterBuild
            {
                Character = this
            };
        }

        var nameRichText = new RichTextOptions(SystemFonts.CreateFont(Bot.GlobalFontName, 30));
        var textSize = TextMeasurer.MeasureSize(characterBuild.BuildName, nameRichText);
        nameRichText.Origin = new Vector2((image.Width / 2.0 - textSize.Width / 2.0).Round(), 20 + characterImageSize.Height);
        imageCtx.DrawText(nameRichText,characterBuild.BuildName,
            SixLabors.ImageSharp.Color.Black);
        
        int yOffSet = characterImageSize.Height + 25;
        yOffSet +=(int) textSize.Height;
         
        RichTextOptions options = new RichTextOptions(SystemFonts.CreateFont(Bot.GlobalFontName, 25)){WrappingLength = 500};
        var color = SixLabors.ImageSharp.Color.Black;

       
        foreach (var i in MoveList)
        {
            using var moveImage = await i.GetImageForCombatAsync();
            moveImage.Mutate(j => j
                .Resize(50,50)
            );
            int xOffset = 150;

            var description = i.GetDescription(this);
            if (i is Special special)
                description += $" (Cooldown: {special.MaxCooldown} turns)";
            imageCtx.DrawImage(moveImage, new Point(xOffset, yOffSet), new GraphicsOptions());
    
           
            options.Origin = new Vector2(60 + xOffset,   yOffSet);

            var fontRectangle = TextMeasurer.MeasureSize(description, options);

            imageCtx.DrawText(options, description,color );
 
            var max = float.Max(moveImage.Height, fontRectangle.Height);
            yOffSet += (15 + max).Round();
            
        }

        yOffSet += 20;
        options.Font = SystemFonts.CreateFont(Bot.GlobalFontName, 25);
        options.Origin = new Vector2(150, yOffSet);
        var statsStringBuilder = new StringBuilder();
        foreach (var i in Enum.GetValues<StatType>())
        {
            statsStringBuilder.Append($"{BasicFunctionality.Englishify(i.ToString())}: {GetStatFromType(i)}\n");
        }

        imageCtx.DrawText(options, statsStringBuilder.ToString() , color);

        options.Origin = new Vector2(500, yOffSet);

        
        var buildString = characterBuild.ToString();
        imageCtx.DrawText(options,  buildString, color);
        imageCtx.BackgroundColor(Color.ToImageSharpColor());
        var characterXOffset = 30;
        if (Blessing is null)
            characterXOffset = 250;
        imageCtx.DrawImage(characterImageInfo, new Point(characterXOffset, 20),new GraphicsOptions());
        
        if (Blessing is not null)
        {
            using var blessingImageInfo = await Blessing.GetInfoAsync();
           blessingImageInfo.Mutate(i => i.Resize(characterImageSize));

           imageCtx.DrawImage(blessingImageInfo, new Point(characterXOffset + characterImageSize.Width - 90 , 20),
               new GraphicsOptions());

        }

        var height = (yOffSet + TextMeasurer.MeasureSize(buildString, options).Height + 50).Round();
        if (height > image.Height) height = image.Height;
        var width = image.Width;
        var x = 0;
        var y = 0;
        if (Blessing is null)
        {
            width = 720;
            x = 90;
        }
        imageCtx.Crop(new Rectangle(x,y,width,height));
    
        return image;
    }
    public sealed override   Task<Image<Rgba32>> GetDetailsImageAsync()
    {
        return GetDetailsImageAsync(true);
    }


    /// <summary>
    /// Caches the cropped combat images, since cropping takes time
    /// </summary>
    private static MemoryCache _cachedCombatCroppedImages = new(new MemoryCacheOptions());

    private static readonly MemoryCacheEntryOptions EntryOptionsExpiry = new()
    {
        SlidingExpiration = new TimeSpan(0,30,0),
        PostEvictionCallbacks =
            { new PostEvictionCallbackRegistration() { EvictionCallback= BasicFunctionality.DisposeEvictionCallback } }
    };
    private static readonly MemoryCacheEntryOptions EntryOptions = new()
    {
        PostEvictionCallbacks =
            { new PostEvictionCallbackRegistration() { EvictionCallback= BasicFunctionality.DisposeEvictionCallback } }
    };
    public async Task<Image<Rgba32>> GetImageForCombatAsync()
    {

        var image = new Image<Rgba32>(190, 150);
        var url = ImageUrl;
        if (!_cachedCombatCroppedImages.TryGetValue(url, out Image<Rgba32> characterImage))
        {
            characterImage = await  BasicFunctionality.GetImageFromUrlAsync(url);
            characterImage.Mutate(ctx =>
            {
                ctx.Resize(new Size(50, 50));
            });
            var entryOptions = EntryOptions;
            
            //any image outside of the domain will n=be removed after a certain amount of time using this entry option
            if (!url.Contains(Website.DomainName)) entryOptions = EntryOptionsExpiry;
            _cachedCombatCroppedImages.Set(url,characterImage,entryOptions);
        }
 
        IImageProcessingContext ctx = null!;
        image.Mutate(idk => ctx = idk);
       
        ctx
            .DrawImage(characterImage, new Point(0, 0), new GraphicsOptions())
            .Draw(SixLabors.ImageSharp.Color.Black, 1, new Rectangle(new Point(0, 0), new Size(50, 50)))
            .DrawText($"Lvl {Level}", SystemFonts.CreateFont(Bot.GlobalFontName, 10),
        SixLabors.ImageSharp.Color.Black, new PointF(55, 21.5f))
            .Draw(SixLabors.ImageSharp.Color.Black, 1,
        new RectangleF(52.5f, 20, 70, 11.5f))
            .DrawText(Name + $" [{AlphabetIdentifier}] [{Position}]", SystemFonts.CreateFont(Bot.GlobalFontName, 11),
        SixLabors.ImageSharp.Color.Black, new PointF(55, 36.2f))
            .Draw(SixLabors.ImageSharp.Color.Black, 1,
        new RectangleF(52.5f, 35, 115, 12.5f));

        var healthPercentage = HealthPercentage;
        int width = 175;
        var shieldPercentage = ShieldPercentage;
        int filledWidth = (width * healthPercentage / 100.0).Round();
        int filledShieldWidth = (width * shieldPercentage / 100).Round();
        int barHeight = 16; 
        if(healthPercentage < 100)
            ctx.Fill(SixLabors.ImageSharp.Color.Red, new Rectangle(0, 50, width, barHeight));
        ctx.Fill(SixLabors.ImageSharp.Color.Green, new Rectangle(0, 50, filledWidth, barHeight));
        int shieldXPosition =  filledWidth;
        if (shieldXPosition + filledShieldWidth > width)
        {
            shieldXPosition = width - filledShieldWidth;
        }
        if(shieldPercentage > 0)
            ctx.Fill(SixLabors.ImageSharp.Color.White, new RectangleF(shieldXPosition, 50, filledShieldWidth, barHeight));

        // Creates a border for the health bar
        ctx.Draw(SixLabors.ImageSharp.Color.Black, 0.5f, new Rectangle(0, 50, width, barHeight));
        ctx.DrawText($"{Health}/{MaxHealth}", SystemFonts.CreateFont(Bot.GlobalFontName, 14),
        SixLabors.ImageSharp.Color.Black, new PointF(2.5f, 51.5f));

        int xOffSet = 0;
        int yOffSet = 50 + barHeight + 5;

        int moveLength = 25; 

        foreach (var i in MoveList)
        {
            //do not change size of the move image here.
            //do it in the method that gets the image
            using var moveImage = await i.GetImageForCombatAsync();
            ctx.DrawImage(moveImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += moveLength;
            int cooldown = 0;
            if (i is Special special)
            {
                cooldown = special.Cooldown;
            }

            var cooldownString = ""; 
            if (cooldown > 0)
            {
                cooldownString = cooldown.ToString();
            }
            ctx.DrawText(cooldownString, SystemFonts.CreateFont(Bot.GlobalFontName, moveLength),
                SixLabors.ImageSharp.Color.Black, new PointF(xOffSet + 5, yOffSet));
            xOffSet += moveLength;
        }
     

        xOffSet = 0;
        yOffSet += moveLength + 5;

      
        
        foreach (var i in _statusEffects.Take(16))
        {
            
            //do not change size of the status effect image here.
            //do it in the method that gets the image
            using var statusImage = await i.GetImageForCombatAsync();
            var statusLength = statusImage.Size.Width;
            if (xOffSet + statusLength + 2 >= 185)
            {
                xOffSet = 0;
                yOffSet += statusLength + 2;
            }
            ctx.DrawImage(statusImage, new Point(xOffSet, yOffSet), new GraphicsOptions());
            xOffSet += statusLength + 2;
        }
       
        if (IsDead)
        {
            ctx.Opacity(0.5f);
        }

        ctx.EntropyCrop(0.05f);
     

        return image;
    }





    [NotMapped] public int BaseMaxHealth    { get
        {
            var points = EquippedCharacterBuild?.MaxHealthPoints;

            return GetMaxHealthValue(points.GetValueOrDefault(0)).Round();

        }
    }

    public int MaxHealth
    {
        get
        {

            float percentage = 100;
            float originalMaxHealth = TotalMaxHealth;
            var modifiedStats =
                GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<MaxHealthPercentageModifierArgs>())
            {
                originalMaxHealth += originalMaxHealth * 0.01f * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<MaxHealthFlatModifierArgs>())
            {
                originalMaxHealth += i.ValueToChangeWith;
            }
            float flat = 0;

            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<MaxHealthPercentageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<MaxHealthFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            float newMaxHealth = originalMaxHealth  * percentage * 0.01f;
            newMaxHealth += flat;
            if (newMaxHealth < 0) newMaxHealth = 0;

            return newMaxHealth.Round();
        }
    }

    [NotMapped] public  int BaseDefense    { get
        {
            var points = EquippedCharacterBuild?.DefensePoints;

            return GetDefenseValue(points.GetValueOrDefault(0)).Round();

        }
    }
    [NotMapped] public virtual Element Element { get; protected set; } = Element.Fire;

    [NotMapped] public int BaseSpeed    { get
        {
            var points = EquippedCharacterBuild?.SpeedPoints;

            return GetSpeedValue(points.GetValueOrDefault(0)).Round();

        }
    }
    public float Speed
    {
        get
        {
            float percentage = 100;
            float originalSpeed = TotalSpeed;

            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<SpeedPercentageModifierArgs>())
            {
                originalSpeed += originalSpeed * 0.01f * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<SpeedFlatModifierArgs>())
            {
                originalSpeed += i.ValueToChangeWith;
            }
            float flat = 0;


            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<SpeedPercentageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<SpeedFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            float newSpeed = originalSpeed  * percentage * 0.01f;
            newSpeed += flat;
            if (newSpeed < 0) newSpeed = 0;

            return newSpeed;
        }
    }

    public float Defense { 
        get
        {
            float percentage = 100;
            float originalDefense = TotalDefense;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<DefensePercentageModifierArgs>())
            {
                originalDefense += originalDefense * 0.01f * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<DefenseFlatModifierArgs>())
            {
                originalDefense += i.ValueToChangeWith;
            }
            float flat = 0;

            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<DefensePercentageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<DefenseFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            float newDefense = originalDefense  * percentage * 0.01f;
            newDefense += flat;
            if (newDefense < 0) newDefense = 0;
            return newDefense;
        } 
    }

    [NotMapped]
    public int BaseAttack
    {
        get
        {
            var points = EquippedCharacterBuild?.AttackPoints;
            
            return GetAttackValue(points.GetValueOrDefault(0)).Round();

        }
    }



    public float Attack { 
        get     
        {
         
            float percentage = 100;
            float originalAttack = TotalAttack;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<AttackPercentageModifierArgs>())
            {
                originalAttack += originalAttack * 0.01f * i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<AttackFlatModifierArgs>())
            {
                originalAttack += i.ValueToChangeWith;
            }
            float flat = 0;
            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<AttackPercentageModifierArgs>())
            {
       
                percentage += i.ValueToChangeWith;
            }

            foreach (var i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<AttackFlatModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }
            float newAttack = originalAttack  * percentage * 0.01f;
            newAttack += flat;
            if (newAttack < 0) newAttack = 0;

            return newAttack;
        } 
    }

    [NotMapped]
    public  int BaseCriticalDamage  
    { get
        {
            var points = EquippedCharacterBuild?.CriticalDamagePoints;

            return GetCriticalDamageValue(points.GetValueOrDefault(0)).Round();

        }
    }

    public float CriticalDamage {
        get
        {
            float percentage = 0;
            float originalCriticalDamage = TotalCriticalDamage;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<CriticalDamageModifierArgs>())
            {
                originalCriticalDamage += i.ValueToChangeWith;
            }
            
            foreach (CriticalDamageModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<CriticalDamageModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }


            float newCriticalDamage = originalCriticalDamage + percentage;

            if (newCriticalDamage < 0) newCriticalDamage = 0;
            return newCriticalDamage;
        }
    }

    [NotMapped]
    public  int BaseEffectiveness    { get
        {
            var points = EquippedCharacterBuild?.EffectivenessPoints;

            return GetEffectivenessValue(points.GetValueOrDefault(0)).Round();

        }
    }
    [NotMapped]
    public int Resistance {
        get
        {
            {
                float percentage = 0;
                float originalResistance = TotalResistance;
                var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
                foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<ResistanceModifierArgs>())
                {
                    originalResistance += i.ValueToChangeWith;
                }

                foreach (ResistanceModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<ResistanceModifierArgs>())
                {
                    percentage +=i.ValueToChangeWith;
                }
                float newResistance = originalResistance + percentage;

                if (newResistance < 0) newResistance = 0;
                return newResistance.Round();
            }
        } 
    }
    /// <summary>
    /// Derives dialogue profile from character properties
    /// </summary>
    public DialogueProfile DialogueProfile =>
        new()
        {
            CharacterColor = Color,
            CharacterName = Name,
            CharacterUrl = ImageUrl
        };

    /// <summary>
    /// this will be used to get the items this character will drop if killed
    /// </summary>
    [NotMapped]
    public virtual IEnumerable<Reward> DroppedRewards => [];

    /// <summary>
    /// Use this if you want this character to drop any extra items
    /// </summary>
    [NotMapped]
    public virtual List<Entity> ExtraItemsToDrop { get; set; } = new();


    public async Task<Image<Rgba32>> GetInfoAsync()
    {
        using var userImage = await BasicFunctionality.GetImageFromUrlAsync(ImageUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100,100)));
        var userImagePoint = new Point(20, 20);
        var levelBarMaxLevelWidth = 250ul;
        var gottenExp = levelBarMaxLevelWidth * (Experience/(GetRequiredExperienceToNextLevel() * 1.0f));
        var levelBarY = userImage.Height - 30 + userImagePoint.Y;
        var font = SystemFonts.CreateFont(Bot.GlobalFontName, 25);
        var xPos = 135;
        image.Mutate(ctx =>
        
            ctx.BackgroundColor(Color.ToImageSharpColor())
                .DrawImage(userImage,userImagePoint, new GraphicsOptions())
                .Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(userImagePoint,userImage.Size))
                .Fill(SixLabors.ImageSharp.Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30))
               .Fill(SixLabors.ImageSharp.Color.Green, new RectangleF(130, levelBarY, gottenExp, 30))
               .Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30))
                .DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,SixLabors.ImageSharp.Color.Black,new PointF(xPos,levelBarY+2))
            .DrawText($"Name: {Name}", font, SixLabors.ImageSharp.Color.Black, new PointF(xPos, levelBarY -57))
            .DrawText($"Level: {Level}",font,SixLabors.ImageSharp.Color.Black,new PointF(xPos,levelBarY - 30))
            .Resize(1000, 300));
        

        return image;
    }



   
    /// <summary>
    /// if this character is not being controlled by the player, it will use custom AI
    /// </summary>
    /// <param name="target"></param>
    /// <param name="decision"></param>
    public virtual void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        List<BattleDecision> possibleDecisions = [BattleDecision.BasicAttack];


        if(Skill is not null && Skill.CanBeUsed())
            possibleDecisions.Add(BattleDecision.Skill);
        if(Ultimate is not null && Ultimate.CanBeUsed())
            possibleDecisions.Add(BattleDecision.Ultimate);

   
        Move move;
        BattleDecision moveDecision = BattleDecision.BasicAttack;

        moveDecision =BasicFunctionality.RandomChoice<BattleDecision>(possibleDecisions);
        move = this[moveDecision]!;
        Character[] possibleTargets = move.GetPossibleTargets().ToArray();
        possibleDecisions.Remove(moveDecision);
    

        
        target = BasicFunctionality.RandomChoice(possibleTargets);

        decision = moveDecision;

    }

    public Move? this[BattleDecision battleDecision]
    {
        get
        {
            switch (battleDecision)
            {
                case BattleDecision.Skill:
                    return Skill;
                case BattleDecision.Ultimate:
                    return Ultimate;
                case BattleDecision.BasicAttack:
                    return BasicAttack;
                default:
                    return null;
            }
        }
    }

    [NotMapped] private readonly HashSet<StatusEffect> _statusEffects = [];

    [NotMapped]
    public IEnumerable<StatusEffect> StatusEffects
    {
        get
        {
            foreach (var i in _statusEffects)
            {
                yield return i;
            }
        }
    }

    [NotMapped] public virtual DiscordColor Color { get; protected set; } = DiscordColor.Green;

    [NotMapped]
    public int Effectiveness
    {
        get
        {
            float percentage =0;
            float originalEffectiveness = TotalEffectiveness;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
            foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<EffectivenessModifierArgs>())
            {
                originalEffectiveness += i.ValueToChangeWith;
            }

            foreach (EffectivenessModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<EffectivenessModifierArgs>())
            {
                percentage +=i.ValueToChangeWith;
            }
            float newEffectiveness = originalEffectiveness  +percentage;

            if (newEffectiveness < 0) newEffectiveness = 0;
            return newEffectiveness.Round();
        }
    }

    [NotMapped] public  int BaseResistance    { get
        {
            var points = EquippedCharacterBuild?.ResistancePoints;

            return GetResistanceValue(points.GetValueOrDefault(0)).Round();

        }
    }


    [NotMapped]
    public float CriticalChance {
        get
        {
            {
                float percentage = 0;
                float originalCriticalChance = TotalCriticalChance;
                var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();
                foreach (var i in modifiedStats.Where(i => !i.WorksAfterGearCalculation).OfType<CriticalChanceModifierArgs>())
                {
                    originalCriticalChance += i.ValueToChangeWith;
                }

                foreach (CriticalChanceModifierArgs i in modifiedStats.Where(i => i.WorksAfterGearCalculation).OfType<CriticalChanceModifierArgs>())
                {
                    percentage +=i.ValueToChangeWith;
                }


                float newCriticalChance = originalCriticalChance  + percentage;

                if (newCriticalChance < 0) newCriticalChance = 0;
                return newCriticalChance;
            }
        }
        
    }


    [NotMapped]
    public  int BaseCriticalChance    { get
        {
            var points = EquippedCharacterBuild?.CriticalChancePoints;

            return GetCriticalChanceValue(points.GetValueOrDefault(0)).Round();

        }
    }

    [NotMapped] public BattleSimulator CurrentBattle => Team?.CurrentBattle!;

    public bool RemoveStatusEffect(StatusEffect statusEffect) => _statusEffects.Remove(statusEffect);

    public override int MaxLevel => 120;
    
    
    public static float CalculateStat(int initialValue,  int maxValue, int points)
    {
        var maxPoints = CharacterBuild.MaxPointsPerStat;
        // Ensure points are within the valid range (0 to maxPoints)
        points = Math.Clamp(points, 0, maxPoints);

        // Calculate the increase per point
        float increasePerPoint = (float)(maxValue - initialValue) / maxPoints;

        // Calculate the final stat value
        var finalStatValue = initialValue + (points * increasePerPoint);

        // Ensure the final value doesn't exceed the maxValue
        finalStatValue = Math.Min(finalStatValue, maxValue);

        return finalStatValue;
    }
    public void SetLevel(int level)
    {
        if (level > MaxLevel) level = MaxLevel;
        Level = level;
    }
    [NotMapped]
    public float TotalAttack { get; set; }
    [NotMapped]
    public float TotalDefense { get; set; }
    [NotMapped]
    public float TotalMaxHealth { get; set; }
    [NotMapped]
    public float TotalSpeed { get; set; }
    [NotMapped]
    public float TotalCriticalChance { get; set; }
    [NotMapped]
    public float TotalCriticalDamage { get; set; }
    [NotMapped]
    public float TotalEffectiveness { get; set; }
    [NotMapped]
    public float TotalResistance { get; set; }

    /// <summary>
    /// Use this to load the build (stats) of the character. if u want to manually set the stats of this character, just
    /// change the Total properties, and avoid calling this method. its called in load async unless u set thee
    /// bool param to false
    /// </summary>
    public virtual void LoadBuild()
    {
        TotalAttack = BaseAttack;
        TotalDefense = BaseDefense;
        TotalSpeed = BaseSpeed;
        TotalCriticalDamage = BaseCriticalDamage;
        TotalCriticalChance = BaseCriticalChance;
        TotalResistance = BaseResistance;
        TotalEffectiveness = BaseEffectiveness;
        TotalMaxHealth = BaseMaxHealth;
        if (Blessing is not null)
        {
            TotalAttack += Blessing.Attack;
            TotalMaxHealth += Blessing.Health;
        }
    }
    public sealed override async Task LoadAsync()
    {
  
        await LoadAsync(true);



    }
    
    public virtual async Task LoadAsync(bool loadBuild)
    {
        await base.LoadAsync();
        
        if(loadBuild)
            LoadBuild();
        if(TotalMaxHealth != 0)
            Health = TotalMaxHealth.Round();


    }

    public void TakeDamageWhileConsideringShield(int damage)
    {
        var shield = Shield;

        if (shield is null)
        {
            Health -= damage;
            return;
        }

        var shieldValue = shield.GetShieldValue(this);
        // Check if the shield can absorb some of the damage
        if (shieldValue > 0)
        {
            shieldValue -= damage;
            if (shieldValue < 0)
            {
                Health += shieldValue;
                shieldValue = 0;
            }
        }
        else
        {
            // If no shield, damage affects health directly
            Health -= damage;
        }
        shield.SetShieldValue(shieldValue);
    }
    public static float DamageFormula(float potentialDamage, float defense)
    {
        return (potentialDamage * 375) / (300 + defense);
    }
    /// <summary>
    /// Used to damage this character
    /// </summary>
    /// <param name="damage">The potential damage</param>
    /// <param name="damageText">if there is a text for the damage, then use this. Use $ in the string and it will be replaced with the damage dealt</param>
    /// <param name="caster">The character causing the damage</param>
    /// <param name="canCrit">Whether the damage can cause a critical hit or not</param>
    /// <param name="damageElement">The element of the damage</param>
    /// <returns>The results of the damage</returns>
    public  DamageResult Damage(DamageArgs damageArgs)
    {
        if (IsDead) throw new Exception("Cannot damage dead character");
        CurrentBattle.InvokeBattleEvent(new CharacterPreDamageEventArgs(damageArgs));
        var didCrit = false;
        var defenseToIgnore = Math.Clamp(damageArgs.DefenseToIgnore,0,100);
        var defenseToUse = (100 - defenseToIgnore) * 0.01f * Defense;
        var damageModifyPercentage = 0;
        
        var damage = DamageFormula(damageArgs.Damage, defenseToUse);

        var advantageLevel = ElementalAdvantage.Neutral;
        if(damageArgs.ElementToDamageWith is not null)
            advantageLevel = BattleFunctionality.GetAdvantageLevel(damageArgs.ElementToDamageWith.Value, Element);

        switch (advantageLevel){
            case ElementalAdvantage.Disadvantage:
                damageModifyPercentage -= 30;
                break;
            case ElementalAdvantage.Advantage:
                damageModifyPercentage += 30;
                break;
        }
    


        damage = (damage * 0.01f * (damageModifyPercentage + 100));
        var chance = damageArgs.CriticalChance;
        if (damageArgs.AlwaysCrits)
        {
            chance = 100;
        }
        if (BasicFunctionality.RandomChance(chance) && damageArgs.CanCrit)
        {

            damage *= damageArgs.CriticalDamage / 100.0f;
            didCrit = true;
        }

        int actualDamage = damage.Round();
        var damageText = damageArgs.DamageText;
        if (damageText is null)
        {
            damageText = $"{damageArgs.Caster} dealt {actualDamage} damage to {this}!";
        }

        damageText = damageText.Replace("$", actualDamage.ToString());

        switch (advantageLevel)
        {
            case ElementalAdvantage.Advantage:
                damageText = "It's super effective! " + damageText;
                break;
            case ElementalAdvantage.Disadvantage:
                damageText = "It's not that effective... " + damageText;
                break;
        }
    

        if (didCrit)
            damageText = "A critical hit! " + damageText;

    
        CurrentBattle.AddAdditionalBattleText(damageText);
        
        TakeDamageWhileConsideringShield(actualDamage);
        DamageResult damageResult;
        if (damageArgs.Move is not null)
        {
            damageResult = new DamageResult(damageArgs.Move)
            {
                WasCrit = didCrit,
                Damage = actualDamage,
                DamageDealer = damageArgs.Caster,
                DamageReceiver = this,
                CanBeCountered = damageArgs.CanBeCountered
            };
        }
        else
        {
            damageResult = new DamageResult(damageArgs.StatusEffect)
            {
                
                WasCrit = didCrit,
                Damage = actualDamage,
                DamageDealer = damageArgs.Caster,
                DamageReceiver = this,
                CanBeCountered = damageArgs.CanBeCountered
            };
        }
        CurrentBattle.InvokeBattleEvent(new CharacterPostDamageEventArgs(damageResult));
        return damageResult;
    }


    [NotMapped]
    public abstract BasicAttack BasicAttack { get; }
    
    public string GetNameWithAlphabetIdentifier(bool isEnemy)
    {
        string side = "enemy";
        if (!isEnemy)
        {
            side = "team mate";
        }
        return $"{Name} ({side}) [{AlphabetIdentifier}]";
    }
    
    public string NameWithAlphabetIdentifier => $"{Name} ({AlphabetIdentifier})";
    [NotMapped] public virtual Skill? Skill { get; } 
    /// <summary>
    /// The position of the player based on combat readiness
    /// </summary>
    public int Position => Array.IndexOf(CurrentBattle.Characters.OrderByDescending(i => i.CombatReadiness).ToArray(),this) +1;
    [NotMapped] public virtual Ultimate? Ultimate { get; }
    /// <summary>
    /// Checks if something overrides the player turn eg stun status effect preventing the player from doing anything
    /// </summary>
    public bool IsOverriden
    {
        get
        {
            return _statusEffects.Any(i => i.OverrideTurnType > 0);
        }
    }
    /// <param name="damageText">if there is a text for the damage, then use this. Use $ in the string and it will be replaced with the damage dealt</param>
  /// <param name="caster">The character causing the damage</param>
/// <param name="damage">The potential damage</param>
    public DamageResult? FixedDamage(DamageArgs damageArgs)
    {
        if (IsDead) return null;
        var damageText = damageArgs.DamageText;
        var damage = damageArgs.Damage;
        var caster = damageArgs.Caster;
        var canBeCountered = damageArgs.CanBeCountered;
        if (damageText is null)
        {
            damageText = $"{this} took $ fixed damage!";
        }
        CurrentBattle.AddAdditionalBattleText(damageText.Replace("$", damage.Round().ToString()));
        TakeDamageWhileConsideringShield(damage.Round());
        DamageResult damageResult;
        if (damageArgs.Move is not null)
        {
            damageResult = new DamageResult(damageArgs.Move)
            {
              
                WasCrit = false,
                Damage = damage.Round(),
                DamageDealer = caster, 
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
        else
        {
            damageResult = new DamageResult(damageArgs.StatusEffect)
            {
              
                WasCrit = false,
                Damage = damage.Round(),
                DamageDealer = caster, 
                DamageReceiver = this,
                CanBeCountered = canBeCountered
            };
        }
            
        CurrentBattle.InvokeBattleEvent(new CharacterPostDamageEventArgs(damageResult));
        return damageResult;
    }

    public List<PlayerTeam> PlayerTeams { get; protected set; } = [];
    /// <summary>
    /// Recovers the health of this character
    /// </summary>
    /// <param name="toRecover">Amount to recover</param>
    /// <param name="recoveryText">text to say when health recovered. use $ to represent health recovered</param>
    /// <returns>Amount recovered</returns>
    public virtual int RecoverHealth(float toRecover,
        string? recoveryText = null, bool announceHealing = true)
    {
        if (IsDead) return 0;
        var healthToRecover = toRecover.Round();

        Health += healthToRecover;
        if (recoveryText is null)
            recoveryText = $"{this} recovered $ health!";
        if(announceHealing)
            CurrentBattle.AddAdditionalBattleText(recoveryText.Replace("$",healthToRecover.ToString()));
        
        return healthToRecover;
    }




 
    public override long GetRequiredExperienceToNextLevel(int level)
    {
       return BattleFunctionality.NextLevelFormula(Level);
    }

    /// <summary>
/// Increases the Exp of a character and returns useful text
/// </summary>
/// <returns></returns>
    public override ExperienceGainResult IncreaseExp(long exp)
    {
        if (Level >= MaxLevel)
            return new ExperienceGainResult() { ExcessExperience = exp, Text = $"{this} has already reached their max level!" };
        string expGainText = "";
        
        var levelBefore = Level;
        Experience += exp;
        var nextLevelEXP =GetRequiredExperienceToNextLevel(Level);
        while (Experience >= nextLevelEXP && Level < MaxLevel)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            nextLevelEXP = GetRequiredExperienceToNextLevel(Level);
        }
        expGainText += $"{this} gained {exp} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }
        long excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }
        expGainText += "!";
        return new ExperienceGainResult(){ExcessExperience = excessExp, Text = expGainText};
    }

    public void SetExperience(long experience)
    {
        Experience = experience;
    }
    
}
public enum StatusEffectInflictResult
{
    Succeeded, Resisted, Failed
}