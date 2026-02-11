using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private protected ChatUI chatUI;

    public void OpenUI(UIBase ui)
    {
        if (ui == null)
        {
            Debug.LogError("OpenUI: 전달된 ui가 null임");
            return;
        }

        ui.Open();
    }

    public void CloseUI(UIBase ui)
    {
        ui.Close();
    }

    public void Chat(BaseChat sender, SpeakerType type, string text, float speed)
    {
        chatUI.Chat(sender, type, text, speed);
    }
}
