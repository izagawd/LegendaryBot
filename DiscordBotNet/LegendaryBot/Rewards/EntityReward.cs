using System.Text;
using DiscordBotNet.Database.Models;
using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Gears;
using DiscordBotNet.LegendaryBot.Entities.Items;

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
    public InventoryEntityContainer EntitiesToReward { get;  }
    public EntityReward(IEnumerable<IInventoryEntity> entitiesToReward)
    {
        EntitiesToReward = new InventoryEntityContainer(entitiesToReward
            .Where(i => i is not null));
        EntitiesToReward.MergeDuplicates();

    }

    public override string GiveRewardTo(UserData userData)
    {
        var stringBuilder = new StringBuilder($"{userData.Name} got: ");
        EntitiesToReward.MergeDuplicates();
        userData.Inventory.AddRange(EntitiesToReward);
        userData.Inventory.MergeDuplicates();

        Dictionary<string, int> nameSorter = [];
        foreach (var i in EntitiesToReward)
        {
            stringBuilder.Append($"\nName: {i.Name} | Type : {BasicFunctionality.Englishify(i.TypeGroup.Name)} | Rarity : {i.Rarity.ToString().Englishify()}");
            if (i is Item item)
            {
                stringBuilder.Append($" | {nameof(Item.Stacks)}: {item.Stacks}");
            }

            else if (i is Gear gear)
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