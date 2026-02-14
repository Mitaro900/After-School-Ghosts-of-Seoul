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
        Destroy(gameObject); // 임시로 제거 (나중에 아이템 관련 중앙 관리자를 통해 오브젝트를 열거나 NPC에게 관련정보주기)
        return itemData.ItemPrompt;
    }
}
