
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.Database.Models;


public class GuildDataDatabaseConfig : IEntityTypeConfiguration<GuildData>
{
    public void Configure(EntityTypeBuilder<GuildData> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.CharacterSpawnCooldown)
            .HasConversion<long>();
    }
}
public class GuildData
{
    public ulong Id { get; set; }

    public TimeSpan CharacterSpawnCooldown { get; set; } = TimeSpan.FromMinutes(2);

}
