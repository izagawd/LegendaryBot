using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Artifacts;

public class Artifact : BattleEntity
{
    
    public Guid? CharacterGearEquipperId { get; set; }


    public ArtifactStat MainStat { get; set; } 

    public List<ArtifactStat> Stats { get; set; } = [];

    
}

public class ArtifactDatabaseConfiguration : IEntityTypeConfiguration<Artifact>
{
    public void Configure(EntityTypeBuilder<Artifact> entity)
    {
        entity.HasOne(i => i.MainStat)
            .WithOne()
            .HasForeignKey<ArtifactStat>(i => i.IsMainStat);
        entity.HasMany(i => i.Stats)
            .WithOne(i => i.Artifact)
            .HasForeignKey(i => i.GearId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}