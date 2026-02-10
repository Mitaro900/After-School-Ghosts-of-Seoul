using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] protected string npcName;
    [SerializeField] protected GameObject pressE;
    [SerializeField] protected GameObject chatUI;
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
    public virtual void OnInteract()
    {
        if (!isChatActive) return;
        UIManager.Instance.OpenUI(chatUI);
    }

    // NPC 대화 취소시 나올것들
    public virtual void OffInteract()
    {
        if (!isChatActive) return;
        isChatActive = false;
        UIManager.Instance.CloseUI(chatUI);
    }
}
