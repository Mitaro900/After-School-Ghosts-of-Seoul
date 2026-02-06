using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float xMoveSpeed = 5f; // 좌우 속도
    [SerializeField] private float yMoveSpeed = 3f; // 상하 속도
    private bool isMove = true;


    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer; // 체크할 땅


    [Header("NPC Info")]
    [SerializeField] private NPC currentNPC;


    private BoxCollider2D boxCol;   // Ground를 벗어나지 않을 콜라이더



    private void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
        GameManager.Instance.SetPlayer(this);  // 게임매니저에게 카메라 타겟을 Player로 지정
    }


    private void Update()
    {
        // E키 누를시 NPC와 대화 (움직임 차단)
        if (currentNPC != null && Input.GetKeyDown(KeyCode.E))
        {
            currentNPC.OnInteract();
            isMove = false;
        }

        if (isMove)
        Move();
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        // NPC 감지시 E키를 눌러주세요 나타남
        if (col.CompareTag("NPC"))
        {
            currentNPC = col.GetComponent<NPC>();
            currentNPC.ShowPressEkeyUI();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // NPC 감지시 E키를 눌러주세요 숨김
        if (col.CompareTag("NPC"))
        {
            NPC npc = col.GetComponent<NPC>();
            npc.HidePressEkeyUI();
            currentNPC = null;
        }
    }






    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 moveInput = new Vector2(x, y);

        if (moveInput.sqrMagnitude > 1f)    // 대각선 이동시 빠르지 1f로 고정하여 속도가 더 빠르지 않게함
            moveInput.Normalize();

        Vector3 pos = transform.position;   // 플레이어 중심 위치

        Vector2 halfSize = boxCol.bounds.extents;   // 박스 콜라이더의 전체크기의 중심에서 가장자리까지의 거리
        Vector2 boxSize = boxCol.bounds.size;   // 현재 박스 콜라이더 사이즈


        // 좌우에 gorund가 없다면 멈춤
        if (moveInput.x != 0)
        {
            float moveX = moveInput.x * xMoveSpeed * Time.deltaTime;
            Vector3 nextPos = pos + new Vector3(moveX, 0, 0);   //  이동했을때 도착하는 중심 위치

            Vector2 checkPos = new Vector2(nextPos.x + halfSize.x * Mathf.Sign(x), nextPos.y);   // 도착하는 중심 위치 + 박스콜라이더 가장자리 * 방향 정규화, 현재 y위치 (예정)

            Collider2D hit = Physics2D.OverlapBox(checkPos, new Vector2(0.05f, boxSize.y), 0f, groundLayer); // 콜라이더 옆에 0.05f box를 만들어 감지시킴

            if (hit != null) 
                pos.x = nextPos.x;
        }

        // 상하에 gorund가 없다면 멈춤
        if (moveInput.y != 0)
        {
            float moveY = moveInput.y * yMoveSpeed * Time.deltaTime;
            Vector3 nextPos = pos + new Vector3(0, moveY, 0);

            Vector2 checkPos = new Vector2(nextPos.x, nextPos.y + halfSize.y * Mathf.Sign(y));

            Collider2D hit = Physics2D.OverlapBox(checkPos, new Vector2(boxSize.x, 0.05f), 0f, groundLayer);

            if (hit != null)
                pos.y = nextPos.y;
        }

        transform.position = pos;
    }
}
