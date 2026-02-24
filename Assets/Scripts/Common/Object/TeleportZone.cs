using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private TeleportZone linkedZone;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private BoxCollider2D newBounds;   // 카메라 제한 구역
    [SerializeField] protected GameObject pressE;

    public void Teleport(Player player)
    {
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
