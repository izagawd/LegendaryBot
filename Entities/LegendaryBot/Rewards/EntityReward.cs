using System.Text;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.Models;

namespace Entities.LegendaryBot.Rewards;

/// <summary>
///     Rewards the user with entities to add to their inventoru
/// </summary>
public class EntityReward : Reward
{
    public EntityReward(IEnumerable<IInventoryEntity> entitiesToReward)
    {
        EntitiesToReward = new InventoryEntityContainer(entitiesToReward
            .Where(i => i is not null));
        EntitiesToReward.MergeItemStacks();
    }

    public override int Priority => 2;

    public override bool IsValid => EntitiesToReward.Count > 0;
    public InventoryEntityContainer EntitiesToReward { get; }

    public override Reward MergeWith(Reward reward)
    {
        if (reward is not EntityReward entityReward) throw new Exception("Reward type given is not of same type");
        return new EntityReward(EntitiesToReward.Union(entityReward.EntitiesToReward));
    }

    public static string GetDisplayString(IInventoryEntity entity)
    {
        switch (entity)
        {
            case Character chara:
                return $"{chara.Name} • Lvl {chara.Level}";
            case Item item:
                return $"{item.Name} • Stacks: {item.Stacks}";
            case Blessing blessing:
                return $"{blessing.Name}";
            case Gear gear:
                return gear.DisplayString;
            default:
                return "NOOOO";
        }
    }

    public override string GiveRewardTo(UserData userData)
    {
        var stringBuilder = new StringBuilder($"{userData.Name} got:\n ");
        EntitiesToReward.MergeItemStacks();
        userData.Inventory.AddRange(EntitiesToReward);
        userData.Inventory.MergeItemStacks();


        foreach (var i in EntitiesToReward) stringBuilder.Append($"`{GetDisplayString(i)}`\n");


        return stringBuilder.ToString();
    }
}