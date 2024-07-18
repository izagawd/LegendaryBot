using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Gear : IInventoryEntity
{
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
    public IInventoryEntity Clone()
    {
        var clone =(Gear)  MemberwiseClone();
        clone.Id = Guid.Empty;
        clone.UserData = null;
        clone.UserDataId = 0;
        return clone;
    }

    public string Name { get; }
    public UserData? UserData { get; set; }
    public string ImageUrl { get; }
    public Guid Id { get; set; }
    public ulong UserDataId { get; set; }
    public Character Character { get; set; }

    public Gear(){}
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


    

    
    public GearStat MainStat { get; set; } 

    public List<GearStat> Stats { get; set; } = [];


    public IEnumerable<string> ImageUrls { get; }
    public Guid? CharacterId { get; set; }
}

public class GearDatabaseConfiguration : IEntityTypeConfiguration<Gear>
{
    public void Configure(EntityTypeBuilder<Gear> entity)
    {
        entity.HasKey(i => i.Id);
        entity.HasOne(i => i.MainStat)
            .WithOne()
            .HasForeignKey<GearStat>(i => i.IsMainStat);
        entity.HasMany(i => i.Stats)
            .WithOne(i => i.Gear)
            .HasForeignKey(i => i.GearId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.Property(i => i.Rarity)
            .HasColumnName(nameof(Gear.Rarity));
    }


}