using UnityEngine;

[CreateAssetMenu(fileName = "NpcData", menuName = "ScriptableObjects/NpcData", order = 1)]
public class NpcData : ScriptableObject
{
    [SerializeField]
    private string npcName;

    [SerializeField]
    [TextArea]
    private string npcPrompt;
}
