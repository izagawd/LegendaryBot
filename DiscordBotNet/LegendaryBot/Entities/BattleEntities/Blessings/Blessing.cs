using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;

public class BlessingDatabaseConfiguration : IEntityTypeConfiguration<Blessing>
{
    public void Configure(EntityTypeBuilder<Blessing> builder)
    {
        builder.HasKey(i => i.Id);
        var starting = builder.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunctionality.GetDefaultObjectsThatIsInstanceOf<Blessing>())
        {
            starting = starting.HasValue(i.GetType(), i.TypeId);
        }

    }
}
public abstract class Blessing : IInventoryEntity, IGuidPrimaryIdHaver
{

    public static Blessing GetRandomBlessing(Dictionary<Rarity,double> rates)
    {
        var groups = TypesFunctionality
            .GetDefaultObjectsThatIsInstanceOf<Blessing>()
            .Where(i =>  i.SpawnsNormally)
            .GroupBy(i => i.Rarity)
            .ToImmutableArray();
         
        var rarityToUse = BasicFunctionality.GetRandom(rates);
        var randomBlessing
            = BasicFunctionality.RandomChoice(
                groups.First(i => i.Key == rarityToUse).Select(i => i)).GetType();
        return (Blessing)Activator.CreateInstance(randomBlessing)!;



    }
    public virtual bool SpawnsNormally => true;
    public virtual  string Description => GetDescription(Level);
    public abstract Rarity Rarity { get; }


 
    public string Name { get; }
    public UserData? UserData { get; set; }

    public virtual string GetDescription(int level)
    {
        return "Idk man";
    }

    public bool CanBeTraded => true;

 
    public  int TypeId { get; protected init; }
    public string DisplayString => $"`{Name}`";
    public  Type TypeGroup => typeof(Blessing);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public long? CharacterId { get; set; }


    
    

    public string ImageUrl { get; }
    public long Id { get; set; }
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


}