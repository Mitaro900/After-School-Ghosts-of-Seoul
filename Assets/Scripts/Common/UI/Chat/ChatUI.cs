using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum EmotionType
{
    Neutral,
    Happy,
    Angry,
    Sad
}

public class ChatUI : UIBase
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
    [SerializeField] private TMP_InputField input;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.02f; // 타이핑 속도

    private NPC npc; // 대화중인 NPC 저장
    private Player player;

    public System.Action<string> OnSubmitText;

    private bool _busy; // 타이핑 중 입력 잠금
    private Coroutine _turnCo; // 중복 전송 방지

    private void ClearFocusIfOwned()
    {
        if (!input || EventSystem.current == null) return;
        if (EventSystem.current.currentSelectedGameObject == input.gameObject)
            EventSystem.current.SetSelectedGameObject(null);
    }

    // 채팅 끝내면 npc값 저장 초기화
    public override void CloseUI(bool isCloseAll = false)
    {
        // 포커스 회수 먼저(입력 새는 문제 방지)
        ClearFocusIfOwned();

        // 진행중 코루틴 정리(선택)
        if (_turnCo != null)
        {
            StopCoroutine(_turnCo);
            _turnCo = null;
        }

        _busy = false;
        if (input) input.interactable = true;

        base.CloseUI(isCloseAll);
        npc = null;
    }

    private void Update()
    {
        if (!input) return;
        if (_busy) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // Enter / NumpadEnter만
        bool enter =
            (kb.enterKey != null && kb.enterKey.wasPressedThisFrame) ||
            (kb.numpadEnterKey != null && kb.numpadEnterKey.wasPressedThisFrame);

        if (!enter) return;

        Debug.Log("Enter pressed");

        var text = input.text;
        if (string.IsNullOrWhiteSpace(text)) return;

        // 전송 처리
        input.text = "";

        OnSubmitText?.Invoke(text.Trim());

        StartTurn(text.Trim());
    }

    private void StartTurn(string playerText)
    {
        if (_turnCo != null)
        {
            StopCoroutine(_turnCo);
            _turnCo = null;
        }

        _turnCo = StartCoroutine(TurnCo(playerText));
    }

    private IEnumerator TurnCo(string playerText)
    {
        _busy = true;
        if (input) input.interactable = false;

        if (npc == null)
        {
            AddSystemBubble("NPC가 설정되지 않았습니다. (SetChat 호출 필요)");
            EndBusy();
            yield break;
        }

        // 1) 플레이어 말풍선 타이핑(끝날 때까지 기다림)
        var pBubble = CreateBubble(isPlayer: true);
        yield return StartCoroutine(TypeText(pBubble, playerText, true, player, typingSpeed));
        ChatLogManager.Instance.AddLine(true, playerText);

        // 2) 서버 요청(플레이어 타이핑 끝난 뒤)
        string reply = null;
        yield return StartCoroutine(OpenAIManager.Instance.SendMessage(
            playerText,
            npc.NpcData.NpcPrompt,
            r => reply = r
        ));

        if (string.IsNullOrWhiteSpace(reply))
            reply = "죄송합니다, 응답을 받지 못했습니다.";

        // 3) NPC 말풍선 타이핑(끝날 때까지)
        var nBubble = CreateBubble(isPlayer: false);
        yield return StartCoroutine(TypeText(nBubble, reply, false, npc, typingSpeed));
        ChatLogManager.Instance.AddLine(false, reply);

        EndBusy();
    }

    private void EndBusy()
    {
        _busy = false;
        if (input) input.interactable = true;
        _turnCo = null;
    }

    private ChatBubble CreateBubble(bool isPlayer)
    {
        var prefab = isPlayer ? playerBubble : npcBubble;
        var bubble = Instantiate(prefab).GetComponent<ChatBubble>();

        bubble.transform.SetParent(contentRect, false);
        bubble.boxRect.sizeDelta = new Vector2(600, 0);
        bubble.InputText.text = "";
        return bubble;
    }

    private void AddSystemBubble(string text)
    {
        var bubble = CreateBubble(isPlayer: false);
        bubble.InputText.text = text;
        UpdateBubbleSize(bubble);
        ScrollToBottom();
    }

    // 타이핑하며 감정상태 확인함
    private IEnumerator TypeText(ChatBubble bubble, string text, bool isPlayer, BaseChat sender, float speed)
    {
        TMP_Text tmp = bubble.InputText;
        Image targetProfile = isPlayer ? playerProfile : npcProfile;

        EmotionType currentEmotion = EmotionType.Neutral;
        if (sender != null)
            targetProfile.sprite = sender.GetEmotionSprite(currentEmotion);

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '<')
            {
                if (TryParseEmotion(text, ref i, out EmotionType newEmotion))
                {
                    currentEmotion = newEmotion;
                    if (sender != null)
                        targetProfile.sprite = sender.GetEmotionSprite(currentEmotion);
                    continue;
                }
            }

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
        tmp.ForceMeshUpdate();              // 텍스트 메쉬를 갱신

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

    public void SetChat(NPC chattingNpc, Player chattingPlayer)
    {
        npc = chattingNpc;
        player = chattingPlayer;
    }
}
