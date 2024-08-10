using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DiscordBotNet.Extensions;
using DiscordBotNet.LegendaryBot;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Blessings;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;
using DiscordBotNet.LegendaryBot.Quests;
using DiscordBotNet.LegendaryBot.Results;
using DiscordBotNet.LegendaryBot.Rewards;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace DiscordBotNet.Database.Models;




public enum Gender : byte
{
    Male, Female
}
public class UserData :   ICanBeLeveledUp
{


    public Gender Gender { get; set; }
    public string Name { get; set; } = "Aether";




    public ulong Id { get; set; }
    
    public UserData(ulong id) : this()
    {
        Id = id;
    }

    public UserData()
    {
    
        Inventory = new(this);
        
    }

    public PlayerTeam? EquippedPlayerTeam { get; set; }

    public List<PlayerTeam> PlayerTeams { get; set; } = [];

    public List<QuoteReaction> QuoteReactions { get; set; } = [];
    
    public async Task<Image<Rgba32>> GetInfoAsync(DiscordUser? user = null)
    {
        if (user is null)
        {
            user = await Bot.Client.GetUserAsync(Id);
        } 
        else if (user.Id != Id)
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
            ctx.DrawText($"{Experience}/{GetRequiredExperienceToNextLevel()}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY+2));
            ctx.DrawText($"Adventurer Level {AdventurerLevel}",font,SixLabors.ImageSharp.Color.Black,new PointF(140,levelBarY - 33));
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


    public List<Quest> Quests { get; } = new();
    /// <summary>
    /// This is used to refresh quest 
    /// </summary>
    public DateTime LastTimeQuestWasChecked { get; set; } = DateTime.UtcNow.AddDays(-1);

    [NotMapped]
    private List<Quote>? _quotes;
    public List<Quote> Quotes => _quotes ??= new();
    public bool IsOccupied { get; set; } = false;
    
    
    public int Experience { get; protected set; }
    public int GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(level) * 10;
    }
    public int GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(AdventurerLevel);
    }

    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(params Reward[] rewards)
    {
        var rewardStringBuilder = new StringBuilder("");
        var mergedRewards = Reward.MergeAllRewards(rewards)
            .Order();
        
        foreach (var i in mergedRewards)
        {
            if(!i.IsValid) continue;
            rewardStringBuilder.Append($"{i.GiveRewardTo(this)}\n");
        }

        return rewardStringBuilder.ToString();
    }

    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(IEnumerable<Reward> rewards)
    {
        return ReceiveRewards(rewards.ToArray());
    }

    
 

    public ExperienceGainResult IncreaseExp(int experienceToGain)
    {
        var maxLevel = 60;
        if (AdventurerLevel >= maxLevel)
            return new ExperienceGainResult() { ExcessExperience = experienceToGain, Text = $"you have already reached max level!" };
        var expGainText = "";
        
        var levelBefore = AdventurerLevel;
        Experience += experienceToGain;


        var nextLevelEXP =GetRequiredExperienceToNextLevel(AdventurerLevel);
        while (Experience >= nextLevelEXP && AdventurerLevel < maxLevel)
        {
            Experience -= nextLevelEXP;
            AdventurerLevel += 1;
            nextLevelEXP = GetRequiredExperienceToNextLevel(AdventurerLevel);
        }

        expGainText += $"you gained {experienceToGain} exp for your adventurer level";
        if (levelBefore != AdventurerLevel)
        {
            expGainText += $", and moved from level {levelBefore} to level {AdventurerLevel}";
        }
        int excessExp = 0;
        if (Experience > nextLevelEXP)
        {
            excessExp = Experience - nextLevelEXP;
        }
        expGainText += "!";
        return new ExperienceGainResult(){ExcessExperience = excessExp, Text = expGainText};
    }







    
 

    public Tier Tier { get; set; } = Tier.Unranked;

    
    int ICanBeLeveledUp.Level => AdventurerLevel;
    
    public int AdventurerLevel { get; set; } = 1;
    public DiscordColor Color { get; set; } = DiscordColor.Green;
    public string Language { get; set; } = "english";
    
    [NotMapped]
    public UserDataInventoryCombined Inventory { get; }

    public ItemContainer Items { get; } = new();


    [Timestamp]
    public uint Version { get; private set; }
    public List<Gear> Gears { get; } = new();
    public List<Character> Characters { get; } = new();

    public List<Blessing> Blessings { get; } = new();
}
public class UserDataDatabaseConfiguration : IEntityTypeConfiguration<UserData>
{
    public void Configure(EntityTypeBuilder<UserData> builder)
    {
        builder.Property(i => i.Color)
            .HasConversion(i => i.ToString(), j => new DiscordColor(j)
            );
        builder.HasMany(i => i.PlayerTeams)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId)
            .OnDelete(DeleteBehavior.Cascade);
                    
        builder
            .HasMany(i => i.Items)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Gears)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Blessings)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Characters)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasOne(i => i.EquippedPlayerTeam)
            .WithOne()
            .HasForeignKey<PlayerTeam>(i => i.IsEquipped)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(i => i.Quests)
            .WithOne()
            .HasForeignKey(i => i.UserDataId);
        builder
            .HasMany(i => i.Quotes)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);
        builder.HasMany(i => i.QuoteReactions)
            .WithOne(i => i.UserData)
            .HasForeignKey(i => i.UserDataId);

        
        
        builder.HasKey(i => i.Id);

    }
}