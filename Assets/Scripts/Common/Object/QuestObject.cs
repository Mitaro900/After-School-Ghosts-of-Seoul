using NUnit.Framework.Interfaces;
using System.Xml.Linq;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] protected QuestData questData;
    [SerializeField] protected GameObject pressE;

    [Header("Quest Chat")]
    [SerializeField] protected string questNotStarted = "문이 잠겨있다. 누군가가 알고있을지도";
    [SerializeField] protected string questInProgress = "이 문을 열려면 아이템을 찾아야 될 것 같아";
    [SerializeField] protected string questCompleted = "문이 열렸다.";
    private string currentChat;


    // E키를 눌러주세요 활성화
    public void ShowPressEkeyUI()
    {
        pressE.SetActive(true);
    }

    // E키를 눌러주세요 비활성화
    public void HidePressEkeyUI()
    {
        pressE.SetActive(false);
    }

    public virtual string GetItemPrompt()
    {
        // 퀘스트 전일시
        if (QuestManager.Instance.GetQuestState(questData.questId) == QuestState.NotStarted)
        {
            // 아이템 조건 검사후 퀘스트 완료시
            if (QuestManager.Instance.CheckQuestComplete(questData.questId))    // 이 부분은 생각 더 해보셈 바빠서 이것만 적어놈
            {
                currentChat = questCompleted;
                Destroy(gameObject);
            }
            else
            {
                currentChat = questNotStarted;
            }
        }

        // 퀘스트 중일시
        if (QuestManager.Instance.GetQuestState(questData.questId) == QuestState.InProgress)
        {
            // 아이템 조건 검사후 퀘스트 완료시
            if (QuestManager.Instance.CheckQuestComplete(questData.questId))
            {
                currentChat = questCompleted;
                Destroy(gameObject);
            }
            else // 조건 불일치시
            {
                Debug.Log("다른 퀘스트임");
                currentChat = questInProgress;
            }
        }

        return currentChat;
    }
}
