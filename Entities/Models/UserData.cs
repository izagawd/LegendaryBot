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

public class UserData : ICanBeLeveledUp
{
    [NotMapped] private List<Quote>? _quotes;

    public UserData(ulong discordId) : this()
    {
        DiscordId = discordId;
    }

    public UserData()
    {
        Inventory = new UserDataInventoryCombined(this);
    }


    public Gender Gender { get; set; }
    public string Name { get; set; } = "Aether";


    public long Id { get; set; }

    public PlayerTeam? EquippedPlayerTeam { get; set; }

    public List<PlayerTeam> PlayerTeams { get; set; } = [];

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
    ///     This is used to refresh quest
    /// </summary>
    public DateTime LastTimeQuestWasChecked { get; set; } = DateTime.UtcNow.AddDays(-1);

    public List<Quote> Quotes => _quotes ??= new List<Quote>();
    public bool IsOccupied { get; set; }


    public Tier Tier { get; set; } = Tier.Unranked;

    public ulong DiscordId { get; set; }

    public int AdventurerLevel { get; set; } = 1;
    public DiscordColor Color { get; set; } = DiscordColor.Green;
    public string Language { get; set; } = "english";

    [NotMapped] public UserDataInventoryCombined Inventory { get; }

    public ItemContainer Items { get; } = new();


    [Timestamp] public uint Version { get; set; }

    public List<Gear> Gears { get; } = new();
    public List<Character> Characters { get; } = new();

    public void SortDupeCharacters()
    {
        foreach (var i in Characters.ToArray()
                     .GroupBy(i => i.GetType()))
        {
            var totalCharacters = i.ToArray();
            if (totalCharacters.Length > 1)
            {
                var mainCharacter = totalCharacters
                    .MinBy(j => j.DateAcquired)!;
                var dupes =
                    totalCharacters.Where(j => j != mainCharacter).ToArray();
                mainCharacter.DupeCount +=
                        dupes
                        .Select(j => j.DupeCount + 1)
                        .Sum();
                Characters.RemoveAll(j => dupes.Contains(j));
            }
        }
      
            
    }
    public List<Blessing> Blessings { get; } = new();


    public int Experience { get; protected set; }

    public int GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(level) * 10;
    }

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
        var expGainText = "";

        var levelBefore = AdventurerLevel;
        Experience += experienceToGain;


        var nextLevelEXP = GetRequiredExperienceToNextLevel(AdventurerLevel);
        while (Experience >= nextLevelEXP && AdventurerLevel < maxLevel)
        {
            Experience -= nextLevelEXP;
            AdventurerLevel += 1;
            nextLevelEXP = GetRequiredExperienceToNextLevel(AdventurerLevel);
        }

        expGainText += $"you gained {experienceToGain} exp for your adventurer level";
        if (levelBefore != AdventurerLevel)
            expGainText += $", and moved from level {levelBefore} to level {AdventurerLevel}";
        var excessExp = 0;
        if (Experience > nextLevelEXP) excessExp = Experience - nextLevelEXP;
        expGainText += "!";
        return new ExperienceGainResult { ExcessExperience = excessExp, Text = expGainText };
    }

    int ICanBeLeveledUp.Level => AdventurerLevel;


    /// <summary>
    ///     Receives rewards
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
            if (!i.IsValid) continue;
            rewardStringBuilder.Append($"{i.GiveRewardTo(this)}\n");
        }

        return rewardStringBuilder.ToString();
    }

    /// <summary>
    ///     Receives rewards
    /// </summary>
    /// <param name="rewards">the rewards</param>
    /// <returns>the receive rewards text</returns>
    public string ReceiveRewards(IEnumerable<Reward> rewards)
    {
        return ReceiveRewards(rewards.ToArray());
    }
}