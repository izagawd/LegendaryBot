using System.Text;
using BasicFunctionality;
using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.BattleEntities.Blessings;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;
using Entities.LegendaryBot.Entities.BattleEntities.Gears;
using Entities.LegendaryBot.Entities.Items;
using Entities.LegendaryBot.Entities.Items.ExpIncreaseMaterial;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

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

    public static string GetDisplayString(IInventoryEntity entity, UserData userData)
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

    private readonly int DivinePermitTypeId = TypesFunction.GetDefaultObject<DivinePermit>().TypeId;
    public override async Task<string> GiveRewardToAsync(UserData userData, IQueryable<UserData> userDataQueryable)
    {
        
        var stringBuilder = new StringBuilder($"{userData.Name} got:\n ");
        EntitiesToReward.MergeItemStacks();
        var gottenItemsTypeIds = EntitiesToReward
            .OfType<Item>()
            .Select(i => i.TypeId)
            .Append(DivinePermitTypeId)
            .Distinct()
            .ToArray();
        var gottenCharactersTypeIds = EntitiesToReward
            .OfType<Character>()
            .Select(i => i.TypeId)
            .Distinct()
            .ToArray();
        await userDataQueryable
            .Where(i => i.Id == userData.Id)
            .Include(i =>
                i.Items.Where(j => gottenItemsTypeIds.Contains(j.TypeId)))
            .Include(i => i.Characters.Where(j => gottenCharactersTypeIds.Contains(j.TypeId)))
            .LoadAsync();

        var dupeExcessBuilder = new StringBuilder();
        foreach (var i in EntitiesToReward
                     .OfType<Character>()
                     .ToArray()
                     .GroupBy(i => i.TypeId))
        {
            var already = userData.Characters.FirstOrDefault(j => j.TypeId == i.Key);
            List<Character> toWorkWith = i.ToList();
            if (already is null)
            {
                already = toWorkWith.First();
                toWorkWith.Remove(already);
            }
            int dupeCounts = toWorkWith.Count;
            int excess = 0;
            if (already.DivineEcho + dupeCounts > Character.MaxDivineEcho)
            {
                excess = (already.DivineEcho + dupeCounts) - Character.MaxDivineEcho;
                if (already.Rarity == Rarity.FourStar)
                {
                    EntitiesToReward.Add(new DivinePermit(){Stacks = 10 * excess});
                } else if (already.Rarity == Rarity.FiveStar)
                {
                    EntitiesToReward.Add(new DivinePermit(){Stacks = 50 * excess});
                }
                already.DivineEcho = Character.MaxDivineEcho; // Set it to the maximum value
            }
            else
            {
                already.DivineEcho += dupeCounts; // No excess, just add the counts
            }
            var usedDupes = dupeCounts - excess;
            if (usedDupes > 0)
            {
                dupeExcessBuilder.Append($"{already.Name} x{usedDupes} (dupes)\n");
            }
            if (excess > 0)
            {
                dupeExcessBuilder.Append($"{already.Name} x{excess} (excess)\n");
            }
            foreach (var j in EntitiesToReward
                         .OfType<Character>()
                         .Where(j => j != already && j.TypeId == already.TypeId)
                         .ToArray())
            {
                EntitiesToReward.Remove(j);
            }
        }
        EntitiesToReward.MergeItemStacks();
        userData.Inventory.AddRange(EntitiesToReward);
        userData.Inventory.MergeItemStacks();
        foreach (var i in EntitiesToReward)
            stringBuilder.Append($"{GetDisplayString(i, userData)}\n");
        stringBuilder.Append(dupeExcessBuilder);
        return stringBuilder.ToString();
    }
}