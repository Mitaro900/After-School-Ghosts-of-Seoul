using UnityEngine;

public class MapZone : MonoBehaviour
{
    [SerializeField] private BoxCollider2D cameraBounds;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager.Instance.SetBounds(cameraBounds);
        }
    }
}
