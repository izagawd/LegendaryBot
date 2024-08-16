using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.GearSets;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Gear : IInventoryEntity, IGuidPrimaryIdHaver
{
    private static Dictionary<int, Type> _gearSetCache = [];

    private int _gearSetTypeId;

    static Gear()
    {
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<GearSet>())
            _gearSetCache[i.TypeId] = i.GetType();
    }

    [Timestamp] public uint Version { get; private set; }

    public int GearSetTypeId
    {
        get => _gearSetTypeId;
        set
        {
            if (!_gearSetCache.Keys.Contains(value))
                throw new Exception($"No gear set type with typeid {value}");
            _gearSetTypeId = value;
        }
    }

    [NotMapped]
    public Type GearSetType
    {
        get => _gearSetCache[_gearSetTypeId];
        set
        {
            if (!value.IsAssignableTo(typeof(GearSet)))
                throw new Exception("Inputted type must be of type gearstat");
            _gearSetTypeId = ((GearSet)TypesFunction.GetDefaultObject(value)).TypeId;
        }
    }

    [NotMapped] public IEnumerable<GearStat> Substats => Stats.Except([MainStat]);


    [NotMapped] public virtual IEnumerable<Type> PossibleMainStats => [];
    public Character Character { get; set; }


    public static IEnumerable<Type> AllGearTypes
    {
        get
        {
            yield return typeof(Armor);
            yield return typeof(Boots);
            yield return typeof(Weapon);
            yield return typeof(Ring);
            yield return typeof(Necklace);
            yield return typeof(Helmet);
        }
    }

    public GearStat MainStat { get; set; }

    public List<GearStat> Stats { get; set; } = [];


    public long? CharacterId { get; set; }
    public int Number { get; set; }
    public long Id { get; set; }
    public abstract int TypeId { get; protected init; }

    public bool CanBeTraded => true;

    public string DisplayString
    {
        get
        {
            MainStat.SetMainStatValue(Rarity);

            string numberToUse = null!;
            if (Number == 0)
                numberToUse = "";
            else
                numberToUse = $"{Number} • ";

            var setName = ((GearSet)TypesFunction.GetDefaultObject(GearSetType)).Name;
            var shouldSpace = false;
            var stringToUse = new StringBuilder($"```{numberToUse}{Name}".PadRight(12) +
                                                $" • {MainStat.AsNameAndValue()} • " +
                                                $"Rarity: {(int)Rarity}\u2b50\nSet: {setName}\nSubstats:");
            foreach (var j in Substats)
            {
                if (shouldSpace)
                {
                    shouldSpace = false;
                }
                else
                {
                    shouldSpace = true;
                    stringToUse.Append("\n");
                }

                var zaString = j.AsNameAndValue();

                if (shouldSpace)
                    zaString = zaString.PadRight(25);
                stringToUse.Append(zaString);
            }

            if (Character is not null)
                stringToUse.Append($"\nEquipped By: {Character.Name} [{Character.Number}]");
            stringToUse.Append("```");
            return stringToUse.ToString();
        }
    }

    public Type TypeGroup => typeof(Gear);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;


    public string Description { get; }
    public Rarity Rarity { get; private set; }


    public abstract string Name { get; }
    public UserData? UserData { get; set; }
    public string ImageUrl => $"{Website.DomainName}/battle_images/gears/{GetType().Name}.png";
    public long UserDataId { get; set; }


    private void AddSubstat()
    {
        if (Stats.Count < 6)
        {
            Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
        }
        else
        {
            var randomSubstat = BasicFunctionality.RandomChoice(Substats);
            randomSubstat.Increase(Rarity);
        }
    }


    private GearStat GenerateArtifactPossibleSubStat(Rarity rarity)
    {
        var typesToConsider = GearStat.AllGearStatTypes.Except(
            [..Stats.Select(i => i.GetType()), GearStat.SpeedPercentageType]);
        var chosenType = BasicFunctionality.RandomChoice(typesToConsider);
        var created = (GearStat)Activator.CreateInstance(chosenType)!;
        created.Increase(rarity);
        return created;
    }


    public void Initialize(Rarity rarity, Type? desiredGearSet = null, Type? desiredMainStat = null)
    {
        if (Stats.Count != 0) return;
        Rarity = rarity;


        if (desiredGearSet is null)
            GearSetType =
                BasicFunctionality.RandomChoice(
                        TypesFunction.GetDefaultObjectsAndSubclasses<GearSet>())
                    .GetType();
        else if (!desiredGearSet.IsAssignableTo(typeof(GearSet)))
            throw new ArgumentException(
                $"{nameof(desiredGearSet)} inputted is not subclass of type {typeof(GearSet).FullName}");
        else
            GearSetType = desiredGearSet;
        if (desiredMainStat is null)
            desiredMainStat = BasicFunctionality.RandomChoice(PossibleMainStats);
        else if (!PossibleMainStats.Contains(desiredMainStat))
            throw new Exception("Inputted desired main stat not possible for this artifact type");
        MainStat = (GearStat)Activator.CreateInstance(desiredMainStat)!;
        Stats.Add(MainStat);

        switch (Rarity)
        {
            case Rarity.FiveStar:
                AddSubstat();
                goto case Rarity.FourStar;
            case Rarity.FourStar:
                AddSubstat();
                goto case Rarity.ThreeStar;
            case Rarity.ThreeStar:
                AddSubstat();
                goto case Rarity.TwoStar;
            case Rarity.TwoStar:
                AddSubstat();
                goto case Rarity.OneStar;
            case Rarity.OneStar:
                AddSubstat();
                break;
        }

        foreach (var _ in Enumerable.Range(0, 6)) AddSubstat();
        MainStat.SetMainStatValue(Rarity);
    }
}

public class GearDatabaseConfiguration : IEntityTypeConfiguration<Gear>
{
    public void Configure(EntityTypeBuilder<Gear> entity)
    {
        entity.HasKey(i => i.Id);
        var starting = entity.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Gear>())
            starting = starting.HasValue(i.GetType(), i.TypeId);

        entity.HasIndex(i => new { i.TypeId, i.CharacterId })
            .IsUnique();
        entity.HasOne(i => i.MainStat)
            .WithOne()
            .HasForeignKey<GearStat>(i => i.IsMainStat);
        entity.HasMany(i => i.Stats)
            .WithOne(i => i.Gear)
            .HasForeignKey(i => i.GearId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.Property(i => i.Rarity)
            .HasColumnName(nameof(Gear.Rarity));
        entity.HasIndex(i => new { i.Number, i.UserDataId })
            .IsUnique();
        // generated on add even though a trigger handles it, just in case the trigger doesn't work
        entity.Property(i => i.Number)
            .ValueGeneratedOnAdd();
    }
}