using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public enum StepTriggerType
{
    None,
    KeywordAny,
    KeywordAll,
    Regex
}

[System.Serializable]
public class StepTrigger
{
    public StepTriggerType type = StepTriggerType.KeywordAny;

    [Tooltip("KeywordAny/All에서 사용. 동의어까지 넣는 것을 추천.")]
    public List<string> keywords = new();

    [Tooltip("Regex에서 사용. 예: 체육관.*뒤")]
    public string regex;

    public bool caseInsensitive = true;
}

[System.Serializable]
public class StepCondition
{
    public ItemData requiredItem;      // 있으면 인벤토리 보유 필요
    public NpcData requiredTalkNpc;    // 있으면 해당 NPC와 대화 기록 필요(선택)
    public List<string> requiredFlags; // 있으면 모두 충족 필요(선택)
}

public enum StepOnCompleteAction
{
    None,
    SetFlag,
    CompleteQuest
}

[System.Serializable]
public class StepEffect
{
    public StepOnCompleteAction action = StepOnCompleteAction.None;
    public string flagToSet;
}

[System.Serializable]
public class QuestStepData
{
    [Header("Identity")]
    public string stepId;       // 고유 ID
    public string label;        // step 이름
    public NpcData stepNpcData; // NPC 프롬프트를 특정 NPC에 적용하고 싶을 때 사용. 없으면 모든 NPC에 적용.

    [Header("Unlock 조건")]
    public List<string> requires;   // AND
    public List<string> anyOf;      // (선택) K-of-N
    public int anyOfMin = 1;        // anyOfMin이 0이면 anyOf는 선택 사항이 됨

    [Header("Trigger(플레이어 입력으로 자동 진행)")]
    public StepTrigger trigger;

    [Header("완료 조건(선택)")]
    public StepCondition completeCondition;

    [Header("완료 효과(선택)")]
    public StepEffect onComplete;

    [Header("Step별 프롬프트(선택)")]
    [TextArea(6, 20)]
    public string stepPromptOverride;

    [Header("선택")]
    public int priority = 0;    // 여러 step이 동시에 unlock/complete 조건을 만족할 때 우선적으로 진행할 step 선택. 높을수록 우선
}

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;      // 고유 ID
    public string questName;    // 퀘스트 이름

    [Header("NPC 프롬프트")]
    [TextArea] public string questNotStarted;
    [TextArea] public string questInProgress;
    [TextArea] public string questOnComplete;

    [Header("완료 조건(레거시)")]
    public ItemData requiredItem;
    public NpcData targetNpcData;

    [Header("보상")]
    public bool givesReward;
    public List<ItemData> rewardItems;

    [Header("Step 기반 진행(옵션)")]
    public bool useSteps;
    public List<QuestStepData> steps = new();
    public string completeStepId; // 비우면 "steps 전부 완료"가 완료 조건
}