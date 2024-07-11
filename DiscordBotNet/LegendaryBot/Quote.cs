using DiscordBotNet.Database.Models;

namespace DiscordBotNet.LegendaryBot;

public class QuoteReaction 
{
    public Guid Id { get; set; } 
    /// <summary>
    /// The ID of the user who reacted to the quote
    /// </summary>
    public long UserDataId { get; set; } 
    
    /// <summary>
    /// the quote id that was reacted to
    /// </summary>
    public Guid QuoteId { get; set; }
    /// <summary>
    /// The quote that was reacted to
    /// </summary>
    public Quote Quote { get; set; }

    public bool IsLike { get; set; } = true;

    public UserData UserData { get; set; }
}
public class Quote 
{
    public Guid Id { get; set; } 
    public bool IsApproved { get; set; } 
    public long UserDataId { get; set; }
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