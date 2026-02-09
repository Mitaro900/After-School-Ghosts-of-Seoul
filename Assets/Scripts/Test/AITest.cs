using TMPro;
using UnityEngine;

public class AITest : MonoBehaviour
{
    public OpenAIManager openAI;
    public TextMeshProUGUI result;

    public void TestCall(string input)
    {
        StartCoroutine(openAI.SendMessage(
            input,
            "너는 친절한 판타지 마을의 안내자 NPC야. 짧고 상냥하게 대답해.",
            (reply) => result.text = "NPC Reply: " + reply
        ));
    }

}
