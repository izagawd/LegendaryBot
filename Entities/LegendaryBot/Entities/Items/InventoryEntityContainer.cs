using System.Collections;

namespace Entities.LegendaryBot.Entities.Items;

public interface IInventoryEntityContainer<TInventoryEntity> : IList<TInventoryEntity>
    where TInventoryEntity : IInventoryEntity
{
    public void MergeItemStacks();
}

public abstract class InventoryEntityContainer<TInventoryEntity> : IInventoryEntityContainer<TInventoryEntity>
    where TInventoryEntity : IInventoryEntity
{
    public InventoryEntityContainer()
    {
        List = new List<TInventoryEntity>();
    }

    public InventoryEntityContainer(IEnumerable<TInventoryEntity> entities) : this()
    {
        AddRange(entities);
    }

    protected List<TInventoryEntity> List { get; }
    public abstract void MergeItemStacks();

    public virtual IEnumerator<TInventoryEntity> GetEnumerator()
    {
        return List.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    public void Add(TInventoryEntity inventoryEntity)
    {
        List.Add(inventoryEntity);
    }

    public void Clear()
    {
        List.Clear();
    }

    public bool Contains(TInventoryEntity item)
    {
        return List.Contains(item);
    }

    public void CopyTo(TInventoryEntity[] array, int arrayIndex)
    {
        List.CopyTo(array, arrayIndex);
    }

    public bool Remove(TInventoryEntity item)
    {
        return List.Remove(item);
    }

    public int Count => List.Count;
    public bool IsReadOnly => false;

    public int IndexOf(TInventoryEntity item)
    {
        return List.IndexOf(item);
    }

    public void Insert(int index, TInventoryEntity item)
    {
        List.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        List.RemoveAt(index);
    }

    public TInventoryEntity this[int index]
    {
        get => List[index];
        set => List[index] = value;
    }

    public void AddRange(IEnumerable<TInventoryEntity> enumerable)
    {
        List.AddRange(enumerable);
    }
}