using UnityEngine;

public class BaseChat : MonoBehaviour
{
    [SerializeField] private protected SpeakerType type;
    [SerializeField] private protected ChatUI chatUI;

    protected virtual void Update()
    {

    }



    private protected void Send(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        chatUI.Chat(type, text, null);
    }
}
