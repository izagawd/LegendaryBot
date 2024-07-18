using System.ComponentModel;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Character = DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials.Character;

namespace DiscordBotNet.LegendaryBot.Commands;

[Command("display")]
public class Display : GeneralCommandClass
{
    protected static DiscordButtonComponent Next = new DiscordButtonComponent(DiscordButtonStyle.Success, "next", "NEXT");
    protected static DiscordButtonComponent Previous = new DiscordButtonComponent(DiscordButtonStyle.Primary, "previous", "PREVIOUS");

// Last Button
    protected static DiscordButtonComponent Last = new DiscordButtonComponent(DiscordButtonStyle.Primary, "last", "LAST");

    private  static async ValueTask ExecuteDisplayAsync<TObject>(CommandContext context, IEnumerable<TObject> objects, int displaySectionLimit,
        Func<TObject, string> textToDisplayPerItem, string joiner, string objectTypeName,
        DiscordColor discordColor)
    {
        List<List<string>> displayList = [];
        var count = 0;
        List<string> currentList = [];
        
        displayList.Add(currentList);
    
        foreach (var i in objects)
        {
     
            if (count >= displaySectionLimit)
            {
                currentList = new List<string>();
                displayList.Add(currentList);
                count = 0;
            }

            var stringToUse = textToDisplayPerItem(i);
            currentList.Add(stringToUse);
            count++;
        }

        var index = 0;

        DiscordMessage? message = null;

    
        while (true)
        {
        
            var embed = new DiscordEmbedBuilder()
                .WithUser(context.User)
                .WithColor(discordColor)
                .WithTitle($"Page {index + 1}/{displayList.Count} ({objectTypeName})")
                .WithDescription(displayList[index].Join(joiner));
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
        [Command("items"),Description("Displays all the stackable items you have")]
    public async ValueTask ExecuteDisplayItems(CommandContext context,[Description("pretty self explanatory")]  string nameFilter = "")
    {
        var simplified = nameFilter.Replace(" ", "").ToLower();
        var userData = await DatabaseContext.UserData
            .Include(i => i.Items.Where(j =>
                    EF.Property<string>(j, "Discriminator").ToLower().Contains(simplified)))
            .FirstOrDefaultAsync(i => i.Id == context.User.Id); 
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }

        await ExecuteDisplayAsync(context, userData.Items, 20,
            i => $"Name: {i.Name} | Stacks: {i.Stacks} | Rarity: {i.Rarity}",
            "\n","Items",userData.Color);
    }
    
    protected static DiscordButtonComponent First = new DiscordButtonComponent(DiscordButtonStyle.Primary, "first", "FIRST");
        [Command("gears"),Description("Displays all the egars you have")]
    public async ValueTask ExecuteDisplayGear(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Gears)
            .ThenInclude(i => i.Character)
            .Include(i => i.Gears)
            .ThenInclude(i =>  i.Stats)
            .FirstOrDefaultAsync(i => i.Id == context.User.Id);
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
        await ExecuteDisplayAsync(context, userData.Gears, 3,
            i =>
            {
                i.MainStat.SetMainStatValue(i.Rarity);
            
                var stringToUse =new StringBuilder($"{i.Name} | Id: {i.Id} \nMain Stat = {i.MainStat.AsNameAndValue()}\nSubstats:");
                foreach (var j in i.Substats)
                {
                    stringToUse.Append($"\n{j.AsNameAndValue()}");
                }

                if (i.Character is not null)
                    stringToUse.Append($"Equipped By: {i.Character.Name}");
                return stringToUse.ToString();
            },
            "\n\n","Gears",userData.Color);
        
        

    }

    [Command("characters"),Description("Displays all the characters you have")]
    public async ValueTask ExecuteDisplayCharacters(CommandContext context,[Description("pretty self explanatory")]  string nameFilter = "")
    {
        var simplified = nameFilter.Replace(" ", "").ToLower();
        var userData = await DatabaseContext.UserData
            .Include(i => i.Characters
                .Where(j => EF.Property<string>(j,"Discriminator").ToLower().Contains(simplified)))
            .ThenInclude(i => i.Blessing)
            .FirstOrDefaultAsync(i => i.Id == context.User.Id); 
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
        await ExecuteDisplayAsync(context, userData.Characters, 10,
            i =>
            {
                var stringToUse = $"Name: {i.Name} |  Level: {i.Level} | Element: \n{i.Element} | Rarity: {i.Rarity} | " +
                                  $"{nameof(Character.DupeCount)}: | {i.DupeCount}";
                if (i.Blessing is not null)
                    stringToUse += $"\nBlessing Name: {i.Blessing.Name}\n" +
                                   $"Blessing Id: {i.Blessing.Id}";
                return stringToUse;
            },
            "\n\n","Characters",userData.Color);
  

    }

    [Command("blessings"),Description("Displays all the blessings you have")]
    public async ValueTask ExecuteDisplayBlessings(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.Blessings)
            .ThenInclude(i => i.Character)
            .FirstOrDefaultAsync(i => i.Id == context.User.Id); 
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
        await ExecuteDisplayAsync(context, userData.Blessings, 10,
            i =>
            {
                var stringToUse = $"Name: {i.Name}  | Rarity: {i.Rarity} |\n Id: {i.Id}";
                if (i.Character is not null)
                {

                    stringToUse += $"\n     Character Name: {i.Character.Name} | Character Level: {i.Character.Level}";
                }
                return stringToUse;
            },
            "\n\n","Blessings",userData.Color);


    }
    [Command("teams"), Description("Displays all the teams you have and the characters in them")]
    public async ValueTask ExecuteDisplayTeams(CommandContext context)
    {
        var userData = await DatabaseContext.UserData
            .Include(i => i.PlayerTeams)
            .FirstOrDefaultAsync(i => i.Id == context.User.Id); 
        if (userData is null || userData.Tier == Tier.Unranked)
        {
            await AskToDoBeginAsync(context);
            return;
        }
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