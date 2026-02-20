using UnityEngine;

[CreateAssetMenu(fileName = "NpcData", menuName = "ScriptableObjects/NpcData", order = 1)]
public class NpcData : ScriptableObject
{
    [SerializeField] private string npcName;
    [SerializeField] private QuestData relatedQuest;
    [SerializeField] [TextArea] private string npcPrompt;

    public string NpcName => npcName;
    public QuestData RelatedQuest => relatedQuest;
    public string NpcPrompt => npcPrompt;
}
