using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InventorySlot
{
    public Button button;
    public Image itemImage;
    public ItemData itemData;
}

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject hudPanel;

    private InventoryData _inventory;

    [SerializeField] private List<InventorySlot> slots;

    private void Start()
    {
        SetupButtons();
    }

    private void OnEnable()
    {
        if(InventoryData.TryGetInstance(out _inventory))
        {
            _inventory.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDisable()
    {
        if (_inventory != null)
        {
            _inventory.OnInventoryChanged -= RefreshUI;
        }
        _inventory = null;
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
            GameManager.Instance.Player.TalkToSlotItem(slot.itemData.ItemSlotPrompt);
        }
    }

    // 아이템 등록 (Item, QuestManager 사용중)
    public bool AddItem(ItemData newItem)
    {
        if(_inventory.AddItem(newItem))
        {
            QuestManager.Instance.CheckQuestComplete(newItem.ItemName);

            return true;
        }

        return false;
    }

    // 아이템 제거
    public void RemoveItems(List<ItemData> items)
    {
        foreach (var item in items)
        {
            RemoveItem(item);
        }
    }

    public void RemoveItem(ItemData item)
    {
        _inventory.RemoveItem(item);
    }

    // 현재 아이템을 들고있는지 확인 (QuestManager에서 사용)
    public bool HasAllItems(List<ItemData> items)
    {
        foreach (var item in items)
        {
            if (HasItem(item))
                return true;
        }
        return false;
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
        for (int i = 0; i < _inventory.Items.Count; i++)
        {
            slots[i].itemData = _inventory.Items[i];
        }

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

    public void OpenLog()
    {
        var uiData = new UIBaseData
        {
            OnShow = () => HideHUD(),
            OnClose = () => ShowHUD()
        };
        UIManager.Instance.OpenUI<DialogueLogUI>(uiData);
    }

    public void ShowHUD()
    {
        hudPanel.SetActive(true);
    }

    public void HideHUD()
    {
        hudPanel.SetActive(false);
    }
}
