using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum SpeakerType
{
    Player,
    NPC
}

public enum EmotionType
{
    Neutral,
    Happy,
    Angry,
    Sad
}

public class ChatUI : MonoBehaviour
{
    [Header("Chat info")]
    [SerializeField] private GameObject playerBubble;
    [SerializeField] private GameObject npcBubble;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Profile Images")]
    [SerializeField] private Image playerProfile;   // 플레이어 프로필 이미지
    [SerializeField] private Image npcProfile;  // NPC 프로필 이미지

    [Header("References")]
    [SerializeField] private OpenAIManager openAIManager;
    public NPC npc; // 대화중인 NPC

    public void Chat(BaseChat sender, SpeakerType speaker, string text, float speed)
    {
        if (string.IsNullOrWhiteSpace(text)) return;    // 공백이면 실행안함

        ChatBubble bubble = Instantiate(speaker == SpeakerType.Player ? playerBubble : npcBubble).GetComponent<ChatBubble>();   // 맞는 캐릭터의 말풍선을 생성하고 저장함

        bubble.transform.SetParent(contentRect, false); // 말풍선을 contentRect의 자식으로 붙임
        bubble.boxRect.sizeDelta = new Vector2(600, 0); // 말풍선의 최대 x가 600이 넘지 않게함
        bubble.InputText.text = ""; // 말풍선 입력값 초기화

        StartCoroutine(TypeText(bubble, text, speaker, sender, speed));

        StartCoroutine(openAIManager.SendMessage(
            text,
            npc.NpcData.NpcPrompt,
            (reply) =>
            {
                ChatBubble Nbubble = Instantiate(npcBubble).GetComponent<ChatBubble>();

                Nbubble.transform.SetParent(contentRect, false); // 말풍선을 contentRect의 자식으로 붙임
                Nbubble.boxRect.sizeDelta = new Vector2(600, 0); // 말풍선의 최대 x가 600이 넘지 않게함
                Nbubble.InputText.text = ""; // 말풍선 입력값 초기화

                StartCoroutine(TypeText(Nbubble, reply, SpeakerType.NPC, npc.NpcChat, speed));
            }
        ));
    }


    // 타이핑하며 감정상태 확인함
    private IEnumerator TypeText(ChatBubble bubble, string text, SpeakerType speaker, BaseChat sender, float speed)
    {
        TMP_Text tmp = bubble.InputText;    // 채팅 입력값 저장 
        Image targetProfile = speaker == SpeakerType.Player ? playerProfile : npcProfile;   // 어떤 캐릭터의 프로필인지 확인하고 저장

        //// 대화 시작시 기본 표정으로 변환
        //EmotionType currentEmotion = EmotionType.Neutral;
        //targetProfile.sprite = sender.GetEmotionSprite(currentEmotion);

        // 감정 파악 + 텍스트 타이핑
        for (int i = 0; i < text.Length; i++)
        {
            //// <angry> 같은 태그 감지
            //if (text[i] == '<')
            //{
            //    // 태그가 참이라면 감정 이미지 적용
            //    if (TryParseEmotion(text, ref i, out EmotionType newEmotion))
            //    {
            //        currentEmotion = newEmotion;    // 적용된 감정을 현재 감정에 적용
            //        targetProfile.sprite = sender.GetEmotionSprite(currentEmotion); // 해당 이미지에 맞게 변환
            //        continue;
            //    }
            //}

            tmp.text += text[i];
            UpdateBubbleSize(bubble);
            ScrollToBottom();

            yield return new WaitForSeconds(speed);
        }
    }


    // 텍스트 안에서 <감정> 감지시 해당<감정>의 enum값으로 변환
    private bool TryParseEmotion(string text, ref int index, out EmotionType emotion)
    {
        emotion = EmotionType.Neutral;  // 기본값은 기본표정으로

        int end = text.IndexOf('>', index); // 현재 '<' 이후에 '>'가 있는지 찾음
        if (end == -1) return false; // '>'가 없으면 태그가 아니므로 실패

        string tag = text.Substring(index + 1, end - index - 1); // '<'와 '>' 사이의 문자열만 추출하여 tag에 저장

        // 문자열을 EmotionType enum으로 변환
        if (System.Enum.TryParse(tag, true, out emotion))   // tag의 문자를 emotion으로 보냄
        {
            index = end; // 현재 '<' 이 문자를 '>' 이걸로 변경 i++중이라 '>' 다음 문자로 출력됨
            return true;
        }

        return false;
    }


    // 말풍선 사이즈를 현재 채팅에 맞게 변환함
    private void UpdateBubbleSize(ChatBubble bubble)
    {
        TMP_Text tmp = bubble.InputText;    // 지금까지 적힌 text를 가져옴
        tmp.ForceMeshUpdate(); // 텍스트 메쉬를 갱신

        float width = Mathf.Min(600, tmp.preferredWidth + 42f); // 가로길이 600으로 제한 + 좌우 여백 공간
        float height = tmp.preferredHeight + 24f;

        bubble.boxRect.sizeDelta = new Vector2(width, height);  // 텍스트 실제 필요한 세로 길이 + 상하 여백공간
        LayoutRebuilder.ForceRebuildLayoutImmediate(bubble.boxRect);   // 계산된 크기를 말풍선 RectTransform에 적용
    }


    // 채팅이 입력 될때마다 스크롤을 항상 아래로 고정
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();   // 캔버스 레이아웃을 즉시 갱신
        scrollRect.verticalNormalizedPosition = 0f; /// ScrollRect의 스크롤 위치를 맨 아래로 설정
    }
}

