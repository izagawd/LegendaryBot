using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.Commands;

namespace DiscordBotNet.LegendaryBot.Quests;

public abstract class Quest
{
    private static List<Type> _questTypes = [];
    public static IEnumerable<Type> QuestTypes => _questTypes;
    [NotMapped]
    public abstract string Description { get; }
    [NotMapped]
    public virtual string Title => BasicFunctionality.Englishify(GetType().Name);
    
    public bool Completed { get; set; } = false;
    public Guid Id { get; set; }


    /// <param name="databaseContext"></param>
    /// <param name="context"></param>
    /// <param name="messageToEdit"> if not null, should edit that message instead of adding a new message to the channel</param>
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<bool> StartQuest(PostgreSqlContext databaseContext, CommandContext context,
        DiscordMessage messageToEdit);

    [NotMapped]
    public abstract IEnumerable<Reward> QuestRewards { get; protected set; }
    
    public long UserDataId { get; set; }
 
}