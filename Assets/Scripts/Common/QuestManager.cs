using Singleton.Component;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed
}

public enum ApplyStepResult
{
    Ok,
    Invalid,
    Locked,
    AlreadyDone,
    MissingItem,
    MissingTalk,
    MissingFlag
}

public class QuestManager : SingletonComponent<QuestManager>
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

    // questId → 특정 step에서 막힌 횟수
    private Dictionary<string, int> stuckCounterByQuest = new();

    // 퀘스트 변경 이벤트
    public event System.Action<string> OnQuestUpdated;

    #region Singleton
    protected override void AwakeInstance()
    {

    }

    protected override bool InitInstance()
    {
        foreach (var quest in allQuests)
        {
            if (!questDatabase.ContainsKey(quest.questId))
                questDatabase.Add(quest.questId, quest);
        }

        return true;
    }

    protected override void ReleaseInstance()
    {
        questDatabase.Clear();
    }
    #endregion

    #region 힌트 모드
    // 특정 퀘스트의 특정 step에서 막힌 횟수 반환
    private int GetStuck(string questId)
    {
        return stuckCounterByQuest.TryGetValue(questId, out var v) ? v : 0;
    }

    // 특정 퀘스트의 막힌 횟수 설정
    private void SetStuck(string questId, int v)
    {
        stuckCounterByQuest[questId] = Mathf.Max(0, v);
    }

    // UI용: 특정 퀘스트의 막힌 횟수 반환
    public int GetStuckCountForUi(string questId) => GetStuck(questId);
    #endregion

    #region Step/Flag 관리
    // questId로 플래그 집합 반환 (없으면 새 집합 만들어서 반환)
    private HashSet<string> GetFlagSet(string questId)
    {
        if (!flagsByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            flagsByQuest[questId] = set;
        }
        return set;
    }

    // questId로 대화한 NPC 집합 반환 (없으면 새 집합 만들어서 반환)
    private HashSet<string> GetTalkedSet(string questId)
    {
        if (!talkedNpcByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            talkedNpcByQuest[questId] = set;
        }
        return set;
    }

    // 특정 NPC와 대화했다고 표시
    public void MarkTalked(string questId, NpcData npcData)
    {
        if (string.IsNullOrEmpty(questId) || npcData == null || string.IsNullOrEmpty(npcData.npcId)) return;
        GetTalkedSet(questId).Add(npcData.npcId);
    }

    // questId로 step 완료 상태 집합 반환 (없으면 새 집합 만들어서 반환)
    private HashSet<string> GetDoneSet(string questId)
    {
        if (!doneStepsByQuest.TryGetValue(questId, out var set))
        {
            set = new HashSet<string>();
            doneStepsByQuest[questId] = set;
        }
        return set;
    }

    // step의 완료 조건 검사, 충족하면 Ok 반환, 아니면 부족한 조건에 따른 결과 반환
    private ApplyStepResult CheckCompleteCondition(string questId, QuestStepData step)
    {
        var cond = step.completeCondition;
        if (cond == null) return ApplyStepResult.Ok;

        if (cond.requiredItem != null)
        {
            if (!InventoryData.Instance.HasItem(cond.requiredItem))
                return ApplyStepResult.MissingItem;
        }

        if (cond.requiredTalkNpc != null && !string.IsNullOrWhiteSpace(cond.requiredTalkNpc.npcId))
        {
            if (!GetTalkedSet(questId).Contains(cond.requiredTalkNpc.npcId))
                return ApplyStepResult.MissingTalk;
        }

        if (cond.requiredFlags != null && cond.requiredFlags.Count > 0)
        {
            var flags = GetFlagSet(questId);
            if (!cond.requiredFlags.All(flags.Contains))
                return ApplyStepResult.MissingFlag;
        }

        return ApplyStepResult.Ok;
    }

    // stepId로 step 완료 시도 (조건 검사 후 완료 처리)
    public ApplyStepResult ApplyStep(string questId, string stepId)
    {
        if (string.IsNullOrWhiteSpace(questId) || string.IsNullOrWhiteSpace(stepId))
            return ApplyStepResult.Invalid;

        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps)
            return ApplyStepResult.Invalid;

        if (GetQuestState(questId) == QuestState.Completed)
            return ApplyStepResult.Invalid;

        if (GetQuestState(questId) == QuestState.NotStarted)
            StartQuest(questId);

        if (GetQuestState(questId) != QuestState.InProgress)
            return ApplyStepResult.Invalid;

        var step = quest.steps.FirstOrDefault(s => s != null && s.stepId == stepId);
        if (step == null)
            return ApplyStepResult.Invalid;

        var done = GetDoneSet(questId);
        if (done.Contains(stepId))
            return ApplyStepResult.AlreadyDone;

        // 해금 조건
        if (!IsUnlockedStep(questId, null, step)) // npc 조건은 이미 trigger 단계에서 걸러지는 게 일반적
            return ApplyStepResult.Locked;

        // 완료 조건
        var condResult = CheckCompleteCondition(questId, step);
        if (condResult != ApplyStepResult.Ok)
            return condResult;

        // 완료 처리
        CompleteStep(questId, step);

        // 효과
        if (step.onComplete != null)
        {
            if (step.onComplete.action == StepOnCompleteAction.SetFlag && !string.IsNullOrWhiteSpace(step.onComplete.flagToSet))
                GetFlagSet(questId).Add(step.onComplete.flagToSet);

            if (step.onComplete.action == StepOnCompleteAction.CompleteQuest)
                CompleteQuest(quest);
        }

        return ApplyStepResult.Ok;
    }

    // 플레이어 입력과 일치하는 trigger 가진 step이 있으면 적용 시도
    public ApplyStepResult TryApplyStepFromInput(string questId, NPC npc, string playerInput, out string appliedStepId)
    {
        TryAutoApplySteps(questId, npc);

        appliedStepId = null;

        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps) return ApplyStepResult.Invalid;

        if (GetQuestState(questId) == QuestState.Completed) return ApplyStepResult.Invalid;

        if (GetQuestState(questId) == QuestState.NotStarted)
            StartQuest(questId);

        var candidates = quest.steps
            .Where(s => IsUnlockedStep(questId, npc, s))
            .Where(s => TriggerMatches(s, playerInput))
            .OrderByDescending(s => s.priority)
            .ToList();

        if (candidates.Count == 0)
        {
            // 진행 못했으면 stuck +1
            SetStuck(questId, GetStuck(questId) + 1);
            return ApplyStepResult.Locked;
        }

        var chosen = candidates[0];
        var result = ApplyStep(questId, chosen.stepId);

        if (result == ApplyStepResult.Ok)
        {
            appliedStepId = chosen.stepId;
            // 진행됐으면 stuck 리셋
            SetStuck(questId, 0);
        }
        else
        {
            // 조건 부족/잠김이면 stuck 증가(선택)
            SetStuck(questId, GetStuck(questId) + 1);
        }

        return result;
    }

    // trigger 없이 자동으로 적용 가능한 step들 먼저 적용 (조건 충족하는 것들)
    private bool TryAutoApplySteps(string questId, NPC npc)
    {
        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps) return false;
        if (GetQuestState(questId) == QuestState.Completed) return false;

        var done = GetDoneSet(questId);
        bool appliedAny = false;

        foreach (var s in quest.steps)
        {
            if (s == null) continue;
            if (done.Contains(s.stepId)) continue;

            // trigger가 없거나 None이면 자동 적용 대상으로 취급
            bool isAuto = (s.trigger == null || s.trigger.type == StepTriggerType.None);
            if (!isAuto) continue;

            if (!IsUnlockedStep(questId, npc, s)) continue;

            var r = ApplyStep(questId, s.stepId);
            if (r == ApplyStepResult.Ok) appliedAny = true;
        }

        return appliedAny;
    }

    // 현재 NPC와 조건에 맞는 unlock된 step 중 우선순위가 가장 높은 step 반환 (없으면 null)
    public QuestStepData GetTopAvailableStep(string questId, NPC npc)
    {
        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps) return null;

        var state = GetQuestState(questId);
        if (state == QuestState.Completed) return null;

        // NotStarted여도 "대화하면 시작" 정책이면 여기서 StartQuest해도 됨(선택)
        // if (state == QuestState.NotStarted) StartQuest(questId);

        // 현재 NPC에서 unlock된 step 중 priority가 가장 높은 1개
        return quest.steps
            .Where(s => IsUnlockedStep(questId, npc, s))
            .OrderByDescending(s => s.priority)
            .FirstOrDefault();
    }

    // step이 잠겨있는지 검사 (NPC 조건, requires/anyOf 조건)
    private bool IsUnlockedStep(string questId, NPC npc, QuestStepData s)
    {
        if (s == null || string.IsNullOrWhiteSpace(s.stepId)) return false;

        var done = GetDoneSet(questId);
        if (done.Contains(s.stepId)) return false;

        // NPC 제한
        if (s.stepNpcData != null && npc != null && npc.NpcData != s.stepNpcData) return false;

        // AND requires
        bool andOk = (s.requires == null || s.requires.Count == 0) || s.requires.All(done.Contains);

        // K-of-N anyOf
        bool anyOk = true;
        if (s.anyOf != null && s.anyOf.Count > 0)
        {
            int count = s.anyOf.Count(done.Contains);
            int min = Mathf.Max(1, s.anyOfMin);
            anyOk = count >= min;
        }

        return andOk && anyOk;
    }

    // 플레이어 입력이 step의 trigger와 일치하는지 검사
    private bool TriggerMatches(QuestStepData s, string input)
    {
        if (s?.trigger == null) return false;
        if (s.trigger.type == StepTriggerType.None) return false;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var t = s.trigger;

        switch (t.type)
        {
            case StepTriggerType.KeywordAny:
                {
                    if (t.keywords == null || t.keywords.Count == 0) return false;
                    var src = t.caseInsensitive ? input.ToLowerInvariant() : input;
                    return t.keywords.Any(k =>
                    {
                        if (string.IsNullOrWhiteSpace(k)) return false;
                        var kk = t.caseInsensitive ? k.ToLowerInvariant() : k;
                        return src.Contains(kk);
                    });
                }

            case StepTriggerType.KeywordAll:
                {
                    if (t.keywords == null || t.keywords.Count == 0) return false;
                    var src = t.caseInsensitive ? input.ToLowerInvariant() : input;
                    return t.keywords.All(k =>
                    {
                        if (string.IsNullOrWhiteSpace(k)) return false;
                        var kk = t.caseInsensitive ? k.ToLowerInvariant() : k;
                        return src.Contains(kk);
                    });
                }

            case StepTriggerType.Regex:
                {
                    if (string.IsNullOrWhiteSpace(t.regex)) return false;
                    try
                    {
                        var options = t.caseInsensitive ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : 0;
                        return System.Text.RegularExpressions.Regex.IsMatch(input, t.regex, options);
                    }
                    catch
                    {
                        return false;
                    }
                }
        }

        return false;
    }

    // stepId로 step label 반환 (없으면 stepId 반환)
    public string GetStepLabel(string questId, string stepId)
    {
        var quest = FindQuestById(questId);
        var step = quest?.steps?.Find(s => s != null && s.stepId == stepId);
        return step != null ? step.label : stepId;
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
        {
            activeQuests.Add(quest);
            OnQuestUpdated?.Invoke(questId);
        }

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
            if (!InventoryData.Instance.HasItem(quest.requiredItem))
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
                InventoryData.Instance.AddItem(item);
        }

        OnQuestUpdated?.Invoke(quest.questId);

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

    public QuestStepData GetTopAvailableStepForLog(string questId)
    {
        var quest = FindQuestById(questId);
        if (quest == null || !quest.useSteps)
            return null;

        if (GetQuestState(questId) != QuestState.InProgress)
            return null;

        return quest.steps
            .Where(s => IsUnlockedStep(questId, null, s))
            .OrderByDescending(s => s.priority)
            .FirstOrDefault();
    }

    public List<string> GetDoneSteps(string questId)
    {
        return GetDoneSet(questId).ToList();
    }

    private void CompleteStep(string questId, QuestStepData step)
    {
        var done = GetDoneSet(questId);

        if (done.Contains(step.stepId))
            return;

        done.Add(step.stepId);

        OnQuestUpdated?.Invoke(questId);
    }
}
