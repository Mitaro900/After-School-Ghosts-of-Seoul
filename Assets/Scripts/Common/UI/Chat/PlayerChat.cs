using TMPro;
using UnityEngine;


public class PlayerChat : BaseChat
{
    [SerializeField] private TMP_InputField inputField;


    protected override void Update()
    {
        if (PlayerInputManager.Instance.chatEnterAction.WasPressedThisFrame())
        {
            string text = inputField.text;  // 입력값 저장
            Send(text); // 채팅 발송 (속도, 얼굴은 BaseChat에서 바로 넣는 중)
            inputField.text = "";   // 입력값 초기화
            inputField.ActivateInputField(); // 입력준비(다른 곳을 누르지 않는 이상 입력 활성화)
        }
    }
}