using UnityEngine;

public enum EnemyType
{
    Type1 = 10,
    Type2 = 20,
    Type3 = 30,
    Type4 = 40
}

public class Enemy : MonoBehaviour
{
    public delegate void EnemyDiedFunc(float points);
    public static event EnemyDiedFunc OnEnemyDied;

    [Header("Movement Sounds")]
    public AudioClip ticClip;
    public AudioClip tocClip;

    [Header("Combat Sounds")]
    [Tooltip("Sound played when this enemy fires a bullet.")]
    public AudioClip shootClip;

    [Tooltip("Sound played when this enemy is destroyed.")]
    public AudioClip explodeClip;

    public EnemyType enemyType = EnemyType.Type1;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Destroy(collision.gameObject);

            // Play explode sound on a new temp object so it survives the Destroy
            if (explodeClip != null)
                AudioSource.PlayClipAtPoint(explodeClip, transform.position);

            Animator anim = GetComponent<Animator>();
            if (anim != null)
                anim.SetTrigger("Death");

            OnEnemyDied?.Invoke((int)enemyType);

            Destroy(gameObject, 0.5f);  // Give time for death animation
        }
    }

    /// <summary>
    /// Called by EnemyGroup right after instantiating an enemy bullet so the
    /// enemy plays its shoot sound.
    /// </summary>
    public void PlayShootSound()
    {
        if (audioSource != null && shootClip != null)
            audioSource.PlayOneShot(shootClip);
    }

    public void PlayTicSound()
    {
        if (audioSource != null && ticClip != null)
            audioSource.PlayOneShot(ticClip);
    }

    public void PlayTocSound()
    {
        if (audioSource != null && tocClip != null)
            audioSource.PlayOneShot(tocClip);
    }

    public void PlayShootAnimation()
    {
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            // Only trigger if the animator has this parameter
            foreach (var param in anim.parameters)
            {
                if (param.name == "Shoot" && param.type == AnimatorControllerParameterType.Trigger)
                {
                    anim.SetTrigger("Shoot");
                    return;
                }
            }
        }
    }
}
