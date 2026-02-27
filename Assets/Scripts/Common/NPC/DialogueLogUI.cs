using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueLogUI : UIBase
{
    [Header("Left List")]
    [SerializeField] private Transform logContent;              // NPC LOG 선택 버튼
    [SerializeField] private GameObject sessionButtonPrefab;    // 세션 버튼 프리팹

    [Header("Right Detail")]
    [SerializeField] private Transform chatContent;             // NPC LOG 내용 버튼
    [SerializeField] private GameObject playerLogBubblePrefab;  // 플레이어 말풍선 프리팹
    [SerializeField] private GameObject npcLogBubblePrefab;     // NPC 말풍선 프리팹

    private HashSet<DialogueSession> createdSessions = new HashSet<DialogueSession>(); // 이전에 만든 대화 내용인지 저장
    private DialogueSession currentSession; // 현재 열람중인 대화 내용

    public override void ShowUI()
    {
        base.ShowUI();

        AudioManager.Instance.PlaySFX(SFX.ui_open1);

        // ChatLogManager에 등록된 모든 NPC 데이터 가져오기
        foreach (var npcPair in ChatLogManager.Instance.GetAllNpcSessions())
        {
            foreach (var session in npcPair.Value)
            {
                // 이미 생성된 세션이면 건너뜀
                if (createdSessions.Contains(session))
                    continue;

                CreateSessionButton(session);
                createdSessions.Add(session);
            }
        }
    }

    public override void OnClickCloseButton()
    {
        currentSession = null; // 현재 열람중인 대화 초기화
        AudioManager.Instance.PlaySFX(SFX.ui_close);
        ClearChatView();
        base.OnClickCloseButton();
    }

    // 왼쪽 세션 버튼 생성
    private void CreateSessionButton(DialogueSession session)
    {
        var go = Instantiate(sessionButtonPrefab, logContent);

        // 버튼 텍스트
        go.GetComponentInChildren<TMP_Text>().text = $"{session.npcData.displayName}\n - {session.sessionIndex} -";

        // 버튼 클릭 시 해당 대화 내용 표시
        go.GetComponent<Button>().onClick.AddListener(() => ShowSession(session));
    }

    // 오른쪽 대화 말풍선 생성
    private void ShowSession(DialogueSession session)
    {
        if (currentSession == session)
            return;

        if (Random.value < 0.5f)
        {
            AudioManager.Instance.PlaySFX(SFX.ui_open3);
        }
        else
        {
            AudioManager.Instance.PlaySFX(SFX.ui_open4);
        }

        currentSession = session;

        ClearChatView();

        foreach (var line in session.lines)
        {
            var prefab = line.isPlayer ? playerLogBubblePrefab : npcLogBubblePrefab;

            var go = Instantiate(prefab, chatContent);

            var bubbleUI = go.GetComponent<LogBubbleUI>();

            bubbleUI.SetMessage(line.message);

            if (line.isPlayer)
            {
                bubbleUI.SetProfile(GameManager.Instance.Player.playerProfile);
            }
            else
            {
                bubbleUI.SetProfile(session.npcData.npcProfile);
            }
        }
    }

    // 오른쪽 대화 초기화
    private void ClearChatView()
    {
        foreach (Transform child in chatContent)
            Destroy(child.gameObject);
    }

    // 세션 삭제
    public void DeleteCurrentSession()
    {
        if (currentSession == null)
            return;

        if (Random.value < 0.5f)
        {
            AudioManager.Instance.PlaySFX(SFX.ui_delete1);
        }
        else
        {
            AudioManager.Instance.PlaySFX(SFX.ui_delete2);
        }

        ChatLogManager.Instance.RemoveSession(currentSession);

        currentSession = null;

        ClearChatView();
        RefreshSessionList();
    }

    // 세션 재정렬
    private void RefreshSessionList()
    {
        foreach (Transform child in logContent)
            Destroy(child.gameObject);

        foreach (var npcPair in ChatLogManager.Instance.GetAllNpcSessions())
        {
            foreach (var session in npcPair.Value)
            {
                CreateSessionButton(session);
            }
        }
    }
}
