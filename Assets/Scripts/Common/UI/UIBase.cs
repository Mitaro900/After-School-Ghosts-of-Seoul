using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    protected bool isOpen = false;

    public virtual void Open()
    {
        if (isOpen) return;

        gameObject.SetActive(true);
        isOpen = true;
        OnOpen();
    }

    public virtual void Close()
    {
        if (!isOpen) return;

        OnClose();
        gameObject.SetActive(false);
        isOpen = false;
    }

    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }

    public bool IsOpen => isOpen;
}
