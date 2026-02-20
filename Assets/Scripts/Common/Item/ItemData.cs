using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField][TextArea] private string itemPrompt;


    public string ItemName => itemName;
    public string ItemPrompt => itemPrompt;
}
