using UnityEngine;

public class BaseChat : MonoBehaviour
{
    [SerializeField] private protected SpeakerType type;    // Player, NPC에 따라 나오는 말풍선 방향이 다름
    [SerializeField] private protected Sprite chatFace;
    [SerializeField] private protected float chatSpeed = 0.04f;

    protected virtual void Update()
    {

    }



    private protected void Send(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        UIManager.Instance.Chat(type, text, chatFace, chatSpeed);   // null 말고 chatFace넣어서 얼굴 나오게 하기
    }
}
