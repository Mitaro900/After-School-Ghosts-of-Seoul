using UnityEngine;

public class NPC : BaseChat
{
    [Header("NPC Info")]
    [SerializeField] protected NpcData npcData;
    public NpcData NpcData => npcData;
    [SerializeField] protected GameObject pressE;

    public bool isChatActive = false;

    // E키를 눌러주세요 활성화
    public void ShowPressEkeyUI()
    {
        pressE.SetActive(true);
        isChatActive = true;
    }

    // E키를 눌러주세요 비활성화
    public void HidePressEkeyUI()
    {
        pressE.SetActive(false);
        isChatActive = false;
    }

    // NPC 대화시 나올것들 (일단 대화, 퀘스트 대화 나뉘게)
    public virtual void OnInteract(Player player)
    {
        ChatLogManager.Instance.StartSession(npcData);

        var uiData = new UIBaseData();
        UIManager.Instance.OpenUI<ChatUI>(uiData);

        var chatUI = UIManager.Instance.GetActiveUI<ChatUI>();
        chatUI.SetChat(this, player);
    }

    // NPC 대화 취소시 나올것들
    public virtual void OffInteract()
    {
        isChatActive = false;

        UIManager.Instance.CloseUI(UIManager.Instance.GetActiveUI<ChatUI>());
    }
}
