using UnityEngine;

public class Barricade : MonoBehaviour
{
    public int health = 5;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if hit by any bullet (player or enemy)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet") ||
            collision.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
        {
            // Destroy the bullet
            Destroy(collision.gameObject);

            // Take damage
            TakeDamage();
        }
    }

    void TakeDamage()
    {
        health--;

        // Update visual - make more transparent as health decreases
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = health / 5f;
            spriteRenderer.color = color;
        }

        // Destroy if health depleted
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
