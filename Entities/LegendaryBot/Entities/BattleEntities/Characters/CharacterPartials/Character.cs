using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BasicFunctionality;
using DSharpPlus.Entities;
using Entities.LegendaryBot.BattleEvents.EventArgs;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.DialogueNamespace;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.GearSets;
using Entities.LegendaryBot.Moves;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.Rewards;
using Entities.LegendaryBot.StatusEffects;
using Entities.Models;
using PublicInfo;
using StatusEffects_Barrier = Entities.LegendaryBot.StatusEffects.Barrier;

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

/// <summary>
///     Don't forget to load the character with LoadTeamGearWithPlayerDataAsync before using it in combat.
///     Characters can also be loaded at once if they are in a CharacterTeam and LoadTeamGearWithPlayerDataAsync is called
///     from the CharacterTeam
/// </summary>
public abstract partial class Character : IInventoryEntity, ICanBeLeveledUp, IGuidPrimaryIdHaver
{
    private static readonly ConcurrentDictionary<int, Character> CachedDefaultCharacterTypeIds = [];

    public static readonly Exception NoBattleExc = new("Character is not in battle");


    private readonly HashSet<StatusEffect> _statusEffects = [];


    [NotMapped] private float _combatReadiness;


    [NotMapped] private GearSet[]? _gearSets;


    [NotMapped] private float _health = 1;

    private bool _shouldTakeExtraTurn;

    /// <summary>
    ///     if not null, will assume this character uses super points.
    /// </summary>
    [NotMapped]
    public virtual bool UsesSuperPoints => false;

    [NotMapped] public int SuperPoints { get; set; }

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

    public int Number { get; set; }

    /// <summary>
    ///     Blessing currently equipped by character. Character of blessing must be set to this if useed in battle
    /// </summary>
    public Blessing? Blessing { get; set; }


    public StatusEffects_Barrier? Shield => _statusEffects.OfType<StatusEffects_Barrier>().FirstOrDefault();

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

    public bool IsDead => Health <= 0;

