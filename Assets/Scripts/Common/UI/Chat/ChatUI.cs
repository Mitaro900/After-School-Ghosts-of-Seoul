using System;
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

    [Header("Choice UI")]
    [SerializeField] private Transform choicesRoot;
    [SerializeField] private Button choiceButtonPrefab;

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
        // AI 호출 전에 이전 선택지 제거
        ClearChoices();

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
        
        string questId = "";
        string questNpcPrompt = "";
        Choice[] availableSteps = Array.Empty<Choice>();

        // quest.useSteps가 켜진 퀘스트만 step choices 제공
        availableSteps = QuestManager.Instance.GetAvailableStepsForNpcAsChoices(questId, npc);

        if (quest != null && !string.IsNullOrEmpty(quest.questId))
        {
            questId = quest.questId;

            var state = QuestManager.Instance.GetQuestState(questId);
            questNpcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);

            // step 기반이면 availableSteps 채우기 (quest.useSteps인 경우)
            availableSteps = QuestManager.Instance.GetAvailableStepsForNpcAsChoices(questId, npc);
        }
        else
        {
            // 퀘스트 없으면 잡담/일반모드
            questNpcPrompt = "현재 진행 중인 퀘스트가 없다. 자연스럽게 대화만 이어가라. 플레이어가 원하면 주변 소문/일상 이야기를 하라.";
        }
        
        //var state = QuestManager.Instance.GetQuestState(questId);

        //// 퀘스트 상태에 따라 npcPrompt 가져오기
        //if (state == QuestState.NotStarted)
        //{
        //    questNpcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);
        //    Debug.Log(questNpcPrompt);

        //    // 플레이어가 긍정적인 말 입력 시 퀘스트 시작
        //    if (IsPlayerAccepting(playerText))
        //    {
        //        QuestManager.Instance.StartQuest(questId);
        //        questNpcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId); // InProgress 대사로 갱신
        //    }
        //}
        //else if (state == QuestState.InProgress)
        //{
        //    // 아이템 가져온것을 확인 후 완료로 변경
        //    if (QuestManager.Instance.CheckQuestComplete(questId))
        //    {
        //        questNpcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);   // Completed 대사로 갱신
        //    }
        //}
        //else
        //{
        //    questNpcPrompt = QuestManager.Instance.GetQuestNpcPrompt(questId);
        //}

        // AI 프롬프트 구성
        var topStep = QuestManager.Instance.GetTopAvailableStep(questId, npc);
        string stepPrompt = topStep != null ? topStep.stepPromptOverride : "";

        string finalPrompt = $@"
        [기본 설정]
        {npc.NpcData.npcPrompt}
        
        [현재 퀘스트 상태]
        {questNpcPrompt}
        
        [현재 핵심 Step 가이드]
        {stepPrompt}
        
        플레이어 입력에 맞춰 대답하세요.
        ";

        QuestManager.Instance.MarkTalked(questId, npc.NpcData);

        // AI 호출
        // (예시) NPC ID/Day/Location은 프로젝트 상황에 맞춰 넣기
        string npcId = npc.NpcData != null ? npc.NpcData.name : ""; // NpcData에 id가 있으면 그걸 쓰는 게 베스트
        string playerName = "이서준";
        int day = 1;              // DayManager 있으면 그걸로
        string location = "";     // LocationManager 있으면 그걸로

        ChatResponse reply = null;
        yield return StartCoroutine(OpenAIManager.Instance.SendMessage(
            userMessage: playerText,
            npcPrompt: finalPrompt,
            npcId: npcId,
            playerName: playerName,
            day: day,
            location: location,
            memorySummary: "", // ChatLogManager 요약 있으면 넣기
            availableSteps: availableSteps,
            onComplete: r => reply = r
        ));

        if (string.IsNullOrWhiteSpace(reply.text))
            reply.text = "죄송합니다, 응답을 받지 못했습니다.";

        // NPC가 실제로 답변한 경우에만 로그에 기록
        if (hasPlayerInput)
        {
            var nBubble = CreateBubble(false);
            yield return StartCoroutine(TypeText(nBubble, reply.text, false, npc, typingSpeed));

            // NPC 답변 출력 끝난 뒤
            RenderChoices(reply.choices);

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

    #region 선택지
    private void ClearChoices()
    {
        if (!choicesRoot) return;
        for (int i = choicesRoot.childCount - 1; i >= 0; i--)
            Destroy(choicesRoot.GetChild(i).gameObject);

        choicesRoot.gameObject.SetActive(false);
    }

    private void RenderChoices(Choice[] choices)
    {
        ClearChoices();

        if (!choicesRoot || !choiceButtonPrefab) return;
        if (choices == null || choices.Length == 0) return;

        choicesRoot.gameObject.SetActive(true);

        foreach (var c in choices)
        {
            if (c == null || string.IsNullOrWhiteSpace(c.id) || string.IsNullOrWhiteSpace(c.label))
                continue;

            var btn = Instantiate(choiceButtonPrefab, choicesRoot);
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = c.label;

            btn.onClick.AddListener(() =>
            {
                // 버튼 클릭 시 처리
                OnChoiceClicked(c);
            });
        }
    }

    private void OnChoiceClicked(Choice c)
    {
        if (c == null) return;

        // 1) 플레이어 선택을 말풍선으로 보여주고 싶으면(선택)
        // StartTurn(c.label);  // 선택지를 플레이어 입력처럼 처리하고 싶으면 이 줄 사용

        // 2) quest step이면 ApplyStep
        if (TryParseQuestStep(c.id, out var questId, out var stepId))
        {
            QuestManager.Instance.ApplyStep(questId, stepId);
        }

        // 3) 선택 후 다음 턴을 "선택지 라벨"로 진행시키면 대화 흐름이 자연스러움
        StartTurn(c.label);
    }

    private bool TryParseQuestStep(string id, out string questId, out string stepId)
    {
        questId = null;
        stepId = null;

        if (string.IsNullOrWhiteSpace(id)) return false;

        // 권장 포맷: "<questId>:<stepId>"
        var parts = id.Split(':');
        if (parts.Length != 2) return false;

        questId = parts[0];
        stepId = parts[1];

        return !string.IsNullOrEmpty(questId) && !string.IsNullOrEmpty(stepId);
    }
    #endregion
}
