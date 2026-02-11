using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private protected ChatUI chatUI;

    // UI를 엽니다.
    public void OpenUI(GameObject ui)
    {
        ui.SetActive(true);
    }

    public void CloseUI(GameObject ui)
    {
        ui.SetActive(false);
    }

    public void Chat(BaseChat sender, SpeakerType type, string text, float speed)
    {
        chatUI.Chat(sender, type, text, speed);
    }
}
