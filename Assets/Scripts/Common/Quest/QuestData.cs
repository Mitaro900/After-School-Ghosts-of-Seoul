using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public enum StepOnCompleteAction
{
    None,
    CompleteQuest,  // 이 step 완료 시 퀘스트 완료 처리
    SetFlag         // 플래그 1개 세팅
}

[System.Serializable]
public class StepCondition
{
    public ItemData requiredItem;       // 있으면 인벤토리 보유 필요
    public NpcData requiredTalkNpc;     // 있으면 해당 NPC와 '대화 충족' 플래그 필요
    public List<string> requiredFlags;  // 있으면 모두 충족해야 함
}

[System.Serializable]
public class StepEffect
{
    public StepOnCompleteAction action = StepOnCompleteAction.None;
    public string flagToSet;            // action==SetFlag일 때 세팅할 플래그
}

[System.Serializable]
public class QuestStepData
{
    [Header("Identity")]
    public string stepId;
    public string label;
    public NpcData stepNpcData;

    [Header("Unlock 조건(AND)")]
    public List<string> requires;   // 선행 step들

    [Header("완료 조건(선택)")]
    public StepCondition completeCondition;

    [Header("완료 효과(선택)")]
    public StepEffect onComplete;

    [Header("Step별 프롬프트(선택)")]
    [TextArea(6, 20)]
    public string stepPromptOverride; // 이 step에서만 적용되는 추가 프롬프트

    [Header("선택")]
    public int priority = 0; // 같은 NPC에서 여러 step 열릴 때 우선순위
}

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;
    public string questName;

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