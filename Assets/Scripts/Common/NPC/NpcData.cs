using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcData", menuName = "ScriptableObjects/NpcData", order = 1)]
public class NpcData : ScriptableObject
{
    [SerializeField] private string npcName;
    [SerializeField] private List<QuestData> relatedQuests;
    [SerializeField] [TextArea] private string npcPrompt;

    public string NpcName => npcName;
    public List<QuestData> RelatedQuests => relatedQuests;
    public string NpcPrompt => npcPrompt;
}
