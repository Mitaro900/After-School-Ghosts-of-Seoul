using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float xMoveSpeed = 5f;
    [SerializeField] private float yMoveSpeed = 3f;


    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;



    private void Update()
    {
        Move();
    }

    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 pos = transform.position;

        if (x != 0)
        {
            Vector3 xMove = new Vector3(x * xMoveSpeed * Time.deltaTime, 0f, 0f);
            Vector3 nextXPos = pos + xMove;

            bool xGround =
                Physics2D.Raycast(nextXPos + Vector3.left * 0.2f, Vector2.down, 0f, groundLayer) ||
                Physics2D.Raycast(nextXPos + Vector3.right * 0.2f, Vector2.down, 0f, groundLayer);

            if (xGround)
                pos.x = nextXPos.x;   // 좌우만 이동
        }

        if (y != 0)
        {
            Vector3 yMove = new Vector3(0f, y * yMoveSpeed * Time.deltaTime, 0f);
            Vector3 nextXPos = pos + yMove;

            bool yGround =
                Physics2D.Raycast(nextXPos + Vector3.left * 0.2f, Vector2.down, 0f, groundLayer) ||
                Physics2D.Raycast(nextXPos + Vector3.right * 0.2f, Vector2.down, 0f, groundLayer);

            if (yGround)
                pos.y = nextXPos.y;   // 상하만 이동
        }

        transform.position = pos;
    }
}
