using UnityEngine;
using Singleton.Component;
using Singleton.Data;

public static class TestAutoInit
{
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitForTests()
    {
        // PlayMode 테스트/에디터에서만 돌리고 싶으면 조건을 더 달아도 됨.
        Singleton.SingletonGate.Enable();
        Singleton.SingletonGate.ResetReleaseState();

        SingletonComponent.InitSingletons();
        SingletonData.InitSingletons();
    }
#endif
}
