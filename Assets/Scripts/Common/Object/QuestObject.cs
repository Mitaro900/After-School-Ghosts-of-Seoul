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
        QuestState state = QuestManager.Instance.GetQuestState(questData.questId);

        // 퀘스트가 진행 중이거나 완료 되었을 경우
        if (state == QuestState.InProgress || state == QuestState.Completed)
        {
            bool completed = QuestManager.Instance.CheckQuestComplete(questData.questId);   // 현재 이 코드는 퀘스트 중이여야만 true를 보내기에 npc에게 말을 걸면 안됨

            // 조건을 만족했다면 완료 대사 아니라면 진행 중 대사 반환
            if (completed)
            {
                InventoryData.Instance.RemoveItem(questData.requiredItem);   // 해당 완료 아이템 제거
                Destroy(gameObject);
                return questCompleted;
            }
            else
            {
                return questInProgress;
            }
        }

        // 아직 퀘스트를 시작하지 않은 상태라면
        return questNotStarted;
    }
}
