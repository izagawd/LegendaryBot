using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using Microsoft.Extensions.Primitives;

namespace DiscordBotNet.LegendaryBot.Rewards;
/// <summary>
/// Rewards the user with entities to add to their inventoru
/// </summary>
public class EntityReward : Reward
{
    public override int Priority => 2;

    public override Reward MergeWith(Reward reward)
    {
        if (reward is not EntityReward entityReward) throw new Exception("Reward type given is not of same type");
        return new EntityReward(EntitiesToReward.Union(entityReward.EntitiesToReward));
    }

    public override bool IsValid => EntitiesToReward.Count() > 0;
    public EntityContainer EntitiesToReward { get;  }
    public EntityReward(IEnumerable<Entity> entitiesToReward)
    {
        EntitiesToReward = new EntityContainer(entitiesToReward
            .Where(i => i is not null));
        EntitiesToReward.Arrange();

    }

    public override string GiveRewardTo(UserData userData)
    {
        var stringBuilder = new StringBuilder($"{userData.Name} got: ");
        EntitiesToReward.Arrange();
        userData.Inventory.AddRange(EntitiesToReward);
        userData.Inventory.Arrange();

        Dictionary<string, int> nameSorter = [];
        foreach (var i in EntitiesToReward)
        {
            stringBuilder.Append($"\nName: {i.Name} | Type : {BasicFunctionality.Englishify(i.TypeGroup.Name)} | Rarity : {i.Rarity}");
            
            if (i is Gear gear)
            {
                gear.MainStat.SetMainStatValue(gear.Rarity);
                stringBuilder.Append($"\n\nMainStat = {gear.MainStat.Name}: {gear.MainStat.Value}");
                if (gear.Substats.Any())
                {
                    stringBuilder.Append("\nSubstats: ");
                    foreach (var j in gear.Substats)
                    {
                        stringBuilder.Append($"\n{j.AsNameAndValue()}");
                    }
                }

                stringBuilder.Append("\n");
            }
        }

    

        return stringBuilder.ToString();

    }
}