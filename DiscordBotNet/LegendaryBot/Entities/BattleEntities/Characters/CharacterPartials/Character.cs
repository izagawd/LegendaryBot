using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.DialogueNamespace;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.GearSets;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Moves;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DiscordBotNet.LegendaryBot.StatusEffects;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Barrier = DiscordBotNet.LegendaryBot.StatusEffects.Barrier;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;



public class CharacterDatabaseConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> entity)
    {

        entity.HasKey(i => i.Id);
        entity.HasIndex(i => new{i.Number, i.UserDataId})
            .IsUnique();
        // generated on add even though a trigger handles it, just in case the trigger doesn't work
        entity.Property(i => i.Number)
            .ValueGeneratedOnAdd();
        
        entity.HasMany(i => i.Gears)
            .WithOne(i => i.Character)
            .HasForeignKey(i => i.CharacterId);
        
        entity.HasOne(i => i.Blessing)
            .WithOne(i => i.Character)
            .HasForeignKey<Blessing>(i => i.CharacterId)
            .OnDelete(DeleteBehavior.SetNull);
        entity.Property(i => i.Level)
            .HasColumnName(nameof(Character.Level));
        entity.Property(i => i.Experience)
            .HasColumnName(nameof(Character.Experience));
        var starting = entity.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Character>())
        {
            starting = starting.HasValue(i.GetType(), i.TypeId);
        }
    }
}

/// <summary>
/// Don't forget to load the character with LoadTeamGearWithPlayerDataAsync before using it in combat.
/// Characters can also be loaded at once if they are in a CharacterTeam and LoadTeamGearWithPlayerDataAsync is called
/// from the CharacterTeam
/// </summary>
public abstract partial class Character : IInventoryEntity, ICanBeLeveledUp, IGuidPrimaryIdHaver
{


    
    /// <summary>
    /// if not null, will assume this character uses super points.
    /// </summary>
    [NotMapped]
    public virtual bool UsesSuperPoints => false;

    [NotMapped]
    public int SuperPoints { get; set; }
    public virtual bool CanSpawnNormally => true;
    public bool CannotDoAnything => IsDead || HighestOverrideTurnType >= OverrideTurnType.CannotMove;
    public virtual string? PassiveDescription => null;
    public OverrideTurnType HighestOverrideTurnType
    {
        get
        {
            if (!_statusEffects.Any())
                return OverrideTurnType.None;
            return _statusEffects.Select(i => i.OverrideTurnType).Max();
        }
    }

