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
    // 인스펙터에 등록하는 전체 퀘스트 목록
    [Header("퀘스트 목록")]
    [SerializeField] private List<QuestData> allQuests;

    // questId → QuestData 빠른 검색용
    private Dictionary<string, QuestData> questDatabase = new();

    // questId → 현재 퀘스트 상태 저장
    private Dictionary<string, QuestState> questStates = new();

    // 현재 진행 중인 퀘스트 목록
    private List<QuestData> activeQuests = new();

    #region 초기화

    // 전체 퀘스트를 Dictionary에 등록
    protected override void Awake()
    {
        base.Awake();

        foreach (var quest in allQuests)
        {
            if (!questDatabase.ContainsKey(quest.questId))
                questDatabase.Add(quest.questId, quest);
        }
    }

    #endregion


    #region 상태 조회

    // 퀘스트 상태 반환 (없으면 NotStarted로 초기화)
    public QuestState GetQuestState(string questId)
    {
        if (!questStates.ContainsKey(questId))
            questStates[questId] = QuestState.NotStarted;

        return questStates[questId];
    }

    #endregion


    #region 퀘스트 찾기

    // questId로 등록된 퀘스트가 있는지 검색
    private QuestData FindQuestById(string questId)
    {
        // 등록된 퀘스트가 있다면 퀘스트 전송
        if (questDatabase.TryGetValue(questId, out var quest))
            return quest;   

        return null;
    }

    #endregion


    #region 퀘스트 시작

    // 퀘스트 시작 처리
    public void StartQuest(string questId)
    {
        // 등록된 퀘스트가 있는지 검색하고 저장
        var quest = FindQuestById(questId);
        if (quest == null) return;

        // 퀘스트를 퀘스트 중으로 바꿈
        questStates[questId] = QuestState.InProgress;

        // 이미 진행 중 목록에 들어있는 퀘스트면 또 추가하지 않음
        if (!activeQuests.Contains(quest))
            activeQuests.Add(quest);

        Debug.Log($"퀘스트 시작: {quest.questName}");
    }

    #endregion


    #region NPC 아이템 조건 

    // NPC와 대화했을 때 완료 가능한지 검사
    public bool CheckQuestComplete(string questId)
    {
        // 등록된 퀘스트가 있는지 검색하고 저장
        var quest = FindQuestById(questId);
        if (quest == null)
            return false;

        // 퀘스트가 진행 중이 아니면 실패
        if (GetQuestState(questId) != QuestState.InProgress)
            return false;

        // 아이템 조건이 있다면 검사
        if (quest.requiredItem != null)
        {
            if (!InventoryManager.Instance.HasItem(quest.requiredItem))
                return false;
        }

        // 조건 만족 시 완료 처리
        CompleteQuest(quest);
        return true;
    }

    #endregion


    #region NPC 대화 조건 (특정 NPC 방문형)

    // 특정 NPC와 대화 시 완료되는 퀘스트 처리
    public void CheckTalkCondition(NPC npc)
    {
        foreach (var quest in new List<QuestData>(activeQuests))
        {
            if (quest.targetNpc == npc)
            {
                CompleteQuest(quest);
            }
        }
    }

    #endregion


    #region 완료 처리

    // 실제 퀘스트 완료 처리
    public void CompleteQuest(QuestData quest)
    {
        questStates[quest.questId] = QuestState.Completed;
        activeQuests.Remove(quest);

        // 보상 지급
        if (quest.givesReward)
        {
            foreach (var item in quest.rewardItems)
                InventoryManager.Instance.AddItem(item);
        }

        Debug.Log($"퀘스트 완료: {quest.questName}");
    }

    #endregion


    #region NPC용 인터페이스

    // NPC가 현재 줄 수 있는 퀘스트 반환
    public QuestData GetAvailableQuest(NPC npc)
    {
        if (npc == null) return null;

        foreach (var quest in npc.NpcData.RelatedQuests)
        {
            var state = GetQuestState(quest.questId);

            // 아직 완료 안 된 첫 번째 퀘스트 반환
            if (state != QuestState.Completed)
                return quest;
        }

        return null;
    }
    #endregion


    #region 유틸

    // 현재 진행 중인 퀘스트 목록 반환
    public List<QuestData> GetActiveQuests()
    {
        return activeQuests;
    }

    // NPC 대사 분기용
    public string GetQuestNpcPrompt(string questId)
    {
        var quest = FindQuestById(questId);
        if (quest == null) return "";

        switch (GetQuestState(questId))
        {
            case QuestState.NotStarted:
                return quest.questNotStarted;

            case QuestState.InProgress:
                return quest.questInProgress;

            case QuestState.Completed:
                return quest.questAfterComplete;
        }

        return "";
    }

    #endregion













    public enum StepResultType
    {
        Success,
        Fail_NotFound,
        Fail_NotInProgress,
        Fail_WrongStep,
        Fail_AlreadyDone,
        Fail_ConditionNotMet
    }

    public struct StepResult
    {
        public StepResultType type;
        public string message;
    }

    public StepResult TryPerformStep(string questId, string stepId, string npcId)
    {
        // 1) 퀘스트 존재
        //if (!questDatabase.ContainsKey(questId))
        //    return new StepResult { type = StepResultType.Fail_NotFound, message = "Quest not found" };

        // 2) 상태 확인
        if (GetQuestState(questId) != QuestState.InProgress)
            return new StepResult { type = StepResultType.Fail_NotInProgress, message = "Quest not in progress" };

        // 3) 이미 완료한 step인지
        //var key = $"{questId}:{stepId}";
        //if (doneSteps.Contains(key))
        //    return new StepResult { type = StepResultType.Fail_AlreadyDone, message = "Step already done" };

        // 4) 현재 step과 일치하는지(단계형일 때)
        //if (questStep.TryGetValue(questId, out var cur) && !string.IsNullOrEmpty(cur) && cur != stepId)
        //    return new StepResult { type = StepResultType.Fail_WrongStep, message = $"Current step is {cur}" };

        // 5) 조건 검사(예: 증거 아이템 보유, npcId 일치 등)
        // 예: ShowEvidence step이면 InventoryManager.HasItem(...) 체크 등
        // if (!InventoryManager.Instance.HasItem("EVIDENCE_001")) return Fail_ConditionNotMet;

        // 6) 성공 처리: step 완료 기록 + 다음 step으로 전이(혹은 완료)
        //doneSteps.Add(key);

        // 예: 다음 step으로 이동
        // questStep[questId] = "NEXT_STEP_ID";
        // 또는 마지막이면 CompleteQuest(questId);

        return new StepResult { type = StepResultType.Success, message = "OK" };
    }

    public struct StepOffer
    {
        public string questId;
        public string stepId;
        public string label; // UI 표시용(또는 prompt용)
    }

    public List<StepOffer> GetAvailableStepsForNpc(string npcId)
    {
        var result = new List<StepOffer>();

        // TODO: npcId와 연관된 퀘스트/단계를 찾아 조건을 만족하면 추가
        // 예:
        // if (GetQuestState("Q_PRESSURE_GUNWOO") == InProgress && Inventory.HasItem("EVIDENCE_001") && !doneSteps.Contains("Q_PRESSURE_GUNWOO:SHOW_EVIDENCE"))
        // result.Add(new StepOffer{ questId="Q_PRESSURE_GUNWOO", stepId="SHOW_EVIDENCE", label="증거 자료를 보여준다" });

        return result;
    }
}