    [NotMapped] public Team? BattleTeam { get; set; }

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
    public virtual float Health
    {
        get => _health;
        set
        {
            if (CurrentBattle is null)
                throw NoBattleExc;
            if (_health <= 0) return;
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


    public List<Gear> Gears { get; } = new();

    public float ShieldPercentage
    {
        get
        {
            var shield = Shield;
            if (shield is null) return 0;

            return (float)(shield.GetShieldValue(this) * 1.0 / MaxHealth * 100.0);
        }
    }

    [NotMapped] public Alphabet AlphabetIdentifier => CurrentBattle?.GetAlphabetIdentifier(this) ?? throw NoBattleExc;

    public float HealthPercentage => (float)(Health * 1.0 / MaxHealth * 100.0);


    /// <summary>
    ///     Stats to increase when reaching level milestone. order matters in this case, and it should be exactly 5
    /// </summary>
    protected virtual IEnumerable<StatType> LevelMilestoneStatIncrease =>
    [
        StatType.MaxHealth, StatType.CriticalChance,
        StatType.Attack, StatType.Effectiveness, StatType.Resistance, StatType.Speed
    ];

    [NotMapped] public virtual Element Element { get; protected set; } = Element.Fire;

    /// <summary>
    ///     Derives dialogue profile from character properties
    /// </summary>
    public DialogueProfile DialogueProfile =>
        new()
        {
            CharacterColor = Color,
            CharacterName = Name,
            CharacterUrl = ImageUrl
        };


    [Timestamp] public uint Version { get; }

    public virtual int CoinsToGainWhenKilled => (Level + 50) * (int)Rarity;

    public virtual int ExpToGainWhenKilled
    {
        get
        {
            var averageFormulaTillNextLevel = GetRequiredExperienceToNextLevel(Level);

            var powCalculator = Math.Pow(1.05f, Level);
            var rarityMuliplier = 1 + (int)Rarity * 0.2;
            var computed = (averageFormulaTillNextLevel / powCalculator * rarityMuliplier * 0.75f).Round();
            return computed;
        }
    }

    [NotMapped] public virtual int ExpIncreaseScale => 1;

    /// <summary>
    ///     this will be used to get the items this character will drop if killed
    /// </summary>
    [NotMapped]
    public virtual IEnumerable<Reward> DroppedRewards
    {
        get { yield break; }
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

    [NotMapped] public IEnumerable<StatusEffect> StatusEffects => _statusEffects;

    [NotMapped] public virtual DiscordColor Color => DiscordColor.Green;

    [NotMapped] public IEnumerable<GearSet> GearSets => _gearSets ?? [];

    [NotMapped] public BattleSimulator? CurrentBattle => BattleTeam?.CurrentBattle;

    public string NameWithAlphabet => $"{Name} [{AlphabetIdentifier}]";


    [NotMapped] public BasicAttack BasicAttack { get; protected set; } = null!;


    [NotMapped] public Ultimate? Ultimate { get; protected set; }


    [NotMapped] public Skill? Skill { get; protected set; }

    /// <summary>
    ///     The position of the player based on combat readiness
    /// </summary>
    public int Position
    {
        get
        {
            if (CurrentBattle is null)
                throw NoBattleExc;
            return Array.IndexOf(CurrentBattle.Characters.OrderByDescending(i => i.CombatReadiness).ToArray(), this) +
                   1;
        }
    }


    /// <summary>
    ///     Checks if something overrides the player turn eg stun status effect preventing the player from doing anything
    /// </summary>
    public bool IsOverriden
    {
        get { return _statusEffects.Any(i => i.OverrideTurnType > 0); }
    }

    public int GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }

    public int Level { get; set; } = 1;


    public int GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(Level);
    }


    public int Experience { get; set; }

    /// <summary>
    ///     Increases the Exp of a character and returns useful text
    /// </summary>
    /// <returns></returns>
    public ExperienceGainResult IncreaseExp(int experienceToGain)
    {
        if (Level >= MaxLevel)
            return new ExperienceGainResult
                { ExcessExperience = experienceToGain, Text = $"{this} has already reached their max level!" };
        var expGainText = "";

        var levelBefore = Level;
        Experience += experienceToGain;
        var nextLevelExp = GetRequiredExperienceToNextLevel(Level);
        while (Experience >= nextLevelExp && Level < MaxLevel)
        {
            Experience -= nextLevelExp;
            Level += 1;
            nextLevelExp = GetRequiredExperienceToNextLevel(Level);
        }

        var num = Number != 0 ? $" [{Number}]" : string.Empty;
        expGainText += $"{Name}{num} gained {experienceToGain} exp";
        if (levelBefore != Level) expGainText += $", and moved from level {levelBefore} to level {Level}";
        var excessExp = 0;
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
        return new ExperienceGainResult { ExcessExperience = excessExp, Text = expGainText };
    }


    public long Id { get; set; }


    public abstract int TypeId { get; protected init; }


    public UserData? UserData { get; set; }
    public long UserDataId { get; set; }


    public abstract string Name { get; }

    public virtual string ImageUrl => $"{Information.ApiDomainName}/battle_images/characters/{GetType().Name}.png";


    public Type TypeGroup => typeof(Character);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public virtual string Description => "";
    public virtual Rarity Rarity => Rarity.ThreeStar;

    public virtual bool CanBeTraded => true;

    public static Character GetDefaultFromTypeId(int typeId)
    {
        if (!CachedDefaultCharacterTypeIds.TryGetValue(typeId, out var character))
        {
            character = TypesFunction.GetDefaultObjectsAndSubclasses<Character>()
                .FirstOrDefault(i => i.TypeId == typeId);
            if (character is null) throw new Exception($"Character with type id {typeId} not found");

            CachedDefaultCharacterTypeIds[typeId] = character;
        }

        return character;
    }


    /// <summary>
    ///     Increases combat readiness
    /// </summary>
    /// <param name="increaseAmount"> the amount to increase</param>
    /// <param name="announceIncrease">whetheer or not to announce the fact that combat readiness was increased</param>
    /// <returns>The amount of combat readiness increased</returns>
    public float IncreaseCombatReadiness(float increaseAmount, bool announceIncrease = true)
    {
        if (CurrentBattle is null)
            throw NoBattleExc;
        if (increaseAmount < 0) throw new ArgumentException("Increase amount should be at least 0");
        CombatReadiness += increaseAmount;
        if (announceIncrease && increaseAmount > 0)
            CurrentBattle.AddBattleText(new CombatReadinessChangeBattleText(this, increaseAmount));
        return increaseAmount;
    }


    /// <param name="decreaseAmount">amount to decrease</param>
    /// <param name="effectiveness">Use if it is resistable</param>
    /// <param name="announceDecrease">Whether or not to announce the fact that combat readiness was decreased</param>
    /// <returns>the amount decreased</returns>
    public int DecreaseCombatReadiness(int decreaseAmount, int? effectiveness = 0, bool announceDecrease = true)
    {
        if (CurrentBattle is null)
            throw NoBattleExc;
        if (decreaseAmount < 0) throw new ArgumentException("Decrease amount should be at least 0");
        if (effectiveness is not null)
        {
            var percentToResist = Resistance - effectiveness.Value;

            if (BasicFunctions.RandomChance(percentToResist))
            {
                if (announceDecrease)
                    CurrentBattle.AddBattleText($"{NameWithAlphabet} resisted combat readiness decrease!");
                return 0;
            }
        }

        CombatReadiness -= decreaseAmount;
        if (announceDecrease && decreaseAmount > 0)
            CurrentBattle.AddBattleText(new CombatReadinessChangeBattleText(this, -decreaseAmount));
        return decreaseAmount;
    }

    /// <summary>
    ///     Revives a character from the dead.
    /// </summary>
    public void Revive()
    {
        if (CurrentBattle is null)
            throw NoBattleExc;
        if (!IsDead) return;
        _health = 1;
        CurrentBattle.AddBattleText(new ReviveBattleText(this));
        CurrentBattle.InvokeBattleEvent(new CharacterReviveEventArgs(this));
    }


    /// <summary>
    ///     Grants a character an extra turn
    /// </summary>
    public void GrantExtraTurn()
    {
        if (CurrentBattle is null)
            throw NoBattleExc;
        if (IsDead) return;
        _shouldTakeExtraTurn = true;
        CurrentBattle.AddBattleText(new ExtraTurnBattleText(this));
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


    public IEnumerable<StatType> GetStatsToIncreaseBasedOnLevelMilestone(int levelMilestone)
    {
        if (levelMilestone > 6)
        {
            levelMilestone = 6;
            Console.WriteLine(
                "level milestone cannot be more than 6, cuz max level for a players character is 60, and milestones are reached every 10 levels");
        }

        foreach (var i in LevelMilestoneStatIncrease)
        {
            levelMilestone--;
            if (levelMilestone < 0) yield break;
            yield return i;
        }
    }


    public static int GetCoinsBasedOnCharacters(IEnumerable<Character> characters)
    {
        return characters.Select(i => i.CoinsToGainWhenKilled).Sum();
    }

    public static int GetExpBasedOnDefeatedCharacters(IEnumerable<Character> characters)
    {
        return characters.Select(i => i.ExpToGainWhenKilled).Sum();
    }


    public void SetBotStatsAndLevelBasedOnTier(Tier tier)
    {
        var level = 5;

        var attack = 66;
        var defense = 90;
        var health = 1000;
        var speed = 60;


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


    public override string ToString()
    {
        return CurrentBattle is not null ? NameWithAlphabet : Name;
    }


    /// <summary>
    ///     if this character is not being controlled by the player, it will use custom AI
    /// </summary>
    /// <param name="target"></param>
    /// <param name="decision"></param>
    public virtual void NonPlayerCharacterAi(ref Character target, ref BattleDecision decision)
    {
        if ((Ultimate?.CanBeUsed()).GetValueOrDefault(false))
        {
            decision = BattleDecision.Ultimate;
            target = BasicFunctions.RandomChoice(Ultimate!.GetPossibleTargets());
            return;
        }

        if ((Skill?.CanBeUsed()).GetValueOrDefault(false))
        {
            decision = BattleDecision.Skill;
            target = BasicFunctions.RandomChoice(Skill!.GetPossibleTargets());
            return;
        }

        target = BasicFunctions.RandomChoice(BasicAttack.GetPossibleTargets());
        decision = BattleDecision.BasicAttack;
    }


    public IEnumerable<GearSet> GenerateGearSets()
    {
        foreach (var i in Gears.GroupBy(i => i.GearSetType))
        {
            var count = i.Count();
            if (count < 2) continue;
            var created = (GearSet)Activator.CreateInstance(i.Key)!;
            created.Owner = this;
            if (count >= 4)
                created.CanUseFourPiece = true;
            yield return created;
        }
    }

    public bool RemoveStatusEffect(StatusEffect statusEffect)
    {
        return _statusEffects.Remove(statusEffect);
    }


    public string GetNameWithAlphabetIdentifier(bool isEnemy)
    {
        var side = "enemy";
        if (!isEnemy) side = "team mate";
        return $"{Name} [{side}] [{AlphabetIdentifier}]";
    }

    public void SetExperience(int experience)
    {
        Experience = experience;
    }


    #region Stats

    public int MaxLevel => 60;
    [NotMapped] public float TotalAttack { get; set; }
    [NotMapped] public float TotalDefense { get; set; }
    [NotMapped] public float TotalSpeed { get; set; }
    [NotMapped] public float TotalCriticalChance { get; set; }
    [NotMapped] public float TotalCriticalDamage { get; set; }
    [NotMapped] public float TotalEffectiveness { get; set; }
    [NotMapped] public float TotalResistance { get; set; }
    [NotMapped] public float TotalMaxHealth { get; set; }


    /// <summary>
    ///     Use this to load the build (stats) of the character. if u want to manually set the stats of this character, just
    ///     change the Total properties, and avoid calling this method. its called in load async unless u set thee
    ///     bool param to false
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
            foreach (var j in i.Substats) j.AddStats(this);
            i.MainStat.SetMainStatValue(i.Rarity);
            i.MainStat.AddStats(this);
        }

        _gearSets = GenerateGearSets().ToArray();
    }

    #endregion
}

public enum StatusEffectInflictResult : byte
{
    Failed,
    Resisted,
    Optimized,
    Succeeded
}