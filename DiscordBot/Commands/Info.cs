using System.ComponentModel;
using BasicFunctionality;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBot.Commands;

public class Info : GeneralCommandClass
{
    public static async Task<Image<Rgba32>> GetInfoAsync(UserData userData, DiscordUser user)
    {
        var adventurerLevel = userData.AdventurerLevel;

        var requiredExp = userData.GetRequiredExperienceToNextLevel();
        using var userImage = await ImageFunctions.GetImageFromUrlAsync(user.AvatarUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100, 100)));
        image.Mutate(ctx =>
        {
            ctx.BackgroundColor(SixLabors.ImageSharp.Color.ParseHex(userData.Color.ToString()));
            var userImagePoint = new Point(20, 20);
            ctx.DrawImage(userImage, userImagePoint, new GraphicsOptions());
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(userImagePoint, userImage.Size));
            var levelBarMaxLevelWidth = 300ul;
            var gottenExp = levelBarMaxLevelWidth * (userData.Experience / (requiredExp * 1.0f));
            var levelBarY = userImage.Height - 30 + userImagePoint.Y;
            ctx.Fill(SixLabors.ImageSharp.Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            ctx.Fill(SixLabors.ImageSharp.Color.Green, new RectangleF(130, levelBarY, gottenExp, 30));
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            var font = SystemFonts.CreateFont("Arial", 25);
            ctx.DrawText($"{userData.Experience}/{requiredExp}", font,
                SixLabors.ImageSharp.Color.Black,
                new PointF(140, levelBarY + 2));
            ctx.DrawText($"Adventurer Level {adventurerLevel}", font, SixLabors.ImageSharp.Color.Black,
                new PointF(140, levelBarY - 33));
        });

        return image;
    }

    [Command("info")]
    [Description("Use to see basic details about yourself or a player")]
    [BotCommandCategory]
    public async ValueTask Execute(CommandContext ctx, [Parameter("user")] DiscordUser? author = null)
    {
        if (author is null) author = ctx.User;

        var userData = await DatabaseContext.Set<UserData>()
            .AsNoTrackingWithIdentityResolution()
            .Include(i => i.Items.Where(j => j is Coin || j is DivineShard || j is Stamina))
            .FirstOrDefaultAsync(i => i.DiscordId == author.Id);

        if (userData is null || userData.Tier == Tier.Unranked)
        {
            if (author == ctx.User)
            {
                await AskToDoBeginAsync(ctx);
            }
            else
            {
                var embed = new DiscordEmbedBuilder()
                    .WithUser(author)
                    .WithTitle("Hmm")
                    .WithDescription($"{author.Username} has not begun with /begin")
                    .WithColor(TypesFunction.GetDefaultObject<UserData>().Color);
                await ctx.RespondAsync(embed);
            }

            return;
        }

        var stamina = userData.Items.GetOrCreateItem<Stamina>();
        stamina.RefreshEnergyValue();


        var embedBuilder = new DiscordEmbedBuilder()
            .WithTitle("Info")
            .WithUser(author)
            .WithColor(userData.Color)
            .AddField("Coins", $"`{userData.Items.GetItemStacks(typeof(Coin))}`", true)
            .AddField("Tier", $"`{userData.Tier}`", true)
            .AddField("Date Started", $"`{userData.StartTime}`", true)
            .AddField("Time Till Next Day", $"`{BasicFunctions.TimeTillNextDay()}`", true)
            .AddField("Stamina", $"`{stamina.Stacks}`", true)
            .AddField("Divine Shards", $"`{userData.Items.GetItemStacks(typeof(DivineShard))}`")
            .WithImageUrl("attachment://info.png")
            .WithTimestamp(DateTime.Now);
        using var image = await GetInfoAsync(userData, author);
        await using var stream = new MemoryStream();

        await image.SaveAsPngAsync(stream);
        stream.Position = 0;
        var response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embedBuilder)
            .AddFile("info.png", stream);
        await ctx.RespondAsync(response);
    }
}