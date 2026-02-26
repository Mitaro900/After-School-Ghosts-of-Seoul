using System.Collections;
using TMPro;
using UnityEngine;

public class Player : BaseChat
{
    [Header("Player Info")]
    public Sprite playerProfile;


    [Header("Move")]
    private Animator anim;
    [SerializeField] private float xMoveSpeed = 5f;
    [SerializeField] private float yMoveSpeed = 3f;
    private Vector2 lastMoveDir = Vector2.down;
    private bool isMove = true;

    [Header("Item Info")]
    [SerializeField] private Item currentItem;
    [SerializeField] private GameObject itemChatUI;
    [SerializeField] private TextMeshProUGUI itemChatText;

    [Header("Object Info")]
    [SerializeField] private QuestObject currentObject;
    private TeleportZone currentTeleprot;

    private CanvasGroup itemChatCG;
    private NPC currentNPC;
    private Coroutine itemChatCoroutine;
    private Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (itemChatUI != null)
            itemChatCG = itemChatUI.GetComponent<CanvasGroup>();
        GameManager.Instance.SetPlayer(this);
        CutsceneController.Instance.PlayCutscene(CutsceneType.Intro);
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

        if (moveInput != Vector2.zero)
        {
            lastMoveDir = moveInput;
            anim.SetBool("isMove", true);
        }
        else { anim.SetBool("isMove", false); }


        anim.SetFloat("Xinput", lastMoveDir.x);
        anim.SetFloat("Yinput", lastMoveDir.y);
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

        if (col.CompareTag("Object"))
        {
            currentObject = col.GetComponent<QuestObject>();
            currentObject.ShowPressEkeyUI();
        }

        if (col.CompareTag("Teleport"))
        {
            currentTeleprot = col.GetComponent<TeleportZone>();
            currentTeleprot.ShowPressEkeyUI();
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

        if (col.CompareTag("Object"))
        {
            currentObject.HidePressEkeyUI();
            currentObject = null;
        }

        if (col.CompareTag("Teleport"))
        {
            TeleportZone zone = col.GetComponent<TeleportZone>();

            if (currentTeleprot == zone)
            {
                zone.HidePressEkeyUI();
                currentTeleprot = null;
            }
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
                PlayItemChat();
            }

            if (currentObject != null)
            {
                string text = currentObject.GetItemPrompt();
                itemChatText.text = text;
                PlayItemChat();
            }

            if (currentTeleprot != null)
            {
                currentTeleprot.Teleport(this);
            }
        }
    }

    // InventoryManager에서 슬롯 클릭시 사용함
    public void TalkToSlotItem(string text)
    {
        itemChatText.text = text;
        PlayItemChat();
    }

    public void PlayItemChat()
    {
        if (itemChatCoroutine != null)
        {
            StopCoroutine(itemChatCoroutine);
        }

        itemChatCoroutine = StartCoroutine(ShowItemChat());
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

    public void SetCurrentTeleport(TeleportZone zone)
    {
        currentTeleprot = zone;
    }
}
