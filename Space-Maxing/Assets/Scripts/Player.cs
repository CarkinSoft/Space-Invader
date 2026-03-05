using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootOffsetTransform;

    [Header("Movement")]
    public float moveSpeed = 6f;

    [Tooltip("World-space horizontal clamp (because you move left/right on screen).")]
    public float minX = -7.5f;

    [Tooltip("World-space horizontal clamp (because you move left/right on screen).")]
    public float maxX = 7.5f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameObject shot = Instantiate(bulletPrefab, shootOffsetTransform.position, Quaternion.identity);
            Debug.Log("Bang!");

            Destroy(shot, 3f);

            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }
        }
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        float inputX = 0f;
        if (Keyboard.current.aKey.isPressed) inputX -= 1f;
        if (Keyboard.current.dKey.isPressed) inputX += 1f;

        if (Mathf.Approximately(inputX, 0f)) return;

        // Player is rotated so local up = world-left and local down = world-right.
        // Using -transform.up makes inputX=+1 (D) move world-right.
        Vector3 delta = (-transform.up) * (inputX * moveSpeed * Time.deltaTime);

        Vector3 newPos = transform.position + delta;

        // Clamp horizontally in world space (screen left/right)
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);

        transform.position = newPos;
    }

    public void Die()
    {
        Debug.Log("Player died!");

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        Invoke("ReloadScene", 2f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
