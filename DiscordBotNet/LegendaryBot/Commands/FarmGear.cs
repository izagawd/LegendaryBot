using System.Collections.Immutable;
using System.ComponentModel;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;


namespace DiscordBotNet.LegendaryBot.Commands;

public class FarmGear : GeneralCommandClass
{
    private static IEnumerable<Type>
        allGearsType = DefaultObjects.AllAssemblyTypes.Where(i => i.IsSubclassOf(typeof(Gear))).ToImmutableArray();
    [Command("farm-gear"), Description("Use this to farm gier"),
     AdditionalCommand("/farm-gear", BotCommandType.Battle)]
    public async ValueTask Execute(CommandContext ctx)
    {
        var userData = await DatabaseContext.UserData.IncludeTeamWithAllEquipments()
            .FirstOrDefaultAsync(i => i.Id == ctx.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(ctx);
            return;
        }
        var embed = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithColor(userData.Color)
            .WithTitle("Farming gear")
            .WithDescription("What tier of farming do you want to do?");

        if (userData.Tier == Tier.Unranked)
        {
            embed.WithDescription("Use the begin command to start farming gear");
            await ctx.RespondAsync(embed);
            return;
        }

        if (userData.IsOccupied)
        {
            embed.WithDescription("You are occupied");
            await ctx.RespondAsync(embed);
            return;
        }

        await MakeOccupiedAsync(userData);


        List<DiscordButtonComponent> buttonComponents = [];
        
        foreach (var i in Enum.GetValues<Tier>().Except([Tier.Unranked]))
        {
            var comp = new DiscordButtonComponent(DiscordButtonStyle.Success, i.ToString().ToLower(), i.ToString(),true);
            if (userData.Tier >= i)
            {
                comp.Enable();
            }
            
            buttonComponents.Add(comp);
            
        }

       
        var discordMessageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embed)
            .AddComponents(buttonComponents.Take(5));
        if (buttonComponents.Count == 6)
            discordMessageBuilder.AddComponents(buttonComponents[5]);
        discordMessageBuilder.AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Danger, "leave", "Leave"));
        await ctx.RespondAsync(discordMessageBuilder);

        var message = (await ctx.GetResponseAsync())!;
        var result = await message.WaitForButtonAsync(ctx.User, new TimeSpan(0, 1, 0));
        if (result.TimedOut)
        {
            discordMessageBuilder.ClearComponents();
            await message.ModifyAsync(discordMessageBuilder);
            return;
        }
        
        if (!Enum.TryParse(result.Result.Id, true, out Tier tierResult))
        {
            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.UpdateMessage,
                new DiscordInteractionResponseBuilder()
                    .AddEmbed(embed));
            return;
        }
        await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        
        var enemyTeam = new CharacterTeam( new Delinquent(), new Delinquent(), new Delinquent(), new Delinquent());
        var level = 5;
        var ascension = 1;
        var attack = 250;
        var defense = 90;
        var health = 1500;
        var speed = 80;
     
        
 
        switch (tierResult)
        {
            case Tier.Bronze:
                break;
            case Tier.Silver:
                level = 15;
                ascension = 2;
                attack = 300;
                defense = 250;
                health = 3000;
                speed = 90;
                break;
            case Tier.Gold:
                level = 25;
                ascension = 3;
                attack = 1000;
                defense = 350;
                health = 6000;
                speed = 100;
                break;
            case Tier.Platinum:
                level = 35;
                ascension = 4;
                attack = 2500;
                defense = 600;
                health = 10000;
                speed = 115;
                break;
            case Tier.Diamond:
                level = 45;
                ascension = 5;
                attack = 3750;
                defense = 800;
                health = 13000;
                speed = 130;
                break;
            case Tier.Divine:
                level = 55;
                ascension = 6;
                attack = 5000;
                defense = 1000;
                health = 20000;
                speed = 150;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var i in enemyTeam)
        {
            i.Ascension = ascension;
            i.Level = level;
            i.TotalAttack = attack;
            i.TotalDefense = defense;
            i.TotalSpeed = speed;
            i.TotalMaxHealth = health;
        }

        
        var battle = new BattleSimulator(userData.EquippedPlayerTeam!.LoadTeamStats(), enemyTeam);
        var battleResult = await battle.StartAsync(message);
        if (battleResult.Winners == userData.EquippedPlayerTeam)
        {
            List<Gear> gearsToGive = [];
            var rarity = tierResult.ToRarity();
            foreach (var i in Enumerable.Range(0,3))
            {
                var gearType = BasicFunctionality.RandomChoice(allGearsType);
                var newGear = (Gear) Activator.CreateInstance(gearType)!;
                newGear.Initialize(rarity);
                gearsToGive.Add(newGear);
            }

            
            var idk =userData.ReceiveRewards([new UserExperienceReward(400) ,new CoinsReward(5000),new EntityReward(gearsToGive)]);
            var description = "You won and gained: \n" + idk;
            embed.WithDescription(description);
            await message.ModifyAsync(embed.Build());
            await DatabaseContext.SaveChangesAsync();
        }
    }
}
