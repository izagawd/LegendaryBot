using Entities.LegendaryBot.Entities;
using Entities.LegendaryBot.Entities.Items;

namespace Entities.Models;
/// <summary>
/// An abstraction to combine all entities (items, character, blessings, etc)
/// for easier coding
/// </summary>
public class InventoryEntityContainer : InventoryEntityContainer<IInventoryEntity>
{
    public InventoryEntityContainer(IEnumerable<IInventoryEntity> entities) : base(entities)
    {
    }

    public void MergeItems()
    {
        var items = List.OfType<Item>().ToArray();
        List.RemoveAll(i => i is Item);
        foreach (var i in items)
        {
            
            var already = (Item?)List.FirstOrDefault(j => j.GetType() == i.GetType());
            if (already is not null)
            {
                checked
                {
                    already.Stacks += i.Stacks; 
                }
            }
          
            else
                Add(i);
        }
    }

    public override void MergeItemStacks()
    {
        MergeItems();
    }
}