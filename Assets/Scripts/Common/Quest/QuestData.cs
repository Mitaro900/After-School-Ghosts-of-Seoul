using UnityEngine;
using System.Collections.Generic;

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

    [Header("완료 조건")]
    public ItemData requiredItem;
    public NPC targetNpc;

    [Header("보상")]
    public bool givesReward;
    public List<ItemData> rewardItems;
}