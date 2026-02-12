using UnityEngine;

// 다른 스크립트에게 싱글톤을 적용시킵니다. <T>에서 T는 무조건 MonoBehaviour 이여야 함 (예시 : AudioManager : Singleton<AudioManager> )
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        // 중복 Instance 방지
        if (Instance != null && Instance != this)   // Instance가 Null이 아닐경우
        {
            Destroy(gameObject);    // 중복된 자신을 파괴함
            return;
        }

        // Instance가 씬이 넘어가도 파괴되지 않게함
        Instance = (T)(object)this;   // 적용된 T를 Instance에 저장합니다.
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
