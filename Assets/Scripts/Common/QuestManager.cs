using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed
}

public class QuestManager : Singleton<QuestManager>
{
    [SerializeField] private List<ItemData> allItem;

    private Dictionary<string, QuestState> questStates = new();
    private Dictionary<string, ItemData> questDatabase = new();

    protected override void Awake()
    {
        base.Awake();
        InitializeQuests();
    }


    // 퀘스트 스타트 (퀘스트 중으로 만듬)
    public void StartQuest(ItemData item)
    {
        questStates[item.ItemName] = QuestState.InProgress;
    }


    // 퀘스트 완료 (퀘스트 완료로 만듬)
    public void CompleteQuest(ItemData item)
    {
        questStates[item.ItemName] = QuestState.Completed;

        ItemData quest = questDatabase[item.ItemName];
    }


    // 퀘스트가 완료되었는지 확인
    public QuestState GetQuestState(ItemData item)
    {
        if (questStates.ContainsKey(item.ItemName))
            return questStates[item.ItemName];

        return QuestState.NotStarted;
    }


    // 모든 퀘스트 아이템 적용
    private void InitializeQuests()
    {
        foreach (var quest in allItem)
        {
            if (!questStates.ContainsKey(quest.ItemName))
            {
                questStates.Add(quest.ItemName, QuestState.NotStarted);
                questDatabase.Add(quest.ItemName, quest);
            }
        }
    }
}
