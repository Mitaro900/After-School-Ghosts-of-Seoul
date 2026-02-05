using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    // UI¸¦ ¿±´Ï´Ù
    public void OpenUI(GameObject ui)
    {
        ui.SetActive(true);
    }

    public void CloseUI(GameObject ui)
    {
        ui.SetActive(false);
    }
}
