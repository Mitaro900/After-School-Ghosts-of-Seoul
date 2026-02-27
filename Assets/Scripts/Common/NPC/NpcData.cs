using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcData", menuName = "ScriptableObjects/NpcData", order = 1)]
public class NpcData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("고유 ID. 바뀌지 않게! 예: KIM_YEONWOO")]
    public string npcId;

    public string displayName;

    public Sprite npcProfile;

    public Sprite Neutral;
    public Sprite Happy;
    public Sprite Angry;
    public Sprite Sad;

    [Header("AI Prompt")]
    [TextArea] public string npcPrompt;

    [Header("Quest Links")]
    [Tooltip("이 NPC와 연관된 퀘스트 목록(순서 중요: 선행 → 후행)")]
    public List<QuestData> relatedQuests = new();
}
