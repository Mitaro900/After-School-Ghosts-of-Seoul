using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

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

    // questId → 완료한 stepId 집합
    private Dictionary<string, HashSet<string>> doneStepsByQuest = new();

    // questId → 플래그 집합
    private Dictionary<string, HashSet<string>> flagsByQuest = new();

    // questId → (npcId) 대화 충족 플래그
    private Dictionary<string, HashSet<string>> talkedNpcByQuest = new();

    #region Step/Flag 관리
    private HashSet<string> GetFlagSet(string questId)
    {
        if (!flagsByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            flagsByQuest[questId] = set;
        }
        return set;
    }

    private HashSet<string> GetTalkedSet(string questId)
    {
        if (!talkedNpcByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            talkedNpcByQuest[questId] = set;
        }
        return set;
    }

    public void MarkTalked(string questId, NpcData npcData)
    {
        if (string.IsNullOrEmpty(questId) || npcData == null || string.IsNullOrEmpty(npcData.npcId)) return;
        GetTalkedSet(questId).Add(npcData.npcId);
    }

    private HashSet<string> GetDoneSet(string questId)
    {
        if (!doneStepsByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            doneStepsByQuest[questId] = set;
        }
        return set;
    }

    private bool IsStepCompleteConditionMet(string questId, QuestStepData step)
    {
        var cond = step.completeCondition;
        if (cond == null) return true; // 조건 없으면 즉시 완료

        // 아이템 조건
        if (cond.requiredItem != null)
        {
            if (!InventoryManager.Instance.HasItem(cond.requiredItem))
                return false;
        }

        // 특정 NPC와 대화 조건
        if (cond.requiredTalkNpc != null && !string.IsNullOrEmpty(cond.requiredTalkNpc.npcId))
        {
            var talked = GetTalkedSet(questId);
            if (!talked.Contains(cond.requiredTalkNpc.npcId))
                return false;
        }

        // 플래그 조건
        if (cond.requiredFlags != null && cond.requiredFlags.Count > 0)
        {
            var flags = GetFlagSet(questId);
            if (!cond.requiredFlags.TrueForAll(flags.Contains))
                return false;
        }

        return true;
    }

    public void ApplyStep(string questId, string stepId)
    {
        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps) return;

        if (GetQuestState(questId) == QuestState.NotStarted) StartQuest(questId);
        if (GetQuestState(questId) != QuestState.InProgress) return;

        var step = quest.steps.FirstOrDefault(s => s.stepId == stepId);
        if (step == null) return;

        var done = GetDoneSet(questId);
        if (done.Contains(stepId)) return;

        // 선행 조건(해금 조건)
        if (step.requires != null && step.requires.Count > 0)
        {
            if (!step.requires.All(done.Contains)) return;
        }

        // 완료 조건(아이템/대화/플래그)
        if (!IsStepCompleteConditionMet(questId, step))
        {
            // 여기서 “조건 부족” 안내를 UI/로그로 줄 수도 있음
            Debug.Log($"ApplyStep blocked: step completeCondition not met. quest={questId}, step={stepId}");
            return;
        }

        // 완료 처리
        done.Add(stepId);

        // Step 효과 실행
        var eff = step.onComplete;
        if (eff != null)
        {
            if (eff.action == StepOnCompleteAction.SetFlag && !string.IsNullOrEmpty(eff.flagToSet))
                GetFlagSet(questId).Add(eff.flagToSet);

            if (eff.action == StepOnCompleteAction.CompleteQuest)
                CompleteQuest(quest);
        }

        // (선택) QuestData.completeStepId도 지원하고 싶으면:
        if (!string.IsNullOrEmpty(quest.completeStepId) && done.Contains(quest.completeStepId))
            CompleteQuest(quest);
    }

    public Choice[] GetAvailableStepsForNpcAsChoices(string questId, NPC npc)
    {
        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps) return Array.Empty<Choice>();

        var state = GetQuestState(questId);
        if (state == QuestState.Completed) return Array.Empty<Choice>();

        var done = GetDoneSet(questId);

        bool IsUnlocked(QuestStepData s)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.stepId) || string.IsNullOrWhiteSpace(s.label))
                return false;

            // 완료된 step은 제외
            if (done.Contains(s.stepId)) return false;

            // stepNpc가 지정되어 있으면 해당 NPC에서만 노출
            if (s.stepNpcData != null && npc != null && npc.NpcData != s.stepNpcData) return false;

            // 선행조건
            if (s.requires == null || s.requires.Count == 0) return true;
            return s.requires.All(req => done.Contains(req));
        }

        return quest.steps
            .Where(IsUnlocked)
            .Select(s => new Choice { id = $"{questId}:{s.stepId}", label = s.label })
            .ToArray();
    }

    public QuestStepData GetTopAvailableStep(string questId, NPC npc)
    {
        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps) return null;

        var done = GetDoneSet(questId);

        bool IsUnlocked(QuestStepData s)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.stepId)) return false;
            if (done.Contains(s.stepId)) return false;
            if (s.stepNpcData != null && npc != null && npc.NpcData != s.stepNpcData) return false;
            if (s.requires == null || s.requires.Count == 0) return true;
            return s.requires.All(done.Contains);
        }

        return quest.steps
            .Where(IsUnlocked)
            .OrderByDescending(s => s.priority)
            .FirstOrDefault();
    }
    #endregion

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
            if (quest.targetNpcData != null && npc != null && npc.NpcData == quest.targetNpcData)
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

        QuestData lastQuest = null;

        foreach (var quest in npc.NpcData.relatedQuests)
        {
            lastQuest = quest;

            var state = GetQuestState(quest.questId);

            // 아직 완료 안 된 첫 번째 퀘스트 반환
            if (state != QuestState.Completed)
                return quest;
        }

        // 전부 완료됐으면 마지막 퀘스트 계속 반환
        return lastQuest;
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
                return quest.questOnComplete;
        }

        return "";
    }
    #endregion
}
