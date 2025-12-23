using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Attack, Dead }
    public EnemyState currentState = EnemyState.Idle;

    [Header("References")]
    public Transform player;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Animator animator;
    public Collider2D enemyCollider;

    [Header("Attack Settings")]
    public float attackRange = 4f;
    public float shootForce = 8f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private bool facingRight = true;
    private int health = 3;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (enemyCollider == null)
            enemyCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (currentState == EnemyState.Dead || player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Flip sesuai posisi player
        HandleFlip();

        if (distance <= attackRange)
            currentState = EnemyState.Attack;
        else
            currentState = EnemyState.Idle;

        if (currentState == EnemyState.Attack)
            AttackPlayer();
    }

    void HandleFlip()
    {
        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetTrigger("Shoot");
            Shoot();
            lastAttackTime = Time.time;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 dir = (player.position - firePoint.position).normalized;
            rb.linearVelocity = dir * shootForce;
        }
    }

    // Dipanggil saat terkena peluru player
    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead) return;

        health -= damage;

        if (health <= 0)
            Die();
    }

    void Die()
    {
        currentState = EnemyState.Dead;
        animator.SetTrigger("Die");

        // Player bisa lewat
        enemyCollider.enabled = false;

        // Optional: destroy setelah animasi
        Destroy(gameObject, 1.5f);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Visual jarak serang
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}