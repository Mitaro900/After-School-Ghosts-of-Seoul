using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed
}

public class QuestManager : Singleton<QuestManager>
{
    [SerializeField] private List<QuestData> allQuests; // 모든 퀘스트 등록 (위치 정해지면 자동으로 모두 등록하게 해보기)

    private Dictionary<string, QuestData> questDatabase = new();    // questId → QuestData (퀘스트 정보 저장)
    private Dictionary<string, QuestState> questStates = new();     // questId → 현재 퀘스트 상태


    protected override void Awake()
    {
        base.Awake();
        InitializeQuests(); // 게임 시작 시 퀘스트 초기화
    }


    // 모든 퀘스트를 NotStarted 상태로 초기화
    private void InitializeQuests()
    {
        foreach (var quest in allQuests)
        {
            // 중복 등록 방지
            if (!questStates.ContainsKey(quest.questId))
            {
                questStates.Add(quest.questId, QuestState.NotStarted);
                questDatabase.Add(quest.questId, quest);
            }
        }
    }


    // 퀘스트 현재 상태
    public QuestState GetQuestState(string questId)
    {
        if (questStates.TryGetValue(questId, out var state))
            return state;

        return QuestState.NotStarted;
    }


    // 퀘스트 시작 (NotStarted → InProgress)
    public void StartQuest(string questId)
    {
        if (!questStates.ContainsKey(questId))
            return;

        questStates[questId] = QuestState.InProgress;
    }


    // 퀘스트 완료 처리 (InProgress → Completed)
    public void CompleteQuest(string questId)
    {
        if (!questDatabase.TryGetValue(questId, out var quest))
            return;

        questStates[questId] = QuestState.Completed;


        // 인벤토리에 아이템 지급 (보상 활성화시)
        if (quest.givesReward)
        {
            GiveReward(quest);
        }
    }


    // 보상 아이템 지급
    private void GiveReward(QuestData quest)
    {
        foreach (var reward in quest.rewardItems)
        {
            InventoryManager.Instance.AddItem(reward);
        }
    }


    // 퀘스트 완료 조건 체크 (완료 조건 아이템을 가지고 있는지 검사)
    public bool CheckQuestComplete(string questId)
    {
        if (!questDatabase.ContainsKey(questId))
            return false;

        var quest = questDatabase[questId];

        // 전체 조건 한 번만 검사
        if (!InventoryManager.Instance.HasAllItems(quest.requiredItems))
            return false;

        // 조건 만족 시 해당 아이템들 제거, 퀘스트 완료 처리
        InventoryManager.Instance.RemoveItems(quest.requiredItems);
        CompleteQuest(questId);
        return true;
    }
}
