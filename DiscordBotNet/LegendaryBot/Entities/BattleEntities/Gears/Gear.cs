using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Gear : Entity
{
    public override Type TypeGroup => typeof(Gear);
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


    public Guid? ArtifactWielderId { get; set; }

    
    public GearStat MainStat { get; set; } 

    public List<GearStat> Stats { get; set; } = [];

    
}

public class GearDatabaseConfiguration : IEntityTypeConfiguration<Gear>
{
    public void Configure(EntityTypeBuilder<Gear> entity)
    {
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