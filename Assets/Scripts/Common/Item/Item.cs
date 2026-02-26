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

    // 획득시 플레이어가 퀘스트 관련 대사 (ex 이 아이템을 누군가에게 전해주자)
    public string GetItemPrompt()
    {
        InventoryData.Instance.AddItem(itemData);    // 아이템 슬롯에 등록

        Destroy(gameObject);
        return itemData.ItemQuestPrompt;
    }
}
