using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItmeData")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemImage;
    [SerializeField][TextArea] private string itemQuestPrompt;
    [SerializeField][TextArea] private string itemSlotPrompt;


    public string ItemName => itemName;
    public Sprite ItemImage => itemImage;
    public string ItemQuestPrompt => itemQuestPrompt;
    public string ItemSlotPrompt => itemSlotPrompt;
}
