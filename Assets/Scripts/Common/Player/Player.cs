using System.Collections;
using TMPro;
using UnityEngine;

public class Player : BaseChat
{
    [Header("Move")]
    [SerializeField] private float xMoveSpeed = 5f;
    [SerializeField] private float yMoveSpeed = 3f;
    private bool isMove = true;

    [Header("Item Info")]
    [SerializeField] private Item currentItem;
    [SerializeField] private GameObject itemChatUI;
    [SerializeField] private TextMeshProUGUI itemChatText;

    private CanvasGroup itemChatCG;
    private NPC currentNPC;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (itemChatUI != null)
            itemChatCG = itemChatUI.GetComponent<CanvasGroup>();
        GameManager.Instance.SetPlayer(this);
    }

    protected override void Update()
    {
        TalkToObject();

        if (isMove)
            Move();
        else
            rb.linearVelocity = Vector2.zero;
    }

    private void Move()
    {
        Vector2 moveInput = PlayerInputManager.Instance.moveAction.ReadValue<Vector2>();

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        Vector2 velocity = new Vector2(
            moveInput.x * xMoveSpeed,
            moveInput.y * yMoveSpeed
        );

        rb.linearVelocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("NPC"))
        {
            currentNPC = col.GetComponent<NPC>();
            currentNPC.ShowPressEkeyUI();
        }

        if (col.CompareTag("Item"))
        {
            currentItem = col.GetComponent<Item>();
            currentItem.ShowPressEkeyUI();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("NPC"))
        {
            currentNPC.HidePressEkeyUI();
            currentNPC = null;
        }

        if (col.CompareTag("Item"))
        {
            currentItem.HidePressEkeyUI();
            currentItem = null;
        }
    }

    private void TalkToObject()
    {
        if (PlayerInputManager.Instance.interactAction.WasPressedThisFrame())
        {
            if (currentNPC != null)
            {
                currentNPC.OnInteract(this);
                isMove = false;
            }

            if (currentItem != null)
            {
                string text = currentItem.GetItemPrompt();
                itemChatText.text = text;
                StartCoroutine(ShowItemChat());
            }
        }
    }

    private IEnumerator ShowItemChat()
    {
        itemChatUI.SetActive(true);
        itemChatCG.alpha = 1f;

        yield return new WaitForSeconds(3f);

        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime;
            itemChatCG.alpha = Mathf.Lerp(1f, 0f, time);
            yield return null;
        }

        itemChatCG.alpha = 0f;
        itemChatUI.SetActive(false);
        itemChatCG.alpha = 1f;
    }

    public void PlayerMove(bool moveAllow)
    {
        isMove = moveAllow;

        if (!moveAllow)
            rb.linearVelocity = Vector2.zero;
    }
}
