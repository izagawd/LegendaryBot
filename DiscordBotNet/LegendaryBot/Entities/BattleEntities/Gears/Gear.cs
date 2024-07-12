using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using DiscordBotNet.LegendaryBot.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;

public abstract class Gear : BattleEntity
{


    [NotMapped] public IEnumerable<GearStat> Substats => Stats.Except([MainStat]);
    public sealed override int MaxLevel => 15;
    public sealed override ExperienceGainResult IncreaseExp(long experienceToGain)
    {
        if (Level >= MaxLevel) return new ExperienceGainResult { ExcessExperience = experienceToGain };
        Experience += experienceToGain;
        var expToNextLevel = GetRequiredExperienceToNextLevel();
        while (Experience >= expToNextLevel && Level < MaxLevel)
        {
            
            Experience -= expToNextLevel;
            Level++;
            expToNextLevel = GetRequiredExperienceToNextLevel();
            if (Level == 3 || Level == 6 || Level == 9 || Level == 12 || Level == 15)
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
        }

        var expRes = new ExperienceGainResult();
        if (Level >= MaxLevel)
        {
            expRes.ExcessExperience = Experience;
            Experience = 0;
        }
        return expRes;
    }

    public override long GetRequiredExperienceToNextLevel(int level)
    {
        var requiredExp = base.GetRequiredExperienceToNextLevel(level);
        switch (Rarity)
        {
            case Rarity.OneStar:
                return requiredExp;
               
            case Rarity.TwoStar:
                return requiredExp * 2;
            case Rarity.ThreeStar:
                return requiredExp * 3;
            case Rarity.FourStar:
                return requiredExp * 4;
            case Rarity.FiveStar:
                return requiredExp * 5;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private GearStat GenerateArtifactPossibleSubStat(Rarity rarity)
    {
        var typesToConsider = GearStat.AllGearStatTypes.Except(
            [..Stats.Select(i => i.GetType()),GearStat.SpeedPercentageType]);
        Type chosenType = BasicFunctionality.RandomChoice(typesToConsider);
        var created = (GearStat) Activator.CreateInstance(chosenType)!;
        created.Increase(rarity,false);
        return created;
    }


    [NotMapped] public virtual IEnumerable<Type> PossibleMainStats => [];


    public Gear(Rarity rarity) : this()
    {
        Rarity = rarity;
    }
    public Gear(){}
    public void Initialize(Rarity rarity, Type? desiredMainStat = null) 
    {
        if(Stats.Count != 0) return;
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
                Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
                goto case Rarity.FourStar;
            case Rarity.FourStar:
                Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
                goto case Rarity.ThreeStar;
            case Rarity.ThreeStar:
                Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
                goto case Rarity.TwoStar;
            case Rarity.TwoStar:
                Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
                goto case Rarity.OneStar;
            case Rarity.OneStar:
                Stats.Add(GenerateArtifactPossibleSubStat(Rarity));
                break;
        }
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