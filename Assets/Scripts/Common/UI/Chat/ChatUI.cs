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
    [SerializeField] private TMP_Text inputHelp;

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
        // 타이핑 중이 아니라면 채팅창 나가기 허용
        if (npc != null && PlayerInputManager.Instance.cancelAction.WasPressedThisFrame() && !_busy)
        {
            npc.OffInteract();
            player.PlayerMove(true);    // 플레이어 움직임 허용
        }


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
        inputHelp.text = "채팅이 타이핑 되는동안 입력,ESC가 불가능 합니다.";

        if (npc == null)
        {
            AddSystemBubble("NPC가 설정되지 않았습니다. (SetChat 호출 필요)");
            EndBusy();
            yield break;
        }

        // 플레이어가 실제로 입력한 경우에만 세션 시작
        bool hasPlayerInput = !string.IsNullOrWhiteSpace(playerText);
        if (hasPlayerInput)
        {
            ChatLogManager.Instance.StartSession(npc.NpcData);

            // 플레이어 말풍선 타이핑
            var pBubble = CreateBubble(isPlayer: true);
            yield return StartCoroutine(TypeText(pBubble, playerText, true, player, typingSpeed));

            // 플레이어 대사 로그에 추가
            ChatLogManager.Instance.AddLine(true, playerText);
        }

        // 퀘스트 정보 가져오기
        var quest = QuestManager.Instance.GetAvailableQuest(npc);
        if (quest == null)
        {
            EndBusy();
            yield break;
        }

        string questId = quest.questId;
        if (string.IsNullOrEmpty(questId))
        {
            EndBusy();
            yield break;
        }

        var state = QuestManager.Instance.GetQuestState(questId);
        string npcPrompt = "";

        // 퀘스트 상태에 따라 npcPrompt 가져오기
        if (state == QuestState.NotStarted)
        {
            npcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);
            Debug.Log(npcPrompt);

            // 플레이어가 긍정적인 말 입력 시 퀘스트 시작
            if (IsPlayerAccepting(playerText))
            {
                QuestManager.Instance.StartQuest(questId);
                npcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId); // InProgress 대사로 갱신

                //AddSystemBubble("퀘스트를 수락했습니다!"); 재현님에게 물어보고 수락 되었다고 시스템 말풍선 보낼지 물어보기
            }
        }
        else if (state == QuestState.InProgress)
        {
            // 아이템 가져온것을 확인 후 완료로 변경
            if (QuestManager.Instance.CheckQuestComplete(questId))
            {
                npcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);   // Completed 대사로 갱신
            }
        }
        else
        {
            npcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);
        }

        // AI 프롬프트 구성
        string finalPrompt = $@"
        당신은 이 게임의 NPC입니다. 
        성격을 유지하며 자연스럽게 대화하세요.

        [기본 설정]
        {npc.NpcData.NpcPrompt}

        [현재 퀘스트 상태]
        {npcPrompt}

        플레이어 입력에 맞춰 대답하세요.
        ";

        // AI 호출
        ChatResponse reply = null;
        yield return StartCoroutine(OpenAIManager.Instance.SendMessage(
            playerText,
            finalPrompt,
            r => reply = r
        ));

        if (string.IsNullOrWhiteSpace(reply.text))
            reply.text = "죄송합니다, 응답을 받지 못했습니다.";

        // NPC가 실제로 답변한 경우에만 로그에 기록
        if (hasPlayerInput)
        {
            var nBubble = CreateBubble(false);
            yield return StartCoroutine(TypeText(nBubble, reply.text, false, npc, typingSpeed));

            ChatLogManager.Instance.AddLine(false, reply.text);
            ChatLogManager.Instance.EndSession(); // 세션 종료
        }

        EndBusy();
    }


    private void EndBusy()
    {
        _busy = false;
        if (input) input.interactable = true;
        inputHelp.text = "텍스트를 입력하세요. (Esc를 누르면 종료됩니다.)";
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

    // 플레이어 입력 긍정 체크 함수
    private bool IsPlayerAccepting(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return false;

        string lowerInput = input.Trim().ToLower();
        string[] positiveWords = { "수락", "좋아요", "응", "알겠습니다", "그래", "네" };

        foreach (var word in positiveWords)
            if (lowerInput.Contains(word)) return true;

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
