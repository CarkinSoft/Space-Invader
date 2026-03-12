using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootOffsetTransform;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float minX = -7.5f;
    public float maxX = 7.5f;

    [Header("Audio")]
    [Tooltip("Sound played when the player fires a bullet.")]
    public AudioClip shootClip;

    [Tooltip("Sound played when the player is destroyed.")]
    public AudioClip explodeClip;

    private Animator animator;
    private AudioSource audioSource;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject shot = Instantiate(bulletPrefab, shootOffsetTransform.position, Quaternion.identity);
        Destroy(shot, 3f);

        // Play shoot sound
        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip);

        if (animator != null)
            animator.SetTrigger("Shoot");
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        float inputX = 0f;
        if (Keyboard.current.aKey.isPressed) inputX -= 1f;
        if (Keyboard.current.dKey.isPressed) inputX += 1f;

        if (Mathf.Approximately(inputX, 0f)) return;

        Vector3 delta = (-transform.up) * (inputX * moveSpeed * Time.deltaTime);
        Vector3 newPos = transform.position + delta;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        transform.position = newPos;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");

        // Play explosion sound
        if (audioSource != null && explodeClip != null)
            AudioSource.PlayClipAtPoint(explodeClip, transform.position);

        if (animator != null)
            animator.SetTrigger("Death");

        // Disable controls
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Destroy player after animation plays (or just hide it)
        Destroy(gameObject, 0.5f);

        // Tell GameManager — it will route to Credits after a delay
        if (GameManager.Instance != null)
            GameManager.Instance.NotifyPlayerDied();
    }
}
