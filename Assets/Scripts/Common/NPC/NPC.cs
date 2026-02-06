using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] protected string npcName;
    [SerializeField] protected GameObject pressE;



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

    // NPC 대화시 나올것들 (일단 대화, 퀘스트 대화 나뉘게)
    public virtual void OnInteract()
    {
        Debug.Log("NPC 대화 시작");
    }
}
