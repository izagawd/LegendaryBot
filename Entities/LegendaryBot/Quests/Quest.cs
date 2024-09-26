using System.ComponentModel.DataAnnotations.Schema;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot.Rewards;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.LegendaryBot.Quests;

public class QuestDatabaseSetup : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        builder.HasKey(i => i.Id);
        var discrimStart = builder.HasDiscriminator(i => i.TypeId);
        foreach (var i in TypesFunction.GetDefaultObjectsAndSubclasses<Quest>())
            discrimStart = discrimStart.HasValue(i.GetType(), i.TypeId);
    }
}

public  class QuestResult
{
    public Reward[]? Rewards { get; private set; }

    public bool IsSuccess => Rewards is not null;

    public static QuestResult Success()
    {
        return Success([]);
    }
    public static QuestResult Success(IEnumerable<Reward> rewards)
    {
        var res = new QuestResult();
        res.Rewards = rewards?.ToArray() ?? [];
        return res;
    }

    public static QuestResult Fail()
    {
        return new QuestResult();
    }
    protected QuestResult()
    {
        
    }
}
public abstract class Quest
{
    private static List<Type> _questTypes = [];
    public abstract int TypeId { get; protected init; }
    public static IEnumerable<Type> QuestTypes => _questTypes;

    [NotMapped] public abstract string Description { get; }

    [NotMapped] public abstract string Title { get; }


    public bool Completed { get; set; }
    public long Id { get; set; }



    public long UserDataId { get; set; }


    /// <param name="userDataQueryable"></param>
    /// <param name="context"></param>
    /// <param name="messageToEdit"> if not null, should edit that message instead of adding a new message to the channel</param>
    /// <returns>True if quest was successfully completed</returns>
    public abstract Task<QuestResult> StartQuest(IQueryable<UserData> userDataQueryable, CommandContext context,
        DiscordMessage messageToEdit);
}