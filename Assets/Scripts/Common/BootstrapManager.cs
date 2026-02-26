using UnityEngine;
using Singleton;
using Singleton.Data;
using Singleton.Component;

public class BootstrapManager : MonoBehaviour
{
    private static bool s_initialized = false;

    [Header("초기화 옵션")]
    [SerializeField] private bool initializeOnAwake = true;

    private void Awake()
    {
        // 중복 방지
        if (s_initialized)
        {
            Destroy(gameObject);
            return;
        }

        s_initialized = true;
        DontDestroyOnLoad(gameObject);

        Singleton.SingletonGate.Enable();
        Singleton.SingletonGate.ResetReleaseState();

        if (initializeOnAwake)
        {
            InitializeAllSingletons();
        }
    }

    public static void InitializeAllSingletons()
    {
        if (SingletonGate.IsBlocked)
            return;

        // 1. Data 싱글톤 초기화
        if (!SingletonData.InitSingletons())
        {
            Debug.LogError("Data Singleton Initialization Failed");
        }

        // 2. Component 싱글톤 초기화
        if (!SingletonComponent.InitSingletons())
        {
            Debug.LogError("Component Singleton Initialization Failed");
        }
    }

    public static void ReleaseAllSingletons()
    {
        if (SingletonGate.IsBlocked)
            return;

        // 역순 해제는 각 클래스 내부에서 처리
        SingletonComponent.ReleaseSingletons();
        SingletonData.ReleaseSingletons();
    }
}
