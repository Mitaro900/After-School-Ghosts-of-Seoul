using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] protected string npcName;
    [SerializeField] protected GameObject pressE;



    public void ShowInteractUI()
    {
        pressE.SetActive(true);
    }

    public void HideInteractUI()
    {
        pressE.SetActive(false);
    }

    public virtual void OnInteract()
    {
        Debug.Log("NPC 대화 시작");
    }
}
