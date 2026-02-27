using UnityEngine;

public class PlayerAnimEvent : MonoBehaviour
{
    private void WalkSound()
    {
        float randomPitch = Random.Range(0.8f, 1.3f);

        SFX[] walkSounds = { SFX.walk1, SFX.walk2, SFX.walk3, SFX.walk4 };

        int index = Random.Range(0, walkSounds.Length);
        AudioManager.Instance.PlaySFXWithPitch(walkSounds[index], randomPitch);
    }
}
