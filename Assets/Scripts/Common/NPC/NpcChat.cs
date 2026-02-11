using UnityEngine;
using UnityEngine.UI;

public class NpcChat : BaseChat
{
    private void sandNpcChat(string npcText)
    {
        string text = npcText;  // 입력값 저장
        Send(text); // 채팅 발송 (속도, 얼굴은 BaseChat에서 바로 넣는 중)
        npcText = "";   // 입력값 초기화
    }
}
