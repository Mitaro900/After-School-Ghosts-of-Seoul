using System.Xml.Linq;
using UnityEngine;

public class BaseChat : MonoBehaviour
{
    [SerializeField] private protected bool isPlayer;    // Player, NPC에 따라 나오는 말풍선 방향이 다름
    [SerializeField] private protected float chatSpeed = 0.04f;

    protected virtual void Update(){}


    // ChatUI에 받은 emotion 값에 따라 알맞은 Sprite를 선택해서 반환
    public Sprite npcGetEmotionSprite(NpcData npc, EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => npc.Happy,
            EmotionType.Angry => npc.Angry,
            EmotionType.Sad => npc.Sad,
            _ => npc.Neutral    // 모두 해당 안하면 기본표정
        };
    }

    public Sprite playerGetEmotionSprite(Player player, EmotionType emotion)
    {
        return emotion switch
        {
            EmotionType.Happy => player.Happy,
            EmotionType.Angry => player.Angry,
            EmotionType.Sad => player.Sad,
            _ => player.Neutral    // 모두 해당 안하면 기본표정
        };
    }

    private protected void Send(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
    }
}
