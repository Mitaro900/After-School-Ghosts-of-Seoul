using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI questinfo1;
    [SerializeField] private TextMeshProUGUI questinfo2;

    private void Start()
    {
        questinfo1.text = "";
        questinfo2.text = "";
        SetupButtons();
    }

    private void OnEnable()
    {
        if (InventoryData.TryGetInstance(out _inventory))
        {
            _inventory.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }

        QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;
    }

    private void OnDisable()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= RefreshUI;

        QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
    }

    private void OnQuestUpdated(string questId)
    {
        RefreshQuestLog();
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
        RefreshQuestLog();

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

    public void RefreshQuestLog()
    {
        var quests = QuestManager.Instance.GetActiveQuests();

        foreach (var quest in quests)
        {
            string questId = quest.questId;

            var currentStep =
                QuestManager.Instance.GetTopAvailableStepForLog(questId);

            string currentObjective =
                currentStep != null
                    ? currentStep.label
                    : "진행 중 목표 없음";

            questinfo1.text = $"퀘스트: {quest.questName}";
            questinfo2.text = $"현재 목표: {currentObjective}";

            var doneSteps =
                QuestManager.Instance.GetDoneSteps(questId);

            foreach (var stepId in doneSteps)
            {
                string label =
                    QuestManager.Instance.GetStepLabel(questId, stepId);

                Debug.Log($"완료: {label}");

                questinfo1.text = "";
                questinfo2.text = "";

            }
        }
    }
}
