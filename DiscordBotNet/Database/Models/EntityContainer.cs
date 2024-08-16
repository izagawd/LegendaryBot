using DiscordBotNet.LegendaryBot.Entities;
using DiscordBotNet.LegendaryBot.Entities.Items;

namespace DiscordBotNet.Database.Models;

public class InventoryEntityContainer : InventoryEntityContainer<IInventoryEntity>
{
    public InventoryEntityContainer(IEnumerable<IInventoryEntity> entities) : base(entities)
    {
    }

    public void MergeItems()
    {
        var items = List.OfType<Item>().ToArray();
        List.RemoveAll(i => items.Contains(i));
        foreach (var i in items)
        {
            var already = (Item?)List.FirstOrDefault(j => j.GetType() == i.GetType());
            if (already is not null)
                already.Stacks += i.Stacks;
            else
                Add(i);
        }
    }

    public override void MergeItemStacks()
    {
        MergeItems();
    }
}