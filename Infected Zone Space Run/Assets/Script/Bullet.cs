using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Kena Player
        if (other.CompareTag("Player"))
        {
            // player.TakeDamage(damage);
            Destroy(gameObject);
        }

        // Kena Ground
        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}