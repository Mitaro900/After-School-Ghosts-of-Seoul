using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class QuestStepData
{
    public string stepId;           // "ASK_DETAIL" 같은 고유 id
    public string label;            // 선택지 문구
    public NpcData stepNpcData;             // 이 step이 노출되는 NPC (없으면 누구나 가능)
    public List<string> requires;   // 선행 stepId들 (없으면 바로 가능)

    [TextArea] public string note;  // (선택) 디버그/설명용
}

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;
    public string questName;

    [Header("NPC 대사")]
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