using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] ItemData itemData;
    [SerializeField] bool isQuest = false;
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
        //// 퀘스트가 아닐경우 퀘스트 바로 완료 처리
        //if (!isQuest)
        //{
        //    QuestManager.Instance.CompleteQuest(itemData);
        //}
        //else
        //{
        //    QuestManager.Instance.StartQuest(itemData);
        //}

        Destroy(gameObject);
        return itemData.ItemPrompt;
    }
}
