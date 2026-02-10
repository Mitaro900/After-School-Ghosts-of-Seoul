using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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

    [Header("Typing Effect")]
    [SerializeField] private float typingSpeed; // 채팅 스피드 저장


    public void Chat(SpeakerType speaker, string text, Sprite picture, float speed)
    {
        if (string.IsNullOrWhiteSpace(text)) return;  // 공백은 실행 안함

        typingSpeed = speed;    // 채팅 스피드 저장

        ChatBubble bubble = Instantiate(speaker == SpeakerType.Player ? playerBubble : npcBubble).GetComponent<ChatBubble>();   // 해당 컴퍼넌트를 가져옴
        bubble.transform.SetParent(contentRect.transform, false);   // 자식으로 소환
        bubble.boxRect.sizeDelta = new Vector2(600, 0);    // 박스 최대크기 x값 초기화
        bubble.InputText.text = ""; // 적은 챗 입력란 초기화
        StartCoroutine(TypeText(bubble, text));
    }


    // 채팅 속도
    private IEnumerator TypeText(ChatBubble bubble, string text)
    {
        TMP_Text tmp = bubble.InputText;
        tmp.text = "";

        foreach (char c in text)
        {
            tmp.text += c;

            // 글자 늘어날 때마다 사이즈 재계산
            UpdateBubbleSize(bubble);
            ScrollToBottom();
            yield return new WaitForSeconds(typingSpeed);
        }
    }


    // 실시간 채팅에 따라 말풍선 크기 수정
    private void UpdateBubbleSize(ChatBubble bubble)
    {
        TMP_Text tmp = bubble.InputText;

        // TMP 강제 갱신
        tmp.ForceMeshUpdate();

        // 실제 필요한 텍스트 크기
        float textWidth = tmp.preferredWidth;
        float textHeight = tmp.preferredHeight;

        // 패딩 (말풍선 여백)
        float paddingX = 42f;
        float paddingY = 24f;

        // 가로는 최대 600까지만
        float width = Mathf.Min(600, textWidth + paddingX);
        float height = textHeight + paddingY;

        bubble.boxRect.sizeDelta = new Vector2(width, height);

        Fit(bubble.boxRect);
    }


    // 텍스트 변경 후 말풍선,부모 UI 크기를 즉시 갱신
    private void Fit(RectTransform rect) => LayoutRebuilder.ForceRebuildLayoutImmediate(rect); 


    // 채팅중엔 화면 아래로 맞춤
    private void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
