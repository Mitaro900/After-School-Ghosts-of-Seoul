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
        // 씬 로드 후 cam이 없으면 Camera.main 사용
        if (cam == null)
            cam = Camera.main;

        if (cam != null)
        {
            camHeight = cam.orthographicSize;
            camWidth = camHeight * cam.aspect;
        }
    }

    protected override bool InitInstance() => true;
    protected override void ReleaseInstance() { }
    #endregion

    private void LateUpdate()
    {
        // cam이 null이면 씬에 새로 연결 시도
        if (Target == null) return;
        if (cam == null)
        {
            cam = Camera.main;
            if (cam != null)
            {
                camHeight = cam.orthographicSize;
                camWidth = camHeight * cam.aspect;
            }
            else
            {
                // 씬에 카메라가 없으면 더 이상 진행하지 않음
                return;
            }
        }

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
        if (Target == null || cam == null) return;

        cam.transform.position = new Vector3(
            Target.position.x,
            Target.position.y,
            cam.transform.position.z
        );
    }
}
