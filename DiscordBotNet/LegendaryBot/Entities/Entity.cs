using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.LegendaryBot.Entities;

public abstract class Entity : ICloneable, IImageHaver
{

    public virtual Type TypeGroup => typeof(Entity);
    public DateTime DateAcquired { get; set; } = DateTime.UtcNow;
    public override string ToString()
    {
        return Name;
    }
    public virtual Task<Image<Rgba32>> GetDetailsImageAsync()
    {
        return Task.FromResult(new Image<Rgba32>(100,100));

    }


    object ICloneable.Clone()
    {
        return Clone();
    }

    [NotMapped] public virtual string Description => "";
    [NotMapped] public virtual Rarity Rarity { get; protected set; } = Rarity.OneStar;
    public Entity Clone()
    {
        var clone =(Entity) MemberwiseClone();
        clone.Id = Guid.Empty;
        return clone;
    }

    [NotMapped] public virtual string Name => BasicFunctionality.Englishify(GetType().Name);
 
    public static IEnumerable<Entity> operator *(Entity a, int number)
    {
        if (number <= 0)
        {
            throw new Exception("Entity times a negative number or 0 doesn't make sense");
            
        }
        foreach (var _ in Enumerable.Range(0, number))
        {
            yield return a.Clone();
        }
    }


    public UserData? UserData { get; set; }

    [NotMapped] public virtual string ImageUrl => null!;

    public Guid Id { get;  set; } 
    public ulong UserDataId { get; set; }
    public IEnumerable<string> ImageUrls
    {
        get { yield return ImageUrl; }
    }
}
public class  EntityDatabaseConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.HasKey(i => i.Id);
        
        builder
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();
    }
}