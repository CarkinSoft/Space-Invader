using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 50;
    public bool isEnemyBullet = false;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (isEnemyBullet)
        {
            rb.linearVelocity = Vector2.down * speed;
            gameObject.layer = LayerMask.NameToLayer("EnemyBullet");
        }
        else
        {
            rb.linearVelocity = Vector2.up * speed;
            gameObject.layer = LayerMask.NameToLayer("Bullet");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Enemy bullets ignore other enemies
        if (isEnemyBullet)
        {
            // // Ignore collisions with enemies
            // if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            // {
            //     return;
            // }

            if (collision.gameObject.CompareTag("Enemy"))
                return;

        // Hit player
            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = collision.gameObject.GetComponent<Player>();
                if (player != null)
                {
                    player.Die();
                }
                Destroy(gameObject);
            }
        }
    }
}
