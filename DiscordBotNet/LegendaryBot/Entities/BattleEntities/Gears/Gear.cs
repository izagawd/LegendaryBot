using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Primitives;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;


public abstract class Gear : IInventoryEntity, IGuidPrimaryIdHaver
{

    public abstract int TypeId { get; }

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
            {
                numberToUse = $"{Number} • ";
            }

            bool shouldSpace = false;
            var stringToUse =new StringBuilder($"```{numberToUse}{Name}".PadRight(12) +
                                               $" • {MainStat.AsNameAndValue()} • " +
                                               $"Rarity: {(int) Rarity}\u2b50\nSubstats:");
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
            stringToUse.Append($"```");
            return stringToUse.ToString();
        }
    }

    public  Type TypeGroup => typeof(Gear);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    [NotMapped] public IEnumerable<GearStat> Substats => Stats.Except([MainStat]);

    
    
    

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
            [..Stats.Select(i => i.GetType()),GearStat.SpeedPercentageType]);
        var chosenType = BasicFunctionality.RandomChoice(typesToConsider);
        var created = (GearStat) Activator.CreateInstance(chosenType)!;
        created.Increase(rarity);
        return created;
    }


    [NotMapped] public virtual IEnumerable<Type> PossibleMainStats => [];


    public string Description { get; }
    public Rarity Rarity { get; private set; }


    public string Name { get; }
    public UserData? UserData { get; set; }
    public string ImageUrl { get; }
    public long Id { get; set; }
    public ulong UserDataId { get; set; }
    public Character Character { get; set; }

    public Gear()
    {
        Name = BasicFunctionality.Englishify(GetType().Name);
    }
    public void Initialize(Rarity rarity, Type? desiredMainStat = null)
    {
        if (Stats.Count != 0) return;
        Rarity = rarity;
        if (desiredMainStat is null)
        {
            desiredMainStat = BasicFunctionality.RandomChoice(PossibleMainStats);
        } else if (!PossibleMainStats.Contains(desiredMainStat))
        {
            throw new Exception("Inputted desired main stat not possible for this artifact type");
        }
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

        foreach (var _ in Enumerable.Range(0,6))
        {
            AddSubstat();
        }
        MainStat.SetMainStatValue(Rarity);
       
    }


    
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
}

public class GearDatabaseConfiguration : IEntityTypeConfiguration<Gear>
{


    public void Configure(EntityTypeBuilder<Gear> entity)
    {
        entity.HasKey(i => i.Id);
        var starting = entity.HasDiscriminator<int>("Discriminator");
        foreach (var i in TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Gear>())
        {
            starting = starting.HasValue(i.GetType(), i.TypeId);
        }

        entity.HasIndex(nameof(Gear.CharacterId), "Discriminator")
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
        entity.Property(i => i.Number)
            .ValueGeneratedOnAdd();

    }


}