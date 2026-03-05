using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform shootOffsetTransform;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameObject shot = Instantiate(bulletPrefab, shootOffsetTransform.position, Quaternion.identity);
            Debug.Log("Bang!");

            // Destroy the bullet after 3 seconds
            Destroy(shot, 3f);

            // Trigger shoot animation
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }
        }
    }

    public void Die()
    {
        Debug.Log("Player died!");

        // Trigger death animation if exists
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        // Reload scene or show game over after delay
        Invoke("ReloadScene", 2f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
