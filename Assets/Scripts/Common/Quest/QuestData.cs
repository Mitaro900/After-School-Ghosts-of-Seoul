using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/QuestData")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;          // 고유 ID (CHAPTER1-1 이런식으로)
    public string questName;        // 퀘스트 이름 (UI에 표시용)


    [Header("NPC 대화 - 상태별")]
    [TextArea] public string questOnStart;      // 퀘스트 처음 받을 때
    [TextArea] public string questInProgress;   // 진행 중일 때 말걸면
    [TextArea] public string questOnComplete;   // 완료 조건 충족 시
    [TextArea] public string questAfterComplete;// 완료 이후 다시 말걸면


    [Header("완료 조건")]
    public List<ItemData> requiredItems; // 완료 조건 아이템


    [Header("완료 보상")]
    public bool givesReward = false;    // 보상 아이템을 줄건지 선택
    public List<ItemData> rewardItems;  // 보상 아이템 목록
}