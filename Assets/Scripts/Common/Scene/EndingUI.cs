using UnityEngine;

public class EndingUI : MonoBehaviour
{
    private void Awake()
    {
        CutsceneController.Instance.PlayCutscene(CutsceneType.Ending);
    }
}
