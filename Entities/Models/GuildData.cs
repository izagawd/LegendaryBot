using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models;

public class GuildDataDatabaseConfig : IEntityTypeConfiguration<GuildData>
{
    public void Configure(EntityTypeBuilder<GuildData> builder)
    {
        builder.HasKey(i => i.Id);
    }
}

/// <summary>
/// For now, this class has no use
/// </summary>
public class GuildData
{
    [Timestamp] public uint Version { get; private set; }

    public ulong Id { get; set; }
}