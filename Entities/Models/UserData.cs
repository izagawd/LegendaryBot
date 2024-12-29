using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using DSharpPlus.Entities;
using Entities.LegendaryBot;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Quests;
using Entities.LegendaryBot.Results;
using Entities.LegendaryBot.Rewards;

namespace Entities.Models;

public enum Gender : byte
{
    Male,
    Female
}

/// <summary>
/// Class representing the data of a user
/// </summary>
public class UserData : ICanBeLeveledUp
{
    /// <summary>
    /// The quotes the user has posted
    /// </summary>
    [NotMapped] private List<Quote>? _quotes;

    public UserData(ulong discordId) : this()
    {
        DiscordId = discordId;
    }

    public UserData()
    {
        Inventory = new UserDataInventoryCombined(this);
    }


    /// <summary>
    /// The gender of the user (this will affect how their player class looks like)
    /// </summary>
    public Gender Gender { get; set; }
    
    
    /// <summary>
    /// The name of the player
    /// </summary>
    public string Name { get; set; } = "Aether";


    public long Id { get; set; }

    /// <summary>
    /// The team the user has currently equipped. This team will be used everytime the user
    /// enters combat
    /// </summary>
    public PlayerTeam? EquippedPlayerTeam { get; set; }

    /// <summary>
    /// All the teams the player has
    /// </summary>
    public List<PlayerTeam> PlayerTeams { get; set; } = [];

    /// <summary>
    /// The reactions a player has given to quates (it is either a like or a dislike)
    /// </summary>
    public List<QuoteReaction> QuoteReactions { get; set; } = [];

    /// <summary>
    ///     Time that this player started their journey
    /// </summary>
    public DateTime StartTime { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    ///     Call this method if you want this instance to update it's values if it should be updated because it is a new day
    /// </summary>


    public List<Quest> Quests { get; } = new();

    /// <summary>
    /// This is used to refresh quest
    /// </summary>
    public DateTime LastTimeQuestWasChecked { get; set; } = DateTime.UtcNow.AddDays(-1);

    public List<Quote> Quotes => _quotes ??= new List<Quote>();
    
    
    /// <summary>
    /// Whether or not the user is occupied. Will be true if EG: the user is fighting or
    /// editing equipment
    /// </summary>
    public bool IsOccupied { get; set; }


    public Tier Tier { get; set; } = Tier.Unranked;

    /// <summary>
    /// The discord id of the user
    /// </summary>
    public ulong DiscordId { get; set; }

    /// <summary>
    /// This can be seen as their overall level in the game
    /// </summary>
    public int AdventurerLevel { get; set; } = 1;
    
    /// <summary>
    /// This color will be used for the embed messages the discord bot sends relating
    /// to this user
    /// </summary>
    public DiscordColor Color { get; set; } = DiscordColor.Green;
    public string Language { get; set; } = "english";

    
    
    /// <summary>
    /// Abstraction that combines eberything the user has (eg items, blessings, characters, etc)
    /// </summary>
    [NotMapped] public UserDataInventoryCombined Inventory { get; }

    
    
    /// <summary>
    /// A collection of all the items the user has
    /// </summary>
    public ItemContainer Items { get; } = new();


    [Timestamp] public uint Version { get; set; }
    
    /// <summary>
    /// A collection of all the gears the user has
    /// </summary>
    public List<Gear> Gears { get; } = new();
    /// <summary>
    /// A collection of all the characters the user has
    /// </summary>
    public List<Character> Characters { get; } = new();
    /// <summary>
    /// A collection of all the bkessings the user has
    /// </summary>

    public List<Blessing> Blessings { get; } = new();

    /// <summary>
    /// The experience that would be used to increase their game level
    /// </summary>
    public int Experience { get; protected set; }

    public int GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(level) * 10;
    }
    /// <summary>
    /// Collection of trackers that would help in keeping track fo all the summons the user has done
    /// </summary>
    public List<SummonsTracker> SummonsTrackers { get; set; }
    public int GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(AdventurerLevel);
    }


    public ExperienceGainResult IncreaseExp(int experienceToGain)
    {
        var maxLevel = 60;
        if (AdventurerLevel >= maxLevel)
            return new ExperienceGainResult
                { ExcessExperience = experienceToGain, Text = "you have already reached max level!" };
        

        var levelBefore = AdventurerLevel;
        checked
        {
            Experience += experienceToGain;
        }

        var nextLevelEXP = GetRequiredExperienceToNextLevel(AdventurerLevel);
        while (Experience >= nextLevelEXP && AdventurerLevel < maxLevel)
        {
            Experience -= nextLevelEXP;
            AdventurerLevel += 1;
            nextLevelEXP = GetRequiredExperienceToNextLevel(AdventurerLevel);
        }

        var expGainText = $"you gained {experienceToGain} exp for your adventurer level";
        if (levelBefore != AdventurerLevel)
            expGainText += $", and moved from level {levelBefore} to level {AdventurerLevel}";
        var excessExp = 0;
        if (Experience > nextLevelEXP) excessExp = Experience - nextLevelEXP;
        expGainText += "!";
        return new ExperienceGainResult { ExcessExperience = excessExp, Text = expGainText };
    }

    int ICanBeLeveledUp.Level => AdventurerLevel;


    /// <summary>
    /// Receives rewards
    /// </summary>
    /// <param name="userDataQueryable"></param>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public async  Task<string> ReceiveRewardsAsync(IQueryable<UserData> userDataQueryable, params Reward[] rewards)
    {
        var rewardStringBuilder = new StringBuilder("");
        var mergedRewards = Reward.MergeAllRewards(rewards)
            .Order();

        foreach (var i in mergedRewards)
        {
            if (!i.IsValid) continue;
            rewardStringBuilder.Append($"{await i.GiveRewardToAsync(this, userDataQueryable)}\n");
        }

        return rewardStringBuilder.ToString();
    }

    /// <summary>
    ///  Receives rewards
    /// </summary>
    /// <param name="userDataQueryable"></param>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public Task<string> ReceiveRewardsAsync(IQueryable<UserData> userDataQueryable, IEnumerable<Reward> rewards)
    {
        return ReceiveRewardsAsync(userDataQueryable, rewards.ToArray());
    }
}