    public long GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }

    public int Number { get; set; }

 
    public int TypeId { get; protected init; }

    public string DisplayString
    {
        get
        {
            var stringg = $"{Name} • Lvl: {Level}";
            if (Number != 0)
            {
                stringg = $"{Number} • {stringg}";
            }
            if (Blessing is not null)
            {
                stringg += $" • Blessing: {Blessing.Name}";
            }

            return $"`{stringg}`";
        }
    }

    /// <summary>
    /// Increases combat readiness
    /// </summary>
    /// <param name="increaseAmount"> the amount to increase</param>
    /// <param name="announceIncrease">whetheer or not to announce the fact that combat readiness was increased</param>
    /// <returns>The amount of combat readiness increased</returns>
    public float IncreaseCombatReadiness(float increaseAmount, bool announceIncrease = true)
    {
        if (increaseAmount < 0) throw new ArgumentException("Increase amount should be at least 0");
        CombatReadiness += increaseAmount;
        if(announceIncrease && increaseAmount > 0)
            CurrentBattle.AddBattleText(new CombatReadinessChangeBattleText(this, increaseAmount));
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
                    CurrentBattle.AddBattleText($"{NameWithAlphabet} resisted combat readiness decrease!");
                return 0;
            }
        }

        CombatReadiness -= decreaseAmount;
        if(announceDecrease && decreaseAmount > 0)
            CurrentBattle.AddBattleText(new CombatReadinessChangeBattleText(this, -decreaseAmount));
        return decreaseAmount;
    }

    /// <summary>
    /// Blessing currently equipped by character. Character of blessing must be set to this if useed in battle
    /// </summary>
    public  Blessing? Blessing { get; set; }


    public Barrier? Shield => _statusEffects.OfType<Barrier>().FirstOrDefault();

    [NotMapped]
    public virtual IEnumerable<Move> MoveList
    {
        get
        {
            if (BasicAttack is not null) yield return BasicAttack;
       
            if (Skill is not null) yield return Skill;
            if (Ultimate is not null) yield return Ultimate;
        }
    }


    [NotMapped]
    private float _health = 1;
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
    public virtual float Health { 
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
            
                CurrentBattle.AddBattleText(new DeathBattleText(this));
                ShouldTakeExtraTurn = false;
                _statusEffects.Clear();
                CurrentBattle.InvokeBattleEvent(new CharacterDeathEventArgs(this));
            
            }

            if (_health > tempMaxHealth) _health = tempMaxHealth;
        }
    }

    /// <summary>
    /// Revives a character from the dead.
    /// </summary>
    public void Revive()
    {
        if(!IsDead) return;
        _health = 1;
        CurrentBattle.AddBattleText(new ReviveBattleText(this));
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

    public List<Gear> Gears { get; set; } = [];



    /// <summary>
    /// Grants a character an extra turn
    /// </summary>
    public void GrantExtraTurn()
    {
        if(IsDead) return;
        _shouldTakeExtraTurn = true;
        CurrentBattle.AddBattleText(new ExtraTurnBattleText(this));
    }


    public UserData? UserData { get; set; }


    public long Id { get; set; }
    public ulong UserDataId { get; set; }

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




   

    public float MaxHealth
    {
        get
        {
            float percentage = 100;
            var modifiedStats = GetAllStatsModifierArgs<StatsModifierArgs>().ToArray();

            float flat = 0;

            foreach (var i in modifiedStats.OfType<MaxHealthPercentageModifierArgs>())
            {
                percentage += i.ValueToChangeWith;
            }
            foreach (var i in modifiedStats.OfType<MaxHealthPercentageModifierArgs>())
            {
                flat += i.ValueToChangeWith;
            }

            var newMaxHealth = TotalMaxHealth * percentage * 0.01f;
            newMaxHealth += flat;
            if (newMaxHealth < 0) newMaxHealth = 0;
            return newMaxHealth;
        }
    }

    /// <summary>
    /// Stats to increase when reaching level milestone. order matters in this case, and it should be exactly 5
    /// </summary>
    protected virtual IEnumerable<StatType> LevelMilestoneStatIncrease =>
    [
        StatType.MaxHealth, StatType.CriticalChance,
        StatType.Attack, StatType.Effectiveness, StatType.Resistance, StatType.Speed
    ];


    public IEnumerable<StatType> GetStatsToIncreaseBasedOnLevelMilestone(int levelMilestone)
    {
        if (levelMilestone > 6)
        {
            levelMilestone = 6;
            Console.WriteLine("level milestone cannot be more than 6, cuz max level for a players character is 60, and milestones are reached every 10 levels");
        }
        foreach (var i in LevelMilestoneStatIncrease)
        {
            levelMilestone--;
            if(levelMilestone < 0) yield break;
            yield return i;
        }
    }

    [NotMapped] public virtual Element Element { get; protected set; } = Element.Fire;








    public abstract string Name { get; }

    public virtual string ImageUrl => $"{Website.DomainName}/battle_images/characters/{GetType().Name}.png";
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

    public int Level { get; set; } = 1;
    public virtual long CoinsToGainWhenKilled => (Level + 50) * (int) Rarity;

    public virtual long ExpToGainWhenKilled
    {
        get
        {
            
            var averageFormulaTillNextLevel = GetRequiredExperienceToNextLevel(Level);
            
            var powCalculator = Math.Pow(1.05f, Level);
            var rarityMuliplier = 1 + ((int)Rarity * 0.2);
            var computed = ((averageFormulaTillNextLevel / powCalculator) * rarityMuliplier * 0.75f).RoundLong();
            return computed;

        }
    }





    public static long GetCoinsBasedOnCharacters(IEnumerable<Character> characters)
    {
        return characters.Select(i => i.CoinsToGainWhenKilled).Sum();
    }
    [NotMapped]
    public virtual int ExpIncreaseScale => 1;
    public static long GetExpBasedOnDefeatedCharacters(IEnumerable<Character> characters)
    {
        return characters.Select(i => i.ExpToGainWhenKilled).Sum();
    }


    public void SetBotStatsAndLevelBasedOnTier(Tier tier)
    {
        var level = 5;
       
        var attack = 100;
        var defense = 90;
        var health = 1500;
        var speed = 80;
     
        
 
        switch (tier)
        {
            case Tier.Bronze:
                break;
            case Tier.Silver:
                level = 15;
           
                attack = 300;
                defense = 250;
                health = 3000;
                speed = 90;
                break;
            case Tier.Gold:
                level = 25;
              
                attack = 1000;
                defense = 350;
                health = 6000;
                speed = 100;
                break;
            case Tier.Platinum:
                level = 35;
              
                attack = 2500;
                defense = 600;
                health = 10000;
                speed = 115;
                break;
            case Tier.Diamond:
                level = 45;
            
                attack = 3750;
                defense = 800;
                health = 13000;
                speed = 130;
                break;
            case Tier.Divine:
                level = 55;
         
                attack = 5000;
                defense = 1000;
                health = 20000;
                speed = 150;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        Level = level;
        TotalAttack = attack * BaseAttackMultiplier;
        TotalDefense = defense * BaseDefenseMultiplier;
        TotalSpeed = speed * BaseSpeedMultiplier;
        TotalMaxHealth = health * BaseMaxHealthMultiplier;
        

    }
    /// <summary>
    /// this will be used to get the items this character will drop if killed
    /// </summary>
    [NotMapped]
    public virtual IEnumerable<Reward> DroppedRewards
    {
        get {yield break; }
    }





    public override string ToString()
    {
        return CurrentBattle is not null ? NameWithAlphabet : Name;
    }


    /// <summary>
    /// if this character is not being controlled by the player, it will use custom AI
    /// </summary>
    /// <param name="target"></param>
    /// <param name="decision"></param>
    public virtual void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        if ((Ultimate?.CanBeUsed()).GetValueOrDefault(false))
        {
            decision = BattleDecision.Ultimate;
            target = BasicFunctionality.RandomChoice(Ultimate!.GetPossibleTargets());
            return;
        }
        if ((Skill?.CanBeUsed()).GetValueOrDefault(false))
        {
            decision = BattleDecision.Skill;
            target = BasicFunctionality.RandomChoice(Skill!.GetPossibleTargets());
            return;
        }

        target = BasicFunctionality.RandomChoice(BasicAttack.GetPossibleTargets());
        decision = BattleDecision.BasicAttack;

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
    
    public IEnumerable<GearSet> GenerateGearSets()
    {
        foreach (var i in Gears.GroupBy(i => i.GearSetType))
        {
            var count = i.Count();
            if(count < 2) continue;
            var created = (GearSet)Activator.CreateInstance(i.Key)!;
            created.Owner = this;
            if (count >= 4)
                created.CanUseFourPiece = true;
            yield return created;
        }
    }

    [NotMapped] private readonly HashSet<StatusEffect> _statusEffects = [];


    [NotMapped] public IEnumerable<StatusEffect> StatusEffects => _statusEffects;

    [NotMapped] public virtual DiscordColor Color => DiscordColor.Green;


    

    [NotMapped]
    private List<GearSet> _gearSets = [];

    [NotMapped]
    public IEnumerable<GearSet> GearSets => _gearSets;

    [NotMapped] public BattleSimulator CurrentBattle => Team?.CurrentBattle!;

    public bool RemoveStatusEffect(StatusEffect statusEffect) => _statusEffects.Remove(statusEffect);

    public int MaxLevel => 60;
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
    public virtual void LoadStats()
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

        foreach (var i in Gears)
        {
            foreach (var j in i.Substats)
            {
                j.AddStats(this);
            }
            i.MainStat.SetMainStatValue(i.Rarity);
            i.MainStat.AddStats(this);
        }
        _gearSets = GenerateGearSets().ToList();
    }



    public  Type TypeGroup => typeof(Character);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public virtual string Description => "";
    public virtual Rarity Rarity => Rarity.ThreeStar;


    [NotMapped]
    public   BasicAttack BasicAttack { get; protected set; }
    
    public string GetNameWithAlphabetIdentifier(bool isEnemy)
    {
        var side = "enemy";
        if (!isEnemy)
        {
            side = "team mate";
        }
        return $"{Name} [{side}] [{AlphabetIdentifier}]";
    }
    
    public string NameWithAlphabet => $"{Name} [{AlphabetIdentifier}]";
    [NotMapped] public  Skill? Skill { get; protected set; } 
    /// <summary>
    /// The position of the player based on combat readiness
    /// </summary>
    public int Position => Array.IndexOf(CurrentBattle.Characters.OrderByDescending(i => i.CombatReadiness).ToArray(),this) +1;
    [NotMapped] public  Ultimate? Ultimate { get; protected set; }
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

    





 
    public long GetRequiredExperienceToNextLevel(int level)
    {
       return BattleFunctionality.NextLevelFormula(Level);
    }

    public virtual bool CanBeTraded => true;


    public long Experience { get; set; }
    /// <summary>
    /// Increases the Exp of a character and returns useful text
    /// </summary>
    /// <returns></returns>
    public ExperienceGainResult IncreaseExp(long experienceToGain)
    {
        if (Level >= MaxLevel)
            return new ExperienceGainResult() { ExcessExperience = experienceToGain, Text = $"{this} has already reached their max level!" };
        var expGainText = "";
        
        var levelBefore = Level;
        Experience += experienceToGain;
        var nextLevelExp =GetRequiredExperienceToNextLevel(Level);
        while (Experience >= nextLevelExp && Level < MaxLevel)
        {
            Experience -= nextLevelExp;
            Level += 1;
            nextLevelExp = GetRequiredExperienceToNextLevel(Level);
        }

        string num = Number != 0 ? $" [{Number}]" : String.Empty;
        expGainText += $"{Name}{num} gained {experienceToGain} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }
        long excessExp = 0;
        if (Experience > nextLevelExp)
        {
            excessExp = Experience - nextLevelExp;
            Experience = 0;
        }

        if (Level >= MaxLevel)
        {
            excessExp += Experience;
            Experience = 0;
        }
   
        expGainText += "!";
        return new ExperienceGainResult(){ExcessExperience = excessExp, Text = expGainText};
    }

    public void SetExperience(long experience)
    {
        Experience = experience;
    }


}
public enum StatusEffectInflictResult : byte
{
    Failed, Resisted,Optimized,  Succeeded, 
}