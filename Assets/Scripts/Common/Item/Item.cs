using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] ItemData itemData;
    [SerializeField] protected GameObject pressE;
    [SerializeField] protected GameObject outLine;

    public void ShowPressEkeyUI()
    {
        pressE.SetActive(true);
        outLine.SetActive(true);
    }

    // E키를 눌러주세요 비활성화
    public void HidePressEkeyUI()
    {
        pressE.SetActive(false);
        outLine.SetActive(false);
    }

    public string GetItemPrompt()
    {
        return itemData.ItemPrompt;
    }
}
