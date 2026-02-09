using TMPro;
using UnityEngine;


public class PlayerChat : BaseChat
{
    [SerializeField] private TMP_InputField inputField;


    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            string text = inputField.text;  // 입력값 저장
            Send(text); // 채팅 발송
            inputField.text = "";   // 입력값 초기화
            inputField.ActivateInputField(); // 입력준비
        }
    }
}