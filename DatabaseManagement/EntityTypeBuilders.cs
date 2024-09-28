using BasicFunctionality;
using DSharpPlus.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.BattleEntities.Gears.Stats;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DatabaseManagement;

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
            .WithOne()
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

public class PlayerDatabaseConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
    }
}

public class PlayerTeamDatabaseConfiguration : IEntityTypeConfiguration<PlayerTeam>
{
    public void Configure(EntityTypeBuilder<PlayerTeam> entity)
    {
        entity.Property(i => i.Id)
            .ValueGeneratedOnAdd();
        entity.HasIndex(i => new { i.UserDataId, i.TeamName })
            .IsUnique();

        entity.HasKey(i => i.Id);
        entity.HasMany<Character>()
            .WithMany()
            .UsingEntity<PlayerTeamMembership>(i
                    => i.HasOne<Character>(j => j.Character).WithMany().HasForeignKey(j => j.CharacterId), i =>
                    i.HasOne<PlayerTeam>(j => j.PlayerTeam).WithMany(j => j.TeamMemberships)
                        .HasForeignKey(j => j.PlayerTeamId),
                i =>
                {
                    i.HasKey(j => new { j.CharacterId, j.PlayerTeamId });
                    i.HasIndex(j => new { j.PlayerTeamId, Order = j.Slot }).IsUnique();
                }
            );
    }
}

public class CharacterDatabaseConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> entity)
    {
        entity.HasKey(i => i.Id);

        entity.HasIndex(i => new { i.TypeId, i.UserDataId })
            .IsUnique();
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
            starting = starting.HasValue(i.GetType(), i.TypeId);
    }
}

public class SummonsTrackerConfig : IEntityTypeConfiguration<SummonsTracker>
{
    public void Configure(EntityTypeBuilder<SummonsTracker> builder)
    {
        builder.HasIndex(i => new{i.TypeId, i.UserDataId}).IsUnique();
        builder.HasKey(i => i.Id);
        var toDo = builder.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<SummonsTracker>())
        {
            toDo = toDo.HasValue(i.GetType(), i.TypeId);
        }
    }
}
public class ItemDatabaseConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => new { i.TypeId, i.UserDataId });

        var starting = builder.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Item>())
            starting = starting.HasValue(i.GetType(), i.TypeId);
    }
}
public class UserDataDatabaseConfiguration : IEntityTypeConfiguration<UserData>
{
    public void Configure(EntityTypeBuilder<UserData> builder)
    {
        builder.Property(i => i.Color)
            .HasConversion(i => i.Value, j => new DiscordColor(j)
            );
        builder.HasMany(i => i.PlayerTeams)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.SummonsTrackers)
            .WithOne()
            .HasForeignKey(i => i.UserDataId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => i.DiscordId)
            .IsUnique();
        builder
            .HasMany(i => i.Items)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Gears)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Blessings)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Characters)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasOne(i => i.EquippedPlayerTeam)
            .WithOne()
            .HasForeignKey<PlayerTeam>(i => i.IsEquipped)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(i => i.Quests)
            .WithOne()
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Quotes)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder.HasMany(i => i.QuoteReactions)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);

        builder.HasKey(i => i.Id);
    }
}