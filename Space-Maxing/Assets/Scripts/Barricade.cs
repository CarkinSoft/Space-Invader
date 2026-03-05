using UnityEngine;

public class Barricade : MonoBehaviour
{
    public int health = 5;
    public SpriteRenderer spriteRenderer;

    [Header("Damage Visuals")]
    [Tooltip("How much smaller the barricade gets per hit (multiplicative).")]
    [Range(0.5f, 0.98f)]
    public float scaleMultiplierPerHit = 0.9f;

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

        // Shrink a bit each hit
        transform.localScale *= scaleMultiplierPerHit;

        // Optional: also fade slightly (nice feedback)
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Mathf.Clamp01(health / 5f);
            spriteRenderer.color = color;
        }

        // Destroy if health depleted
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
