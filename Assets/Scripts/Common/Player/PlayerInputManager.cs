using Singleton.Component;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : SingletonComponent<PlayerInputManager>
{
    [Header("Player Input")]
    public InputAction moveAction;      // 움직임 키
    public InputAction cancelAction;    // 채팅 취소 키
    public InputAction chatEnterAction; // 채팅확정 키
    public InputAction interactAction;  // NPC 상호작용 키

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

    private void OnEnable()
    {
        moveAction.Enable();
        cancelAction.Enable();
        chatEnterAction.Enable();
        interactAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        cancelAction.Disable();
        chatEnterAction.Disable();
        interactAction.Disable();
    }
}
