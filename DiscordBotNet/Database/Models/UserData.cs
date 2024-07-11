using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.Database.Models;


public class UserData :   ICanBeLeveledUp
{
    public long Id { get; set; }
    
    public UserData(long id) : this()
    {
        Id = id;
    }

    public UserData()
    {
    
    }

    public PlayerTeam? EquippedPlayerTeam { get; set; }

    public List<PlayerTeam> PlayerTeams { get; set; } = [];

    public List<QuoteReaction> QuoteReactions { get; set; } = [];
    
    public async Task<Image<Rgba32>> GetInfoAsync(DiscordUser? user = null)
    {
        if (user is null)
        {
            user = await Bot.Client.GetUserAsync((ulong) Id);
        } 
        else if ((long)user.Id != Id)
        {
            throw new Exception("discord user's ID does not match user data's id");
        }
        using var userImage = await BasicFunctionality.GetImageFromUrlAsync(user.AvatarUrl);
        var image = new Image<Rgba32>(500, 150);
        userImage.Mutate(ctx => ctx.Resize(new Size(100,100)));
        image.Mutate(ctx =>
        {
            ctx.BackgroundColor(Color.ToImageSharpColor());
            var userImagePoint = new Point(20, 20);
            ctx.DrawImage(userImage,userImagePoint, new GraphicsOptions());
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(userImagePoint,userImage.Size));
            var levelBarMaxLevelWidth = 300ul;
            var gottenExp = levelBarMaxLevelWidth * (Experience/(GetRequiredExperienceToNextLevel() * 1.0f));
            var levelBarY = userImage.Height - 30 + userImagePoint.Y;
            ctx.Fill(SixLabors.ImageSharp.Color.Gray, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            ctx.Fill(SixLabors.ImageSharp.Color.Green, new RectangleF(130, levelBarY, gottenExp, 30));
            ctx.Draw(SixLabors.ImageSharp.Color.Black, 3, new RectangleF(130, levelBarY, levelBarMaxLevelWidth, 30));
            var font = SystemFonts.CreateFont("Arial", 25);
            ctx.DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY));
            ctx.DrawText($"Level {Level}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY - 33));
        });

        return image;
    }
    /// <summary>
    /// Time that this player started their journey
    /// </summary>
    public DateTime StartTime { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Call this method if you want this instance to update it's values if it should be updated because it is a new day
    /// 
    /// </summary>



    public List<Quest> Quests { get; set; } = [];
    /// <summary>
    /// This is used to refresh stuff like daily quests
    /// </summary>
    public DateTime LastTimeChecked { get; set; } = DateTime.UtcNow.AddDays(-1);
    public List<Quote> Quotes { get; protected set; } = new();
    public bool IsOccupied { get; set; } = false;
    public long Experience { get; protected set; }
    public long GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(level) * 10;
    }
    public long GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }
    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="name">the name of the user</param>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(string name, params Reward[] rewards)
    {
        var rewardStringBuilder = new StringBuilder("");
        var mergedRewards = Reward.MergeAllRewards(rewards)
            .Order();
        
        foreach (var i in mergedRewards)
        {
            if(!i.IsValid) continue;
            rewardStringBuilder.Append($"{i.GiveRewardTo(this, name)}\n");
        }

        return rewardStringBuilder.ToString();
    }
    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="name">the name of the user</param>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(string name,IEnumerable<Reward> rewards)
    {
        return ReceiveRewards(name,rewards.ToArray());
    }
    public ExperienceGainResult IncreaseExp(long exp)
    {
        var maxLevel = 60;
        if (Level >= maxLevel)
            return new ExperienceGainResult() { ExcessExperience = exp, Text = $"you have already reached max level!" };
        string expGainText = "";
        
        var levelBefore = Level;
        Experience += exp;


        var nextLevelEXP =GetRequiredExperienceToNextLevel(Level);
        while (Experience >= nextLevelEXP && Level < maxLevel)
        {
            Experience -= nextLevelEXP;
            Level += 1;
            nextLevelEXP = GetRequiredExperienceToNextLevel(Level);
        }

        expGainText += $"you gained {exp} exp";
        if (levelBefore != Level)
        {
            expGainText += $", and moved from level {levelBefore} to level {Level}";
        }
        long excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }
        expGainText += "!";
        return new ExperienceGainResult(){ExcessExperience = excessExp, Text = expGainText};
    }

    [NotMapped]
    public bool DailyPending => LastTimeDailyWasChecked.Date != DateTime.UtcNow.Date;
    public DateTime LastTimeDailyWasChecked { get; set; } = DateTime.UtcNow.AddDays(-1);
    public long StandardPrayers { get; set; } = 0;
    
    public long SupremePrayers { get; set; } = 0;

    public long ShardsOfTheGods { get; set; } = 0;
    public long Coins { get; set; } = 5000;


    public Tier Tier { get; set; } = Tier.Unranked;
    public int Level { get; set; } = 1;
    public DiscordColor Color { get; set; } = DiscordColor.Green;
    public string Language { get; set; } = "english";
    public List<Entity> Inventory { get; protected set; } = [];

}
