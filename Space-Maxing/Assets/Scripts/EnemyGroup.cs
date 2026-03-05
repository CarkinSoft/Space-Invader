using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [Header("Movement Settings")]
    public float stepDistance = 0.5f;
    public float stepDownDistance = 0.5f;
    public float initialStepInterval = 1f;
    public float minStepInterval = 0.1f;

    [Header("Shooting Settings")]
    public GameObject enemyBulletPrefab;
    public float shootInterval = 2f;

    [Header("Wave End")]
    public float respawnDelaySeconds = 1.5f;

    [Header("UFO (Enemy Type 4)")]
    public GameObject ufoPrefab;
    public float ufoMinSpawnTime = 8f;
    public float ufoMaxSpawnTime = 18f;
    public float ufoSpawnY = 4.2f;
    public float ufoSpawnX = 9.5f;

    private float currentStepInterval;
    private float stepTimer;
    private float shootTimer;
    private int direction = 1; // 1 = right, -1 = left
    private int initialEnemyCount;
    private List<Enemy> enemies = new List<Enemy>();

    private bool respawnQueued;
    private float ufoTimer;

    private Vector3 initialGroupPosition;

    private readonly List<GameObject> enemyTemplates = new List<GameObject>();
    private readonly List<Vector3> enemyTemplateLocalPositions = new List<Vector3>();
    private readonly List<Quaternion> enemyTemplateLocalRotations = new List<Quaternion>();
    private readonly List<Vector3> enemyTemplateLocalScales = new List<Vector3>();

    void Start()
    {
        initialGroupPosition = transform.position;

        // Cache the starting enemies as templates (inactive clones)
        CacheEnemyTemplatesFromChildren();

        // Spawn the initial wave based on templates (keeps behavior consistent)
        RespawnWaveImmediate();

        stepTimer = currentStepInterval;
        shootTimer = shootInterval;

        Enemy.OnEnemyDied += OnEnemyDestroyed;

        ResetUfoTimer();
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDied -= OnEnemyDestroyed;

        // Cleanup templates we created at runtime
        for (int i = 0; i < enemyTemplates.Count; i++)
        {
            if (enemyTemplates[i] != null)
                Destroy(enemyTemplates[i]);
        }
    }

    void Update()
    {
        enemies.RemoveAll(e => e == null);

        if (enemies.Count == 0)
        {
            if (!respawnQueued)
            {
                respawnQueued = true;
                StartCoroutine(RespawnAfterDelay(respawnDelaySeconds));
            }
            return;
        }

        // UFO spawn timer (keeps happening during the wave)
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
        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);

        RespawnWaveImmediate();
        respawnQueued = false;
    }

    private void CacheEnemyTemplatesFromChildren()
    {
        enemyTemplates.Clear();
        enemyTemplateLocalPositions.Clear();
        enemyTemplateLocalRotations.Clear();
        enemyTemplateLocalScales.Clear();

        // Grab the enemies currently placed as children in the scene
        Enemy[] startingEnemies = GetComponentsInChildren<Enemy>();

        foreach (Enemy e in startingEnemies)
        {
            if (e == null) continue;

            Transform t = e.transform;

            enemyTemplateLocalPositions.Add(t.localPosition);
            enemyTemplateLocalRotations.Add(t.localRotation);
            enemyTemplateLocalScales.Add(t.localScale);

            // Make an inactive clone to use as a runtime "prefab"
            GameObject template = Instantiate(e.gameObject, transform);
            template.name = e.gameObject.name + "_TEMPLATE";
            template.SetActive(false);

            enemyTemplates.Add(template);
        }

        // Remove the originally placed enemies (we'll spawn clean instances from templates)
        foreach (Enemy e in startingEnemies)
        {
            if (e != null)
                Destroy(e.gameObject);
        }
    }

    private void RespawnWaveImmediate()
    {
        // Reset group state
        transform.position = initialGroupPosition;
        direction = 1;

        // Destroy any remaining children that are enemies (safety)
        Enemy[] leftover = GetComponentsInChildren<Enemy>();
        for (int i = 0; i < leftover.Length; i++)
        {
            if (leftover[i] != null)
                Destroy(leftover[i].gameObject);
        }

        enemies.Clear();

        // Spawn a fresh wave
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
            if (enemy != null)
                enemies.Add(enemy);
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
        Vector3 spawnPos = new Vector3(x, ufoSpawnY, 0f);

        GameObject ufo = Instantiate(ufoPrefab, spawnPos, Quaternion.identity);

        UfoMover mover = ufo.GetComponent<UfoMover>();
        if (mover != null)
        {
            Vector2 dir = fromLeft ? Vector2.right : Vector2.left;
            mover.Init(dir);
        }
    }

    void MoveStep()
    {
        float leftmost = float.MaxValue;
        float rightmost = float.MinValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                float x = enemy.transform.position.x;
                if (x < leftmost) leftmost = x;
                if (x > rightmost) rightmost = x;
            }
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
        if (shooter != null)
        {
            Vector3 spawnPos = shooter.transform.position;
            GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
            Destroy(bullet, 5f);
        }
    }
}
