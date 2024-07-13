using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.BattleEvents;
using DiscordBotNet.LegendaryBot.BattleEvents.EventArgs;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;
using DiscordBotNet.LegendaryBot.ModifierInterfaces;
using DiscordBotNet.LegendaryBot.Results;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities;

public abstract class BattleEntity : Entity, ICanBeLeveledUp
{
    
    public int Level { get;  set; } = 1;
    [NotMapped] public virtual int MaxLevel => 1;
    public virtual ExperienceGainResult  IncreaseExp(long experienceToGain)
    {
        return new ExperienceGainResult();
    }

    public virtual long GetRequiredExperienceToNextLevel(int level)
    {
        return BattleFunctionality.NextLevelFormula(level);
    }
    public long GetRequiredExperienceToNextLevel()
    {
        return GetRequiredExperienceToNextLevel(Level);
    }
    public long Experience
    {
        get;
        protected set;
    }

}