using UnityEngine;

public class NPC : BaseChat
{
    [Header("NPC Info")]
    [SerializeField] protected NpcData npcData;
    public NpcData NpcData => npcData;
    [SerializeField] protected GameObject pressE;


    // E키를 눌러주세요 활성화
    public virtual void ShowPressEkeyUI()
    {
        pressE.SetActive(true);
    }

    // E키를 눌러주세요 비활성화
    public virtual void HidePressEkeyUI()
    {
        pressE.SetActive(false);
    }

    // NPC 대화시 나올것들 (일단 대화, 퀘스트 대화 나뉘게)
    public virtual void OnInteract(Player player)
    {
        ChatLogManager.Instance.StartSession(npcData);

        var uiData = new UIBaseData();
        UIManager.Instance.OpenUI<ChatUI>(uiData);

        var chatUI = UIManager.Instance.GetActiveUI<ChatUI>();
        chatUI.SetChat(this, player);

        // 퀘스트 상태에 따른 대사 처리
        HandleQuest(player);
    }

    // 퀘스트 상태를 확인하고 상황에 맞는 대사 및 처리 실행
    private void HandleQuest(Player player)
    {
        // 현재 NPC가 제공할 수 있는 퀘스트 가져오기
        var quest = QuestManager.Instance.GetAvailableQuest(this);

        // 줄 수 있는 퀘스트가 없다면 기본 NPC 대사 출력
        if (quest == null)
        {
            ChatLogManager.Instance.AddLine(false, npcData.NpcPrompt);
            return;
        }

        // 해당 퀘스트의 현재 상태 확인
        var state = QuestManager.Instance.GetQuestState(quest.questId);

        // 아직 시작하지 않은 퀘스트인 경우
        if (state == QuestState.NotStarted)
        {
            // 퀘스트 시작 전 대사 출력
            ChatLogManager.Instance.AddLine(false, quest.questNotStarted);

            //// 퀘스트를 진행 상태로 변경
            //QuestManager.Instance.StartQuest(quest.questId);
            return;
        }

        // 진행 중인 퀘스트인 경우
        if (state == QuestState.InProgress)
        {
            // 완료 조건(아이템 등) 충족 여부 확인
            bool completed = QuestManager.Instance.CheckQuestComplete(quest.questId);

            // 조건을 만족했다면 완료 대사, 아니라면 진행 중 대사 출력
            ChatLogManager.Instance.AddLine(
                false,
                completed ? quest.questOnComplete : quest.questInProgress
            );

            return;
        }

        // 이미 완료된 퀘스트인 경우
        // 완료 이후 반복 대사 출력
        ChatLogManager.Instance.AddLine(false, quest.questOnComplete);
    }

    // NPC 대화 취소시 나올것들
    public virtual void OffInteract()
    {
        UIManager.Instance.CloseUI(UIManager.Instance.GetActiveUI<ChatUI>());
    }
}
