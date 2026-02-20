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
    private Dictionary<string, string> questStep = new();           // questId -> currentStepId (단계형)
    private HashSet<string> doneSteps = new();                      // 완료한 step 기록 (재수행 방지)


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
        if (!questDatabase.ContainsKey(questId))
            return new StepResult { type = StepResultType.Fail_NotFound, message = "Quest not found" };

        // 2) 상태 확인
        if (GetQuestState(questId) != QuestState.InProgress)
            return new StepResult { type = StepResultType.Fail_NotInProgress, message = "Quest not in progress" };

        // 3) 이미 완료한 step인지
        var key = $"{questId}:{stepId}";
        if (doneSteps.Contains(key))
            return new StepResult { type = StepResultType.Fail_AlreadyDone, message = "Step already done" };

        // 4) 현재 step과 일치하는지(단계형일 때)
        if (questStep.TryGetValue(questId, out var cur) && !string.IsNullOrEmpty(cur) && cur != stepId)
            return new StepResult { type = StepResultType.Fail_WrongStep, message = $"Current step is {cur}" };

        // 5) 조건 검사(예: 증거 아이템 보유, npcId 일치 등)
        // 예: ShowEvidence step이면 InventoryManager.HasItem(...) 체크 등
        // if (!InventoryManager.Instance.HasItem("EVIDENCE_001")) return Fail_ConditionNotMet;

        // 6) 성공 처리: step 완료 기록 + 다음 step으로 전이(혹은 완료)
        doneSteps.Add(key);

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
