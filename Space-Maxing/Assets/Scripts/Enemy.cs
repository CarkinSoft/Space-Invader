using UnityEngine;

public enum EnemyType
{
    Type1 = 10,  // Bottom row - 10 points
    Type2 = 20,  // Middle row - 20 points
    Type3 = 30,  // Top row - 30 points
    Type4 = 40   // UFO/Special - 40 points
}

public class Enemy : MonoBehaviour
{
    public delegate void EnemyDiedFunc(float points);
    public static event EnemyDiedFunc OnEnemyDied;

    public AudioClip ticClip;
    public AudioClip tocClip;
    public EnemyType enemyType = EnemyType.Type1;

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Ouch!");

        // Destroy enemy if hit by player bullet
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Destroy(collision.gameObject);

            // Trigger death animation if exists
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Death");
            }

            // Notify with score based on enemy type
            OnEnemyDied?.Invoke((int)enemyType);

            Destroy(gameObject, 0.1f);
        }
    }

    public void PlayTicSound()
    {
        Debug.Log("Tic");
        GetComponent<AudioSource>().PlayOneShot(ticClip);
    }

    public void PlayTocSound()
    {
        Debug.Log("Toc");
        GetComponent<AudioSource>().PlayOneShot(tocClip);
    }
}
