using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    // UI를 엽니다.
    public void OpenUI(GameObject ui)
    {
        ui.SetActive(true);
    }

    public void CloseUI(GameObject ui)
    {
        ui.SetActive(false);
    }
}
