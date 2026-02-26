using Singleton.Component;
using UnityEngine;

public class CameraManager : SingletonComponent<CameraManager>
{
    [SerializeField] private Camera cam;
    private Transform Target;

    private float minX, maxX, minY, maxY;
    private bool hasBounds = false;

    private float camHeight;
    private float camWidth;

    #region Singleton
    protected override void AwakeInstance()
    {
        camHeight = cam.orthographicSize;
        camWidth = camHeight * cam.aspect;
    }

    protected override bool InitInstance()
    {
        return true;
    }

    protected override void ReleaseInstance()
    {
        
    }
    #endregion

    private void LateUpdate()
    {
        if (Target == null) return;

        float targetX = Target.position.x;
        float targetY = Target.position.y;

        if (hasBounds)
        {
            targetX = Mathf.Clamp(targetX, minX + camWidth, maxX - camWidth);
            targetY = Mathf.Clamp(targetY, minY + camHeight, maxY - camHeight);
        }

        cam.transform.position = new Vector3(targetX, targetY, cam.transform.position.z);
    }

    public void FollowCamera(Transform target)
    {
        Target = target;
    }

    // 구역 변경 시 호출
    public void SetBounds(BoxCollider2D bounds)
    {
        Bounds b = bounds.bounds;

        minX = b.min.x;
        maxX = b.max.x;
        minY = b.min.y;
        maxY = b.max.y;

        hasBounds = true;
    }

    public void ForceMoveToTarget()
    {
        if (Target == null) return;

        cam.transform.position = new Vector3(
            Target.position.x,
            Target.position.y,
            cam.transform.position.z
        );
    }
}
