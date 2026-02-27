using UnityEngine;

public class IntroUI : MonoBehaviour
{
    private void Awake()
    {
        CutsceneController.Instance.PlayCutscene(CutsceneType.Intro);
    }

}
