using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UINpcLogPanel : MonoBehaviour
{
    [Header("Main object")]
    [SerializeField] private GameObject logCavars;  // 이전 대화내용 보는 캔버스
    [SerializeField] private GameObject openButton; // 여는 버튼
    [SerializeField] private GameObject CloseButton;    // 닫는 버튼
    [SerializeField] private GameObject deleteCurrentButton;    // 세션 삭제 버튼

    [Header("Left List")]
    [SerializeField] private Transform logContent;          // NPC LOG 선택 버튼
    [SerializeField] private GameObject sessionButtonPrefab; // 세션 버튼 프리팹

    [Header("Right Detail")]
    [SerializeField] private Transform chatContent;         // NPC LOG 내용 버튼
    [SerializeField] private GameObject playerBubblePrefab; // 플레이어 말풍선 프리팹
    [SerializeField] private GameObject npcBubblePrefab;    // NPC 말풍선 프리팹

    private NpcData currentNpc; // 현재 선택된 NPC
    private HashSet<DialogueSession> createdSessions = new HashSet<DialogueSession>();  // 이전에 만든 대화 내용인지 저장
    private DialogueSession currentSession; // 현재 열람중인 대화 내용


    // 버튼 누를시 Log UI창 활성화 (저장된 NPC대화 데이터도 가져옴)
    public void OpenLog()
    {
        openButton.SetActive(false);
        CloseButton.SetActive(true);
        deleteCurrentButton.SetActive(true);
        logCavars.SetActive(true);

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

    public void CloseLog()
    {
        currentSession = null;  // 현재 열람중인 대화 초기화

        openButton.SetActive(true);
        CloseButton.SetActive(false);
        deleteCurrentButton.SetActive(false);
        logCavars.SetActive(false);
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

        currentSession = session;

        ClearChatView();

        foreach (var line in session.lines)
        {
            var prefab = line.isPlayer ? playerBubblePrefab : npcBubblePrefab;

            var go = Instantiate(prefab, chatContent);
            go.GetComponentInChildren<TMP_Text>().text = line.message;
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
