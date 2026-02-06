using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera cam;
    private Transform currentTarget;


    private void LateUpdate()
    {
        if (currentTarget != null)
            cam.transform.position = new Vector3(currentTarget.position.x, currentTarget.position.y, cam.transform.position.z);
    }

    public void FollowCamera(Transform target)
    {
        currentTarget = target;
    }
}
