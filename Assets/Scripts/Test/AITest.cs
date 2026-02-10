using TMPro;
using UnityEngine;

public class AITest : MonoBehaviour
{

    [Header("Reference")]
    [SerializeField] private OpenAIManager aiManager;
    [SerializeField] private TextMeshProUGUI result;

    [Header("Test Data")]
    [TextArea]
    [SerializeField]
    private string npcPrompt =
        "너는 친절한 판타지 마을의 안내자 NPC야. 짧고 상냥하게 대답해.";

    // Inspector에서 버튼으로 호출해도 되고
    // Start()에서 자동 실행해도 됨
    public void TestCall(string userMessage)
    {
        Debug.Log("▶ Proxy AI Test started");

        StartCoroutine(aiManager.SendMessage(
            userMessage,
            npcPrompt,
            (reply) =>
            {
                result.text = "NPC Reply: " + reply;
            }
        ));
    }
}
