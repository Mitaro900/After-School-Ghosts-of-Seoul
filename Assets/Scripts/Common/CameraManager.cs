using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] private Camera cam;
    private Transform Target;


    private void LateUpdate()
    {
        // 타겟쪽으로 카메라가 따라감
        if (Target != null)
            cam.transform.position = new Vector3(Target.position.x, Target.position.y, cam.transform.position.z);
    }

    public void FollowCamera(Transform target)
    {
        Target = target;
    }
}
