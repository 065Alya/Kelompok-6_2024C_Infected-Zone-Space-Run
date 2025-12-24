using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float runSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Stats")]
    public int maxLives = 2;              // Kesempatan hidup kembali
    public int currentLives;
    public float maxExp = 100f;
    public float currentExp;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float downDuration = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("References")]
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator animator;

    private Vector2 moveInput;
    private bool facingRight = true;
    private bool isGrounded;
    private float currentSpeed;
    private bool isDown;
    private bool isInvincible;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        currentLives = maxLives;
        currentExp = maxExp;
    }

    void Update()
    {
        if (isDown) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        currentSpeed = Mathf.Abs(moveInput.x) > 0.1f ? runSpeed : moveSpeed;

        animator.SetBool("isRunning", Mathf.Abs(moveInput.x) > 0.1f);
        animator.SetBool("isIdle", Mathf.Abs(moveInput.x) <= 0.1f && isGrounded);
        animator.SetBool("isSalto", !isGrounded);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (isDown) return;

        moveInput = context.ReadValue<Vector2>();

        if (moveInput.x > 0 && !facingRight) Flip();
        else if (moveInput.x < 0 && facingRight) Flip();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded && !isDown)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("isSalto", true);
        }
    }

    void FixedUpdate()
    {
        if (isDown) return;

        rb.linearVelocity = new Vector2(
            moveInput.x * currentSpeed,
            rb.linearVelocity.y
        );
    }

    // ================= DAMAGE LOGIC =================

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDown) return;

        currentExp -= damage;

        if (currentExp <= 0)
        {
            StartCoroutine(HandleDown());
        }
        else
        {
            animator.SetBool("isHurt", true);
            StartCoroutine(HurtCooldown());
        }
    }

    IEnumerator HandleDown()
    {
        isDown = true;
        animator.SetBool("isDown", true);
        rb.linearVelocity = Vector2.zero;
        playerInput.enabled = false;

        yield return new WaitForSeconds(downDuration);

        animator.SetBool("isDown", false);

        if (currentLives > 0)
        {
            Respawn();
        }
        else
        {
            GameOver();
        }
    }

    void Respawn()
    {
        currentLives--;
        currentExp = maxExp;

        transform.position = respawnPoint.position;
        playerInput.enabled = true;
        isDown = false;
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        // Panggil UI Game Over di sini
    }

    IEnumerator HurtCooldown()
    {
        isInvincible = true;
        yield return new WaitForSeconds(0.5f);
        animator.SetBool("isHurt", false);
        isInvincible = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
