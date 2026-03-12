using UnityEngine;

public class UfoMover : MonoBehaviour
{
    public float speed = 6f;
    public float destroyWhenPastX = 10f;

    [Header("Audio")]
    public AudioClip explodeClip;

    private Vector2 direction = Vector2.right;
    private bool isDead = false;

    public void Init(Vector2 moveDirection)
    {
        direction = moveDirection.normalized;
    }

    void Update()
    {
        if (isDead) return;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x) > destroyWhenPastX)
            Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Destroy(collision.gameObject);
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // Play explosion sound
        if (explodeClip != null)
            AudioSource.PlayClipAtPoint(explodeClip, transform.position);

        // Trigger death animation
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // Check if Death trigger exists
            foreach (var param in anim.parameters)
            {
                if (param.name == "Death" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.SetTrigger("Death");
                    break;
                }
            }
        }

        // Disable collider so it can't be hit again
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Award points (UFOs are worth 40 points - Type4)
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(40);

        // Destroy after animation plays
        Destroy(gameObject, 0.5f);
    }
}