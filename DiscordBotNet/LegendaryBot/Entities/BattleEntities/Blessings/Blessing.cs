using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class BlessingDatabaseConfiguration : IEntityTypeConfiguration<Blessing>
{
    public void Configure(EntityTypeBuilder<Blessing> builder)
    {
        builder.HasKey(i => i.Id);
    }
}
public abstract class Blessing : IInventoryEntity, IGuidPrimaryIdHaver
{



    public virtual  string Description => GetDescription(Level);
    public abstract Rarity Rarity { get; }
    public IInventoryEntity Clone()
    {
        var clone =(Blessing)  MemberwiseClone();
        clone.Id = Guid.Empty;
        clone.UserData = null;
        clone.UserDataId = 0;
        return clone;
    }

 
    public string Name { get; }
    public UserData? UserData { get; set; }

    public virtual string GetDescription(int level)
    {
        return "Idk man";
    }
    public  Type TypeGroup => typeof(Blessing);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public Guid? CharacterId { get; set; }


    
    

    public string ImageUrl { get; }
    public Guid Id { get; set; }
    public ulong UserDataId { get; set; }


    [NotMapped] public virtual int Attack => 20 + (Level * 4);
    [NotMapped] public virtual int Health => 70 + (Level * 8);


    public Blessing()
    {
        ImageUrl = $"{Website.DomainName}/battle_images/blessings/{GetType().Name}.png";
        Name = BasicFunctionality.Englishify(GetType().Name);
    }
    [NotMapped]
    protected int Level => (Character?.Level).GetValueOrDefault(1);
    [NotMapped]
    public bool IsInStandardBanner => true;
    public Character? Character { get; set; }

    public IEnumerable<string> ImageUrls { get; }
}