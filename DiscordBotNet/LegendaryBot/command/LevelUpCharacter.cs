using System.Linq.Expressions;
using DiscordBotNet.Database.Models;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotNet.LegendaryBot.command;

public class LevelUpCharacter : GeneralCommandClass
{
    [SlashCommand("level_up", "leveling up character"),
     AdditionalSlashCommand("/level_up player",BotCommandType.Battle)]
    public async Task LevelUp(InteractionContext ctx,
        [Option("character_name", "the name of the character")] string characterName)
    {
        var simplifiedCharacterName = characterName.ToLower().Replace(" ", "");

        Expression<Func<UserData,IEnumerable<Entity>>> includeLambda = (UserData i) => i.Inventory.Where( j => j is Character
                                 && EF.Property<string>(j, "Discriminator").ToLower() == simplifiedCharacterName);



        var gottenUserData =await  DatabaseContext.UserData

            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character).Gears)
            .ThenInclude(i => i.Stats)
            .Include(includeLambda)
            .ThenInclude((Entity i) => (i as Character).Blessing)
            .FindOrCreateUserDataAsync((long)ctx.User.Id);
        Character character = gottenUserData.Inventory.OfType<Character>().FirstOrDefault();
        var embedBuilder = new DiscordEmbedBuilder()
            .WithUser(ctx.User)
            .WithTitle("Hmm")
            .WithImageUrl("attachment://levelupimage.png")
            .WithColor(gottenUserData.Color)
            .WithDescription($"Unable to find character with the name \"{simplifiedCharacterName}\"");
        
        if (character is null)
        {
            await ctx.CreateResponseAsync(embedBuilder.Build());
            
        }
        else
        {

            character.LoadGear();
            embedBuilder.WithTitle($"{character.Name}'s level up process");
            using var levelUpImage = await character.GetImageForLevelUpAndAscensionAsync();
            await using var stream = new MemoryStream();
            await levelUpImage.SaveAsPngAsync(stream);
            stream.Position = 0;
            var messageBuilder =new DiscordInteractionResponseBuilder()
                .AddEmbed(embedBuilder.Build())
                .AddFile("levelupimage.png", stream);
            await ctx.CreateResponseAsync(messageBuilder);
            
        }

    }
}