using Singleton.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryData : SingletonData<InventoryData>
{
    private const int MaxInventorySize = 7;

    private List<ItemData> items = new List<ItemData>();
    public IReadOnlyList<ItemData> Items => items;

    public event Action OnInventoryChanged;

    #region Singleton
    protected override bool InitInstance()
    {
        items.Clear();

        for (int i = 0; i < MaxInventorySize; i++)
        {
            items.Add(null);
        }

        Debug.Log("[Inventory] 인스턴스 초기화 완료 (7슬롯 생성)");

        return true;
    }

    protected override void ReleaseInstance()
    {
        items.Clear();
    }
    #endregion

    public bool AddItem(ItemData item)
    {
        if (item == null) return false;

        // 1. 리스트가 초기화되지 않았을 경우를 대비한 방어 코드
        if (items == null || items.Count == 0)
        {
            items = new List<ItemData>();
            for (int i = 0; i < 7; i++) items.Add(null);
        }

        // 2. 빈 칸(null)을 찾아 아이템을 넣습니다.
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;

                // 3. 변경 사항 알림 (이게 호출되어야 HUD가 새로고침됩니다)
                NotifyChanged();
                return true;
            }
        }

        return false;
    }

    public void RemoveItem(ItemData item)
    {
        int index = items.IndexOf(item);
        if (index != -1)
        {
            items[index] = null; // 리스트에서 제거하는 게 아니라 null로 만들어 슬롯 유지
            NotifyChanged();
        }
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
