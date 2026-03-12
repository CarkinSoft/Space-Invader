using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [Header("Movement Settings")] public float stepDistance = 0.5f;
    public float stepDownDistance = 0.5f;
    public float initialStepInterval = 1f;
    public float minStepInterval = 0.1f;

    [Header("Shooting Settings")] public GameObject enemyBulletPrefab;
    public float shootInterval = 2f;

    [Header("Wave End")] [Tooltip("Set to 0 to go to Credits immediately when all enemies are dead (no respawn).")]
    public float respawnDelaySeconds = 0f;

    [Header("UFO (Enemy Type 4)")] public GameObject ufoPrefab;
    public float ufoMinSpawnTime = 8f;
    public float ufoMaxSpawnTime = 18f;
    public float ufoSpawnY = 4.2f;
    public float ufoSpawnX = 9.5f;

    private float currentStepInterval;
    private float stepTimer;
    private float shootTimer;
    private int direction = 1;
    private int initialEnemyCount;
    private List<Enemy> enemies = new List<Enemy>();

    private bool waveCleared;
    private float ufoTimer;

    private Vector3 initialGroupPosition;

    private readonly List<GameObject> enemyTemplates = new List<GameObject>();
    private readonly List<Vector3> enemyTemplateLocalPositions = new List<Vector3>();
    private readonly List<Quaternion> enemyTemplateLocalRotations = new List<Quaternion>();
    private readonly List<Vector3> enemyTemplateLocalScales = new List<Vector3>();

    void Start()
    {
        initialGroupPosition = transform.position;
        CacheEnemyTemplatesFromChildren();
        RespawnWaveImmediate();

        stepTimer = currentStepInterval;
        shootTimer = shootInterval;

        Enemy.OnEnemyDied += OnEnemyDestroyed;
        ResetUfoTimer();
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDied -= OnEnemyDestroyed;

        foreach (var t in enemyTemplates)
            if (t != null)
                Destroy(t);
    }

    void Update()
    {
        enemies.RemoveAll(e => e == null);

        if (enemies.Count == 0)
        {
            if (!waveCleared)
            {
                waveCleared = true;

                // If respawnDelaySeconds == 0, treat clearing the wave as a WIN → Credits
                if (respawnDelaySeconds <= 0f)
                {
                    if (GameManager.Instance != null)
                        GameManager.Instance.NotifyAllEnemiesKilled();
                }
                else
                {
                    StartCoroutine(RespawnAfterDelay(respawnDelaySeconds));
                }
            }

            return;
        }

        if (ufoPrefab != null)
        {
            ufoTimer -= Time.deltaTime;
            if (ufoTimer <= 0f)
            {
                SpawnUfo();
                ResetUfoTimer();
            }
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0)
        {
            MoveStep();
            stepTimer = currentStepInterval;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            ShootRandomBullet();
            shootTimer = shootInterval;
        }
    }

    private IEnumerator RespawnAfterDelay(float seconds)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
        RespawnWaveImmediate();
        waveCleared = false;
    }

    private void CacheEnemyTemplatesFromChildren()
    {
        enemyTemplates.Clear();
        enemyTemplateLocalPositions.Clear();
        enemyTemplateLocalRotations.Clear();
        enemyTemplateLocalScales.Clear();

        Enemy[] startingEnemies = GetComponentsInChildren<Enemy>();

        foreach (Enemy e in startingEnemies)
        {
            if (e == null) continue;
            Transform t = e.transform;
            enemyTemplateLocalPositions.Add(t.localPosition);
            enemyTemplateLocalRotations.Add(t.localRotation);
            enemyTemplateLocalScales.Add(t.localScale);

            GameObject template = Instantiate(e.gameObject, transform);
            template.name = e.gameObject.name + "_TEMPLATE";
            template.SetActive(false);
            enemyTemplates.Add(template);
        }

        foreach (Enemy e in startingEnemies)
            if (e != null)
                Destroy(e.gameObject);
    }

    private void RespawnWaveImmediate()
    {
        transform.position = initialGroupPosition;
        direction = 1;

        Enemy[] leftover = GetComponentsInChildren<Enemy>();
        foreach (var l in leftover)
            if (l != null)
                Destroy(l.gameObject);

        enemies.Clear();

        for (int i = 0; i < enemyTemplates.Count; i++)
        {
            GameObject template = enemyTemplates[i];
            if (template == null) continue;

            GameObject spawned = Instantiate(template, transform);
            spawned.name = template.name.Replace("_TEMPLATE", "");
            spawned.SetActive(true);

            Transform st = spawned.transform;
            st.localPosition = enemyTemplateLocalPositions[i];
            st.localRotation = enemyTemplateLocalRotations[i];
            st.localScale = enemyTemplateLocalScales[i];

            Enemy enemy = spawned.GetComponent<Enemy>();
            if (enemy != null) enemies.Add(enemy);
        }

        initialEnemyCount = enemies.Count;
        currentStepInterval = initialStepInterval;
        stepTimer = currentStepInterval;
        shootTimer = shootInterval;
    }

    private void ResetUfoTimer()
    {
        ufoTimer = Random.Range(ufoMinSpawnTime, ufoMaxSpawnTime);
    }

    private void SpawnUfo()
    {
        bool fromLeft = Random.value < 0.5f;
        float x = fromLeft ? -ufoSpawnX : ufoSpawnX;
        GameObject ufo = Instantiate(ufoPrefab, new Vector3(x, ufoSpawnY, 0f), Quaternion.identity);

        UfoMover mover = ufo.GetComponent<UfoMover>();
        if (mover != null)
            mover.Init(fromLeft ? Vector2.right : Vector2.left);
    }

    void MoveStep()
    {
        float leftmost = float.MaxValue;
        float rightmost = float.MinValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            float x = enemy.transform.position.x;
            if (x < leftmost) leftmost = x;
            if (x > rightmost) rightmost = x;
        }

        bool shouldMoveDown = false;
        if (direction > 0 && rightmost >= 4f)
        {
            shouldMoveDown = true;
            direction = -1;
        }
        else if (direction < 0 && leftmost <= -4f)
        {
            shouldMoveDown = true;
            direction = 1;
        }

        if (shouldMoveDown)
            transform.position += Vector3.down * stepDownDistance;
        else
            transform.position += Vector3.right * (stepDistance * direction);

        PlayStepSound();
    }

    void PlayStepSound()
    {
        if (enemies.Count > 0 && enemies[0] != null)
        {
            if (direction > 0) enemies[0].PlayTicSound();
            else enemies[0].PlayTocSound();
        }
    }

    void OnEnemyDestroyed(float points)
    {
        enemies.RemoveAll(e => e == null);
        if (initialEnemyCount <= 0) return;
        float ratio = (float)enemies.Count / initialEnemyCount;
        currentStepInterval = Mathf.Lerp(minStepInterval, initialStepInterval, ratio);
    }

    void ShootRandomBullet()
    {
        if (enemies.Count == 0 || enemyBulletPrefab == null) return;

        Enemy shooter = enemies[Random.Range(0, enemies.Count)];
        if (shooter == null) return;

        GameObject bullet = Instantiate(enemyBulletPrefab, shooter.transform.position, Quaternion.identity);
        Destroy(bullet, 5f);

        // Play enemy shoot sound on the shooter
        shooter.PlayShootSound();

        shooter.PlayShootAnimation(); // Trigger shoot animation
    }
}
