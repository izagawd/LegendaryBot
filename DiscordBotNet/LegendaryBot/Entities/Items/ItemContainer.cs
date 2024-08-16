namespace DiscordBotNet.LegendaryBot.Entities.Items;

public class ItemContainer : InventoryEntityContainer<Item>
{
    public TItem GetOrCreateItem<TItem>() where TItem : Item
    {
        return (TItem)GetOrCreateItem(typeof(TItem));
    }

    public Item GetOrCreateItem(Type itemType)
    {
        if (!itemType.IsAssignableTo(typeof(Item)) || itemType.IsAbstract)
            throw new Exception();
        var gotten = List.FirstOrDefault(i => i.GetType() == itemType);
        if (gotten is null)
        {
            gotten = (Item)Activator.CreateInstance(itemType)!;
            gotten.Stacks = 0;
            List.Add(gotten);
        }

        return gotten;
    }

    public override void MergeItemStacks()
    {
        var items = List.ToArray();
        List.Clear();
        foreach (var i in items)
        {
            var already = List.FirstOrDefault(j => j.GetType() == i.GetType());
            if (already is not null)
                already.Stacks += i.Stacks;
            else
                Add(i);
        }
    }

    /// <summary>
    ///     Gets the amount of stacks of a specific item
    /// </summary>
    public int GetItemStacks(Type itemType)
    {
        if (!itemType.IsSubclassOf(typeof(Item))) throw new Exception("expected item type");
        return List.Where(i => i.GetType() == itemType).Select(i => i.Stacks).Sum();
    }

    /// <summary>
    ///     Gets the amount of stacks of a specific item
    /// </summary>
    public int GetItemStacks<TItem>() where TItem : Item
    {
        return GetItemStacks(typeof(TItem));
    }

    /// <summary>
    ///     Add an amount of stacks for a specific item
    /// </summary>
    public void AddItemStacks(Type itemType, int stacksToAdd)
    {
        if (!itemType.IsSubclassOf(typeof(Item))) throw new Exception("expected item type");
        var first = List.FirstOrDefault(i => i.GetType() == itemType);
        if (first is null)
        {
            first = (Item)Activator.CreateInstance(itemType)!;
            first.Stacks = 0;
        }

        first.Stacks += stacksToAdd;
    }

    public void AddItemStacks<TItem>(int stacksToAdd) where TItem : Item
    {
        AddItemStacks(typeof(TItem), stacksToAdd);
    }

    public void RemoveItemStacks<TItem>(int stacksToRemove) where TItem : Item
    {
        RemoveItemStacks(typeof(TItem), stacksToRemove);
    }

    /// <summary>
    ///     Removes an indicated amount of stacks from the inputted item
    /// </summary>
    public void RemoveItemStacks(Type itemType, int stacksToRemove)
    {
        if (!itemType.IsSubclassOf(typeof(Item))) throw new Exception("expected item type");
        foreach (var i in List.Where(i => i.GetType() == itemType).ToArray())
        {
            if (i.Stacks >= stacksToRemove)
            {
                i.Stacks -= stacksToRemove;
                stacksToRemove = 0;
            }
            else if (stacksToRemove > i.Stacks)
            {
                stacksToRemove -= i.Stacks;
                i.Stacks = 0;
            }

            if (i.Stacks <= 0) List.Remove(i);
            if (stacksToRemove <= 0)
                break;
        }
    }
}