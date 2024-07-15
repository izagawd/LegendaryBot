using System.ComponentModel;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Commands;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.command;

[Command("display")]
public class Display : GeneralCommandClass
{
    protected static DiscordButtonComponent Next = new DiscordButtonComponent(DiscordButtonStyle.Success, "next", "NEXT");
    protected static DiscordButtonComponent Previous = new DiscordButtonComponent(DiscordButtonStyle.Primary, "previous", "PREVIOUS");

// Last Button
    protected static DiscordButtonComponent Last = new DiscordButtonComponent(DiscordButtonStyle.Primary, "last", "LAST");

    
    
    
    protected static DiscordButtonComponent First = new DiscordButtonComponent(DiscordButtonStyle.Primary, "first", "FIRST");
    

    [Command("characters"),Description("Displays all the characters you have")]
    public async ValueTask ExecuteDisplayCharacters(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Character))
            .ThenInclude((Entity i) => (i as Character).Blessing)
            .FindOrCreateUserDataAsync(context.User.Id);

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
                .AddEmbed(embed);
            
            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.RespondAsync(response);
                message = await context.GetResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message.WaitForButtonAsync(context.User);
            
            if(result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
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
    [Command("blessing"),Description("Displays a blessing you have by inputting their id")]
    public async ValueTask ExecuteDisplayABlessing(CommandContext ctx,
        [Parameter("blessing-id")] Guid blessingId)
    {
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithDescription("Invalid id");

        
     
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j.Id == blessingId && j is Blessing))
            .ThenInclude((Entity entity) => (entity as Blessing)!.Character)
            .FindOrCreateUserDataAsync(ctx.User.Id);
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
            
            await ctx.RespondAsync(embedBuilder);
            return;
        }

        

        
        await using var stream = new MemoryStream();
        await (await blessing.GetDetailsImageAsync()).SaveAsPngAsync(stream);
        stream.Position = 0;
        embedBuilder.WithImageUrl("attachment://description.png");

        embedBuilder
            .WithTitle("Here you go!")
            .WithDescription($"Name: {blessing}\nId: {blessing.Id}");
        var builder = new DiscordInteractionResponseBuilder()
            .AddFile("description.png", stream)
            .WithTitle("Detail")
            .AddEmbed(embedBuilder.Build());
        await ctx.RespondAsync(builder);
   
    }
    [Command("blessings"),Description("Displays all the blessings you have")]
    public async ValueTask ExecuteDisplayBlessings(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Inventory.Where(j => j is Blessing))
            .ThenInclude((Entity i) => (i as Blessing).Character)
            .FindOrCreateUserDataAsync(context.User.Id);

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
                .AddEmbed(embed);
            
            if (message is null)
            {
                var response = new DiscordInteractionResponseBuilder(messageBuilder);
                await context.RespondAsync(response);
                
                message = await context.GetResponseAsync();
            }
            else
            {
                message = await message.ModifyAsync(messageBuilder);
            }

            var result = await message.WaitForButtonAsync(context.User);
            
            if(result.TimedOut) break;
            await result.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
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
    [Command("teams"), Description("Displays all the teams you have and the characters in them")]
    public async ValueTask ExecuteDisplayTeams(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FindOrCreateUserDataAsync(context.User.Id);

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

        await context.RespondAsync(embed);
    }

}