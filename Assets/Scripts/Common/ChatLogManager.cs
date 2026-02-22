using System.Collections.Generic;
using UnityEngine;

// 대화 한 줄 정보를 담는 클래스
public class DialogueLine
{
    public bool isPlayer;    // 플레이어가 말했는지 여부
    public string message;   // 대화 내용

    // 생성자: 한 줄 대화를 생성할 때 사용
    public DialogueLine(bool isPlayer, string message)
    {
        this.isPlayer = isPlayer;
        this.message = message;
    }
}

// NPC와 관련된 한 번의 대화 세션을 담는 클래스
public class DialogueSession
{
    public NpcData npcData;                 // 이 세션의 NPC 정보
    public int sessionIndex;                // 세션 순번 (1, 2, 3…)
    public List<DialogueLine> lines = new List<DialogueLine>(); // 세션 내 대화 줄들
}

// 채팅 로그를 관리하는 매니저 클래스
public class ChatLogManager : Singleton<ChatLogManager>
{
    // NPC별 대화 세션을 저장하는 Dictionary
    private Dictionary<NpcData, List<DialogueSession>> npcSessions
        = new Dictionary<NpcData, List<DialogueSession>>();

    // 현재 진행 중인 대화 세션
    private DialogueSession currentSession;

    // 대화 시작
    public void StartSession(NpcData npcData)
    {
        // 해당 NPC의 세션이 없으면 새 리스트 생성
        if (!npcSessions.ContainsKey(npcData))
            npcSessions[npcData] = new List<DialogueSession>();

        // 새 세션의 인덱스 = 기존 세션 개수 + 1
        int newIndex = npcSessions[npcData].Count + 1;

        // 현재 세션 생성
        currentSession = new DialogueSession
        {
            npcData = npcData,
            sessionIndex = newIndex
        };
    }

    // 한 줄 대화 추가
    public void AddLine(bool isPlayer, string message)
    {
        if (currentSession == null) return; // 세션이 없으면 무시

        // 현재 세션에 한 줄 대화 추가
        currentSession.lines.Add(new DialogueLine(isPlayer, message));
    }

    // 대화 종료
    public void EndSession()
    {
        if (currentSession == null) return; // 세션이 없으면 무시

        // Dictionary에서 NPC의 세션 리스트에 추가
        npcSessions[currentSession.npcData].Add(currentSession);

        // 현재 세션 초기화
        currentSession = null;
    }

    // 특정 NPC의 모든 세션 가져오기
    public Dictionary<NpcData, List<DialogueSession>> GetAllNpcSessions()
    {
        return npcSessions; // 현재 저장된 NPC별 세션 전체 반환
    }
}
