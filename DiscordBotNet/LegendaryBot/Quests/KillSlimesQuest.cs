using DiscordBotNet.Database;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace DiscordBotNet.LegendaryBot.Quests;

public class KillSlimesQuest : Quest
{
    public override string Description => "Simple quest. Defeat the slimes that are attacking a village";

    public override async Task<bool> StartQuest(PostgreSqlContext databaseContext, InteractionContext context,
        DiscordMessage? message = null)
    {
        var slimeTeam = new CharacterTeam([new Slime(),new Slime()]);
        var userData = await databaseContext.UserData
            .IncludeTeamWithAllEquipments()
            .FindOrCreateUserDataAsync(UserDataId);
        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithColor(userData.Color)
            .WithTitle("Watch out!")
            .WithDescription("A bunch of slimes are attacking!");
        if (message is null)
        {
            message = await context.Channel.SendMessageAsync(embed);
        }
        else
        {
            message = await message.ModifyAsync(new DiscordMessageBuilder {Embed = embed});
        }
        await Task.Delay(2000);
        var playerTeam = userData.EquippedPlayerTeam.LoadTeamEquipment();

        var battleSimulator = new BattleSimulator(playerTeam,slimeTeam.LoadTeamEquipment());
        var result = await battleSimulator.StartAsync(message);




        if (result.Winners == playerTeam)
        {
            var str = playerTeam.IncreaseExp(Character.GetExpBasedOnDefeatedCharacters(slimeTeam));
            QuestRewards = [new TextReward(str)];
        }
        
        return result.Winners == playerTeam;
        
    }

    public override IEnumerable<Reward> QuestRewards { get; protected set; } = [];
}