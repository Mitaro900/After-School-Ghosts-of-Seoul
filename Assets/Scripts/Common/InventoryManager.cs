using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlot
{
    public Button button;
    public Image itemImage;
    public ItemData itemData;
}

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField] private Player player;
    public List<InventorySlot> slots;

    private void Start()
    {
        SetupButtons();
        RefreshUI();
    }


    // 버튼 등록
    private void SetupButtons()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;
            slots[i].button.onClick.AddListener(() => OnSlotClicked(index));
        }
    }


    // 아이템 클릭시 관련 대사
    private void OnSlotClicked(int index)
    {
        var slot = slots[index];

        if (slot.itemData != null)
        {
            player.TalkToSlotItem(slot.itemData.ItemSlotPrompt);
        }
    }


    // 아이템 등록 (Item, QuestManager 사용중)
    public bool AddItem(ItemData newItem)
    {
        foreach (var slot in slots)
        {
            if (slot.itemData == null)
            {
                slot.itemData = newItem;
                RefreshUI();
                return true;
            }
        }

        return false;
    }


    // 아이템 제거 (QuestManager에서 사용)
    public void RemoveItems(List<ItemData> items)
    {
        foreach (var item in items)
        {
            RemoveItem(item);
        }
    }

    public void RemoveItem(ItemData item)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].itemData == item)
            {
                // 현재 칸 비우기
                slots[i].itemData = null;

                // 뒤에 있는 아이템들 앞으로 당기기
                for (int j = i; j < slots.Count - 1; j++)
                {
                    slots[j].itemData = slots[j + 1].itemData;
                }

                // 마지막 칸 비우기
                slots[slots.Count - 1].itemData = null;

                RefreshUI();
                return;
            }
        }
    }


    // 현재 아이템을 들고있는지 확인 (QuestManager에서 사용)
    public bool HasAllItems(List<ItemData> items)
    {
        foreach (var item in items)
        {
            if (!HasItem(item))
                return false;
        }
        return true;
    }

    public bool HasItem(ItemData item)
    {
        foreach (var slot in slots)
        {
            if (slot.itemData == item)
                return true;
        }

        return false;
    }


    // 아이템 이미지 등록 및 제거
    private void RefreshUI()
    {
        foreach (var slot in slots)
        {
            if (slot.itemData == null)
            {
                slot.itemImage.gameObject.SetActive(false);
            }
            else
            {
                slot.itemImage.gameObject.SetActive(true);
                slot.itemImage.sprite = slot.itemData.ItemImage;
            }
        }
    }
}
