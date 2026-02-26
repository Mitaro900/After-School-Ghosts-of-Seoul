using Singleton.Data;
using System;
using System.Collections.Generic;
using static UnityEditor.Progress;

public class InventoryData : SingletonData<InventoryData>
{
    private const int MaxInventorySize = 7;

    private List<ItemData> items = new List<ItemData>(MaxInventorySize);
    public IReadOnlyList<ItemData> Items => items.AsReadOnly();

    public event Action OnInventoryChanged;

    #region Singleton
    protected override bool InitInstance()
    {
        for (int i = 0; i < MaxInventorySize; i++)
        {
            items.Add(null);
        }

        return true;
    }

    protected override void ReleaseInstance()
    {
        items.Clear();
    }
    #endregion

    public bool AddItem(ItemData item)
    {
        if (items.Count >= MaxInventorySize) return false;

        if (!items.Contains(item))
            items.Add(item);

        NotifyChanged();

        return true;
    }

    public void RemoveItem(ItemData item)
    {
        if (items.Contains(item))
            items.Remove(item);

        NotifyChanged();
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }

    private void NotifyChanged()
    {
        if (Singleton.SingletonGate.IsBlocked)
            return;

        OnInventoryChanged?.Invoke();
    }
}
