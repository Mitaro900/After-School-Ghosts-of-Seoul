using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum SpeakerType
{
    Player,
    NPC
}
public class ChatUI : MonoBehaviour
{
    [Header("Chat info")]
    [SerializeField] private GameObject playerBubble;
    [SerializeField] private GameObject npcBubble;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollBar;


    private ChatBubble chatBubble;


    public void Chat(SpeakerType speaker, string text, Texture picture)
    {
        if (string.IsNullOrWhiteSpace(text)) return;  // 공백은 실행 안함

        ChatBubble bubble = Instantiate(speaker == SpeakerType.Player ? playerBubble : npcBubble).GetComponent<ChatBubble>();   // 해당 컴퍼넌트를 가져옴
        bubble.transform.SetParent(contentRect.transform, false);   // 자식으로 소환
        bubble.boxRect.sizeDelta = new Vector2(600, bubble.boxRect.sizeDelta.y);    // 가로 최대 600 , 위는 기존 설정만큼
        bubble.TextRect.GetComponent<TMP_Text>().text = text;   // 채팅의 Text를 입력한 text로 변환
        Fit(bubble.boxRect);
        ScrollToBottom();


        // 두 줄 이상이면 크기를 줄여가면서, 한 줄이 아래로 내려가면 바로 전 크기를 대입함
        float X = bubble.TextRect.sizeDelta.x + 42;
        float Y = bubble.TextRect.sizeDelta.y;
        if (Y > 49)
        {
            for (int i = 0; i < 200; i++)
            {
                bubble.boxRect.sizeDelta = new Vector2(X - i * 2, bubble.boxRect.sizeDelta.y);
                Fit(bubble.boxRect);

                if (Y != bubble.TextRect.sizeDelta.y) { bubble.boxRect.sizeDelta = new Vector2(X - (i * 2) + 2, Y); break; }
            }
        }
        else bubble.boxRect.sizeDelta = new Vector2(X, Y);
    }

    private void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect);


    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
