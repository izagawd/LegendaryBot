using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.ManyToManyInstances;
using DiscordBotNet.Database.Models;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class PlayerTeamDatabaseConfiguration : IEntityTypeConfiguration<PlayerTeam>
{
    public void Configure(EntityTypeBuilder<PlayerTeam> entity)
    {
        entity.Property(i => i.Id)
            .ValueGeneratedOnAdd();
        entity.HasIndex(i => new { i.UserDataId, i.TeamName })
            .IsUnique();
        entity    .Navigation(i => i.Characters)
            .AutoInclude();
        entity.HasKey(i => i.Id);
        entity.HasMany<CharacterPartials.Character>(i => i.Characters)
            .WithMany()
            .UsingEntity<CharacterPlayerTeam>(i
                => i.HasOne<CharacterPartials.Character>().WithMany().HasForeignKey(j => j.CharacterId), i =>
                i.HasOne<PlayerTeam>().WithMany().HasForeignKey(j => j.PlayerTeamId), i =>
                i.HasKey(j => new { j.CharacterId, j.PlayerTeamId }));
    }
}
public class PlayerTeam : CharacterTeam
{
    
    [Timestamp]
    public uint Version { get; private set; }
    [NotMapped]
    public bool IsFull => Count >= 4;
    public string TeamName { get;  set; } = "Team1";

    public UserData UserData { get; protected set; }
    
    /// <summary>
    /// Will be set to userdata 
    /// </summary>
    public ulong? IsEquipped { get;  set; }
    public override bool Add(CharacterPartials.Character character)
    {
        if (Count >= 4) return false;
 
        if(UserDataId != 0)
            character.UserDataId = UserDataId;

        return base.Add(character);
    }


    public PlayerTeam(ulong userDataId, params CharacterPartials.Character[] characters) : base(characters)
    {
        UserDataId = userDataId;
        
    }
    public PlayerTeam(DiscordUser user,params CharacterPartials.Character[] characters) : this(user.Id,characters)
    {

    }
    public PlayerTeam(params CharacterPartials.Character[] characters) : base(characters)
    {

    }

    public PlayerTeam()
    {
        
    }
    public long Id { get; set; } 
    public ulong UserDataId { get; set; }
    
}