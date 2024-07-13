using System.Linq.Expressions;
using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.command;

[SlashCommandGroup("display", "Display stuff from your inventory")]
public class Display : GeneralCommandClass
{
    protected static DiscordButtonComponent Next = new DiscordButtonComponent(ButtonStyle.Success, "next", "NEXT");
    protected static DiscordButtonComponent Previous = new DiscordButtonComponent(ButtonStyle.Primary, "previous", "PREVIOUS");

// Last Button
    protected static DiscordButtonComponent Last = new DiscordButtonComponent(ButtonStyle.Primary, "last", "LAST");

    
    
    
    protected static DiscordButtonComponent First = new DiscordButtonComponent(ButtonStyle.Primary, "first", "FIRST");
    

    [SlashCommand("characters", "display all your owned characters")]
    public async Task ExecuteDisplayCharacters(InteractionContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Character))
            .ThenInclude((Entity i) => (i as Character).Blessing)
            .FindOrCreateUserDataAsync((long)context.User.Id);

        List<List<string>> displayList = [];
        var count = 0;
        List<string> currentList = [];
        
        displayList.Add(currentList);
        var displaySectionLimit = 10;
        foreach (var i in userData.Inventory.OfType<Character>())
        {
     
            if (count >= displaySectionLimit)
            {
                currentList = new List<string>();
                displayList.Add(currentList);
                count = 0;
            }

            var stringToUse = $"Name: {i.Name} |     Level: {i.Level}";
            if (i.Blessing is not null)
                stringToUse += $"\n     Blessing Name: {i.Blessing.Name}               Blessing Level: {i.Blessing.Level}\n" +
                               $"Blessing Id: {i.Blessing.Id}";
            currentList.Add(stringToUse);
            count++;
        }

        var index = 0;

        DiscordMessage? message = null;

    
        while (true)
        {
        
            var embed = new DiscordEmbedBuilder()
                .WithUser(context.User)
                .WithColor(userData.Color)
                .WithTitle($"Page {index + 1}")
                .WithDescription(displayList[index].Join("\n\n"));
            var messageBuilder = new DiscordMessageBuilder()
                .AddComponents(First,Previous,Next,Last)
                .WithEmbed(embed);
            
            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.CreateResponseAsync(response);
                message = await context.GetOriginalResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message.WaitForButtonAsync(context.User);
            
            if(result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            switch (result.Result.Id.ToLower())
            {
                case "next":
                    index++;
                    break;
                case "previous":
                    index--;
                    break;
                case "last":
                    index = displayList.Count - 1;
                    break;
                case "first":
                    index = 0;
                    break;
            }

            if (index < 0) index = 0;
            if (index > displayList.Count - 1) index = displayList.Count - 1;
        }
        

    }
    [SlashCommand("blessing", "shows the details of a single blessing you own by their id")]
    public async Task ExecuteDisplayABlessing(InteractionContext ctx,
        [Option("blessing_id","The id of the blessing you want to get details about")] string blessingIdString)
    {
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithDescription("Invalid id");

        
        if(!Guid.TryParse(blessingIdString, out Guid blessingId)) 
            blessingId = Guid.Empty;
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j.Id == blessingId && j is Blessing))
            .ThenInclude((Entity entity) => (entity as Blessing).Character)
            .FindOrCreateUserDataAsync((long)ctx.User.Id);
        embedBuilder.WithColor(userData.Color);
        var blessing = userData.Inventory.OfType<Blessing>().FirstOrDefault(i => i.Id == blessingId);
        if (blessing is null)
        {
            if (blessingId == Guid.Empty)
            {
                embedBuilder.WithDescription("Invalid Id");
            }
            else
            {
                embedBuilder.WithDescription($"You do not have any blessing with the id {blessingId}");
            }
            
            await ctx.CreateResponseAsync(embedBuilder);
            return;
        }

        

        
        await using var stream = new MemoryStream();
        await (await blessing.GetDetailsImageAsync()).SaveAsPngAsync(stream);
        stream.Position = 0;
        embedBuilder.WithImageUrl("attachment://description.png");

        embedBuilder
            .WithTitle("Here you go!")
            .WithDescription($"Name: {blessing}\nId: {blessing.Id}");
        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
            .AddFile("description.png", stream)
            .WithTitle("Detail")
            .AddEmbed(embedBuilder.Build());
        await ctx.CreateResponseAsync(builder);
   
    }
    [SlashCommand("blessings", "display all your owned blessings")]
    public async Task ExecuteDisplayBlessings(InteractionContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Blessing))
            .ThenInclude((Entity i) => (i as Blessing).Character)
            .FindOrCreateUserDataAsync((long)context.User.Id);

        List<List<string>> displayList = [];
        var count = 0;
        List<string> currentList = [];
        
        displayList.Add(currentList);
        var displaySectionLimit = 10;
        foreach (var i in userData.Inventory.OfType<Blessing>())
        {
            if (count >= displaySectionLimit)
            {
                currentList = new List<string>();
                displayList.Add(currentList);
                count = 0;
            }
            var stringToUse = $"Name: {i.Name} |     Level: {i.Level} |     Id: {i.Id}";
            if (i.Character is not null)
            {

                stringToUse += $"\n     Character Name: {i.Character.Name} | Character Level: {i.Character.Level}";
            }

  
            currentList.Add(stringToUse);
            count++;
        }

        var index = 0;

        DiscordMessage? message = null;

    
        while (true)
        {
        
            var embed = new DiscordEmbedBuilder()
                .WithUser(context.User)
                .WithColor(userData.Color)
                .WithTitle($"Page {index + 1}")
                .WithDescription(displayList[index].Join("\n\n"));
            var messageBuilder = new DiscordMessageBuilder()
                .AddComponents(First,Previous,Next,Last)
                .WithEmbed(embed);
            
            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.CreateResponseAsync(response);
                message = await context.GetOriginalResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message.WaitForButtonAsync(context.User);
            
            if(result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            switch (result.Result.Id.ToLower())
            {
                case "next":
                    index++;
                    break;
                case "previous":
                    index--;
                    break;
                case "last":
                    index = displayList.Count - 1;
                    break;
                case "first":
                    index = 0;
                    break;
            }

            if (index < 0) index = 0;
            if (index > displayList.Count - 1) index = displayList.Count - 1;
        }
        

    }
    [SlashCommand("teams","displays all your teams")]
    public async Task ExecuteDisplayTeams(InteractionContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FindOrCreateUserDataAsync((long)context.User.Id);

        var teamStringBuilder = new StringBuilder();
        var count = 0;


  
        foreach (var i in userData.PlayerTeams)
        {
            count++;
            var equipped = "";
            if (userData.EquippedPlayerTeam == i)
                equipped = " (equipped)";
            teamStringBuilder.Append($"1.{equipped} {i.TeamName}. Members: ");
            foreach (var j in i)
            {
                teamStringBuilder.Append($"{j}, ");
            }

            teamStringBuilder.Append("\n");
        }

        var embed = new DiscordEmbedBuilder()
            .WithUser(context.User)
            .WithTitle("Here's a list of all your teams")
            .WithColor(userData.Color)
            .WithDescription(teamStringBuilder.ToString());

        await context.CreateResponseAsync(embed);
    }

}