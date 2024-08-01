using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.Database;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordBotNet.LegendaryBot.Quests;

public class QuestDatabaseSetup : IEntityTypeConfiguration<Quest>
{

    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        builder.HasKey(i => i.Id);
        var discrimStart =builder.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunctionality.GetDefaultObjectsAndSubclasses<Quest>())
        {
            discrimStart = discrimStart.HasValue(i.GetType(), i.TypeId);
        }
    }
}
public abstract class Quest
{
    public int TypeId { get; protected set; }
    private static List<Type> _questTypes = [];
    public static IEnumerable<Type> QuestTypes => _questTypes;
    [NotMapped]
    public abstract string Description { get; }

    [NotMapped] public abstract string Title { get; }


    public bool Completed { get; set; } = false;
    public long Id { get; set; }


    /// <param name="databaseContext"></param>
    /// <param name="context"></param>
    /// <param name="messageToEdit"> if not null, should edit that message instead of adding a new message to the channel</param>
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<bool> StartQuest(PostgreSqlContext databaseContext, CommandContext context,
        DiscordMessage messageToEdit);

    [NotMapped]
    public abstract IEnumerable<Reward> QuestRewards { get; protected set; }
    
    public ulong UserDataId { get; set; }
 
}