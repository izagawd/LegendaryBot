using DiscordBotNet.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot;

public class QuoteReaction 
{
    public long Id { get; set; } 
    /// <summary>
    /// The ID of the user who reacted to the quote
    /// </summary>
    public ulong UserDataId { get; set; } 
    
    /// <summary>
    /// the quote id that was reacted to
    /// </summary>
    public long QuoteId { get; set; }
    /// <summary>
    /// The quote that was reacted to
    /// </summary>
    public Quote Quote { get; set; }

    public bool IsLike { get; set; } = true;

    public UserData UserData { get; set; }
}

public class QuoteDatabaseConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> entity)
    {
        entity.HasKey(i => i.Id);
        entity
            .Property(i => i.Id)
            .ValueGeneratedOnAdd();
        entity.HasMany(i => i.QuoteReactions)
            .WithOne(i => i.Quote)
            .HasForeignKey(i => i.QuoteId);
    }
}
public class Quote 
{
    
    public long Id { get; set; } 
    public bool IsApproved { get; set; } 
    public ulong UserDataId { get; set; }
    public string QuoteValue { get; set; } = "Nothing";
    public UserData UserData { get; set; }
    public List<QuoteReaction> QuoteReactions { get; set; } = []; 

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public override string ToString() => QuoteValue;


    public Quote(string quote) : this()
    {
        QuoteValue = quote;
    }
    public Quote(){}
}