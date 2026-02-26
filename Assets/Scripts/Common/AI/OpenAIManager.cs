using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Singleton.Component;

public class OpenAIManager : SingletonComponent<OpenAIManager>
{
    // 로컬 테스트: http://localhost:3000/chat
    // Android 에뮬레이터: http://10.0.2.2:3000/chat
    // Cloud Run: https://YOUR_SERVICE-xxxxx.a.run.app/chat
    [SerializeField] private string API_URL = "http://localhost:3000/chat";

    // (옵션) 프록시에 간단 인증을 붙였을 때만 사용
    // 서버에서 X-App-Token 헤더를 검사하도록 만들면 됨.
    [SerializeField] private string appToken = "mygame-dev-token";

    #region Singleton
    protected override void AwakeInstance()
    {

    }

    protected override bool InitInstance()
    {
        return true;
    }

    protected override void ReleaseInstance()
    {

    }
    #endregion

    public IEnumerator SendMessage(
        string userMessage,
        string npcPrompt,
        string npcId,
        string playerName,
        int day,
        string location,
        string memorySummary,
        System.Action<ChatResponse> onComplete)
    {
        var requestData = new ChatRequest
        {
            input = userMessage,
            npcPrompt = npcPrompt,

            npcId = npcId,
            playerName = playerName,
            day = day,
            location = location,
            memorySummary = memorySummary,
        };

        string jsonBody = JsonUtility.ToJson(requestData);

        using (var request = new UnityWebRequest(API_URL, "POST"))
        {
            request.timeout = 30;

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

            if (!string.IsNullOrEmpty(appToken))
                request.SetRequestHeader("X-App-Token", appToken);

            yield return request.SendWebRequest();

            var raw = request.downloadHandler?.text ?? "";
            bool httpOk = request.responseCode >= 200 && request.responseCode < 300;

            if (request.result == UnityWebRequest.Result.Success && httpOk)
            {
                ChatResponse response = null;
                try { response = JsonUtility.FromJson<ChatResponse>(raw); } catch { }

                if (response == null)
                    response = new ChatResponse { text = "응답 파싱 실패" };

                if (string.IsNullOrWhiteSpace(response.text))
                    response.text = "죄송합니다, 응답을 처리하지 못했습니다.";

                onComplete?.Invoke(response);
            }
            else
            {
                Debug.LogError($"Proxy Error: {request.responseCode} {request.error}\nURL: {API_URL}\n{raw}");
                onComplete?.Invoke(new ChatResponse { text = "죄송합니다, 응답을 받지 못했습니다." });
            }
        }
    }
}

[System.Serializable]
public class ChatRequest
{
    public string input;
    public string npcPrompt;

    // 확장 컨텍스트
    public string npcId;
    public string playerName;
    public int day;
    public string location;
    public string memorySummary;
}

[System.Serializable]
public class ChatResponse
{
    public string text;
}
