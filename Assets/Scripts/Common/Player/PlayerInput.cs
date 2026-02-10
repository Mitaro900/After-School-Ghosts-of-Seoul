using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Player Input")]
    public InputAction moveAction;
    public InputAction cancelAction;
    public InputAction interactAction;



    private void OnEnable()
    {
        moveAction.Enable();
        cancelAction.Enable();
        interactAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        cancelAction.Disable();
        interactAction.Disable();
    }
}
