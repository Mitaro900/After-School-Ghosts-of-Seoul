using UnityEngine;

public class BaseChat : MonoBehaviour
{
    [SerializeField] private protected SpeakerType type;    // Player, NPC에 따라 나오는 말풍선 방향이 다름
    [SerializeField] private protected float chatSpeed = 0.04f;

    [SerializeField] private Sprite Neutral;
    [SerializeField] private Sprite Happy;
    [SerializeField] private Sprite Angry;
    [SerializeField] private Sprite Sad;

    protected virtual void Update(){}


    // ChatUI에 받은 emotion 값에 따라 알맞은 Sprite를 선택해서 반환
    public Sprite GetEmotionSprite(EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => Happy,
            EmotionType.Angry => Angry,
            EmotionType.Sad => Sad,
            _ => Neutral    // 모두 해당 안하면 기본표정
        };
    }



    private protected void Send(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        UIManager.Instance.Chat(this, type, text, chatSpeed);
    }
}
