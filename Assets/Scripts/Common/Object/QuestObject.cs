using NUnit.Framework.Interfaces;
using System.Xml.Linq;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] protected ItemData questItem;
    [SerializeField] protected GameObject pressE;

    [Header("Quest Chat")]
    [SerializeField] protected string questNotStarted = "문을 열려면 아이템이 필요하겠어";
    [SerializeField] protected string questInProgress = "이 문을 열려면 먼저 NPC의 부탁을 들어줘야 할 것 같아";
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
        //// 퀘스트 전일시
        //if (QuestManager.Instance.GetQuestState(questItem) == QuestState.NotStarted)
        //{
        //    currentChat = questNotStarted;
        //}

        //// 퀘스트 중일시
        //if (QuestManager.Instance.GetQuestState(questItem) == QuestState.Completed)
        //{
        //    currentChat = questInProgress;
        //}

        //// 퀘스트 완료시
        //if (QuestManager.Instance.GetQuestState(questItem) == QuestState.Completed)
        //{
        //    currentChat = questCompleted;
        //    Destroy(gameObject);
        //}
        return currentChat;
    }
}
