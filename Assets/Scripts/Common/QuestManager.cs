using System;
using System.Collections.Generic;
using System.Linq;
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

    private HashSet<string> GetDoneSet(string questId)
    {
        if (!doneStepsByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            doneStepsByQuest[questId] = set;
        }
        return set;
    }

    public void ApplyStep(string questId, string stepId)
    {
        if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(stepId))
        {
            Debug.LogWarning($"ApplyStep ignored: invalid ids questId='{questId}', stepId='{stepId}'");
            return;
        }

        var quest = FindQuestById(questId);
        if (quest == null)
        {
            Debug.LogWarning($"ApplyStep ignored: quest not found '{questId}'");
            return;
        }

        // ✅ Step 기반 퀘스트가 아니면: 레거시 모드로 처리
        if (!quest.useSteps)
        {
            // 정책: NotStarted면 시작, "COMPLETE" 같은 stepId면 완료 트리거 가능
            if (GetQuestState(questId) == QuestState.NotStarted)
                StartQuest(questId);

            if (stepId.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase))
                CompleteQuest(quest);

            Debug.Log($"ApplyStep (legacy): quest='{questId}', step='{stepId}', state='{GetQuestState(questId)}'");
            return;
        }

        // 1) quest state 보장
        var state = GetQuestState(questId);
        if (state == QuestState.Completed)
        {
            Debug.Log($"ApplyStep ignored: quest already completed '{questId}'");
            return;
        }
        if (state == QuestState.NotStarted)
        {
            // 대화에서 step이 들어오면 자동 시작하는 정책(추천)
            StartQuest(questId);
        }
        if (GetQuestState(questId) != QuestState.InProgress)
        {
            Debug.LogWarning($"ApplyStep ignored: quest not in progress '{questId}', state='{GetQuestState(questId)}'");
            return;
        }

        // 2) step 정의 찾기
        var step = quest.steps?.FirstOrDefault(s => s != null && s.stepId == stepId);
        if (step == null)
        {
            Debug.LogWarning($"ApplyStep ignored: step not found quest='{questId}', step='{stepId}'");
            return;
        }

        // 3) 선행조건 + 중복 검사
        var done = GetDoneSet(questId);

        if (done.Contains(stepId))
        {
            Debug.Log($"ApplyStep ignored: step already done quest='{questId}', step='{stepId}'");
            return;
        }

        if (step.requires != null)
        {
            foreach (var req in step.requires)
            {
                if (!done.Contains(req))
                {
                    Debug.LogWarning($"ApplyStep blocked: missing requirement '{req}' for quest='{questId}', step='{stepId}'");
                    return;
                }
            }
        }

        // 4) 완료 처리
        done.Add(stepId);
        Debug.Log($"ApplyStep OK: quest='{questId}', step='{stepId}', doneCount={done.Count}");

        // 5) 완료 판정
        bool isCompleted;

        // completeStepId가 있으면 그것을 완료로 본다
        if (!string.IsNullOrWhiteSpace(quest.completeStepId))
        {
            isCompleted = done.Contains(quest.completeStepId);
        }
        else
        {
            // 없으면 전체 steps 완료 시 완료
            int total = quest.steps?.Count(s => s != null && !string.IsNullOrWhiteSpace(s.stepId)) ?? 0;
            isCompleted = (total > 0) && (done.Count >= total);
        }

        if (isCompleted)
        {
            CompleteQuest(quest);
        }
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

        foreach (var quest in npc.NpcData.relatedQuests)
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
                return quest.questOnComplete;
        }

        return "";
    }
    #endregion
}
