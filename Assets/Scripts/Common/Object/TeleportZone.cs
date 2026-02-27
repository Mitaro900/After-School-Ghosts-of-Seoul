using UnityEngine;
public enum TeleportType
{
    Door,
    Stair
}

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private TeleportType teleportType;
    [SerializeField] private TeleportZone linkedZone;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BoxCollider2D newBounds;   // 카메라 제한 구역
    [SerializeField] protected GameObject pressE;

    public void Teleport(Player player)
    {
        float randomPitch = Random.Range(1f, 1.3f);

        switch (teleportType)
        {
            case TeleportType.Door:
                AudioManager.Instance.PlaySFXWithPitch(SFX.door_open, randomPitch);
                break;

            case TeleportType.Stair:
                AudioManager.Instance.PlaySFXWithPitch(SFX.stair_move, randomPitch);
                break;
        }

        player.transform.position = linkedZone.spawnPoint.position;

        CameraManager.Instance.SetBounds(newBounds);
        CameraManager.Instance.FollowCamera(player.transform);

        player.SetCurrentTeleport(linkedZone);
    }

    // E키를 눌러주세요 활성화
    public virtual void ShowPressEkeyUI()
    {
        pressE.SetActive(true);
    }

    // E키를 눌러주세요 비활성화
    public virtual void HidePressEkeyUI()
    {
        pressE.SetActive(false);
    }
}
