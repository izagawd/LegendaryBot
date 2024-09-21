using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using BasicFunctionality;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Entities.Models;
using PublicInfo;

namespace Entities.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Gear : IInventoryEntity, IGuidPrimaryIdHaver
{

    private static readonly ConcurrentDictionary<int, Gear> _cachedDefaultGearsTypeIds = [];


    [Timestamp] public uint Version { get; set; }


    [NotMapped] public IEnumerable<GearStat> Substats => Stats.Except([MainStat]);


    [NotMapped] public virtual IEnumerable<Type> PossibleMainStats => [];
    public Character? Character { get; set; }


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

    public string DisplayString =>
        GetDisplayString(Name, Stats, Rarity,
            Number == 0 ? null : Number, Character?.Name,
            Character?.Number, MainStat);

    public long Id { get; set; }
    public abstract int TypeId { get; protected init; }

    public bool CanBeTraded => true;

    public Type TypeGroup => typeof(Gear);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;


    public string Description { get; }
    public Rarity Rarity { get; private set; }


    public abstract string Name { get; }
    public UserData? UserData { get; set; }
    public string ImageUrl => $"{Information.GearsImagesDirectory}/{TypeId}.png";
    public long UserDataId { get; set; }

    public static string GetDisplayString(string name, IEnumerable<GearStat> gearStats, Rarity rarity
        , int? number, string? characterName, int? characterNumber, GearStat? mainStat = null)
    {
        var enumerable = gearStats as GearStat[] ?? gearStats.ToArray();
        mainStat = mainStat ?? enumerable.First(i => i.IsMainStat is not null);

        var substats = enumerable.Where(i => i != mainStat).ToArray();
        string numberToUse = null!;
        if (number is null)
            numberToUse = "";
        else
            numberToUse = $"{number} • ";

        var shouldSpace = false;
        var stringToUse = new StringBuilder($"```{numberToUse}{name}".PadRight(12) +
                                            $" • {mainStat.AsNameAndValue()} • " +
                                            $"{string.Concat(Enumerable.Repeat("\u2b50", (int)rarity))}\nSubstats:");
        foreach (var j in substats)
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

        if (characterName is not null && characterNumber is not null)
            stringToUse.Append($"\nEquipped By: {characterName} [{characterNumber}]");
        stringToUse.Append("```");
        return stringToUse.ToString();
    }

    public static Gear GetDefaultFromTypeId(int typeId)
    {
        return _cachedDefaultGearsTypeIds.GetOrAdd(typeId, i =>
        {
            return TypesFunction.GetDefaultObjectsAndSubclasses<Gear>()
                .FirstOrDefault(j => j.TypeId == i) ?? throw new Exception($"Gear with type id {i} not found");
        });
        
    }


    private void AddSubstat()
    {
        if (Stats.Count < 6)
        {
            Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
        }
        else
        {
            var randomSubstat = BasicFunctions.RandomChoice(Substats);
            randomSubstat.Increase(Rarity);
        }
    }


    private GearStat GenerateArtifactPossibleSubStat(Rarity rarity)
    {
        var typesToConsider = GearStat.AllGearStatTypes.Except(
            [..Stats.Select(i => i.GetType()), GearStat.SpeedPercentageType]);
        var chosenType = BasicFunctions.RandomChoice(typesToConsider);
        var created = (GearStat)Activator.CreateInstance(chosenType)!;
        created.Increase(rarity);
        return created;
    }


    public void Initialize(Rarity rarity, Type? desiredMainStat = null)
    {
        if (Stats.Count != 0) return;
        Rarity = rarity;


        if (desiredMainStat is null)
            desiredMainStat = BasicFunctions.RandomChoice(PossibleMainStats);
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