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

    private float currentStepInterval;
    private float stepTimer;
    private float shootTimer;
    private int direction = 1; // 1 = right, -1 = left
    private int initialEnemyCount;
    private List<Enemy> enemies = new List<Enemy>();

    void Start()
    {
        // Get all enemy children
        enemies.AddRange(GetComponentsInChildren<Enemy>());
        initialEnemyCount = enemies.Count;
        currentStepInterval = initialStepInterval;

        stepTimer = currentStepInterval;
        shootTimer = shootInterval;

        // Subscribe to enemy death events
        Enemy.OnEnemyDied += OnEnemyDestroyed;
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDied -= OnEnemyDestroyed;
    }

    void Update()
    {
        // Update enemies list (remove destroyed ones)
        enemies.RemoveAll(e => e == null);

        if (enemies.Count == 0) return;

        // Movement timer
        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0)
        {
            MoveStep();
            stepTimer = currentStepInterval;
        }

        // Shooting timer
        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0)
        {
            ShootRandomBullet();
            shootTimer = shootInterval;
        }
    }

    void MoveStep()
    {
        // Get the bounds of the enemy group
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

        // Check if we need to move down and reverse direction
        bool shouldMoveDown = false;
        if (direction > 0 && rightmost >= 8f) // Adjust boundary as needed
        {
            shouldMoveDown = true;
            direction = -1;
        }
        else if (direction < 0 && leftmost <= -8f) // Adjust boundary as needed
        {
            shouldMoveDown = true;
            direction = 1;
        }

        // Move the group
        if (shouldMoveDown)
        {
            transform.position += Vector3.down * stepDownDistance;
        }
        else
        {
            transform.position += Vector3.right * (stepDistance * direction);
        }

        // Play sound effect
        PlayStepSound();
    }

    void PlayStepSound()
    {
        if (enemies.Count > 0 && enemies[0] != null)
        {
            // Alternate between tic and toc sounds
            if (direction > 0)
                enemies[0].PlayTicSound();
            else
                enemies[0].PlayTocSound();
        }
    }

    void OnEnemyDestroyed(float points)
    {
        // Speed up based on remaining enemies
        enemies.RemoveAll(e => e == null);
        float ratio = (float)enemies.Count / initialEnemyCount;
        currentStepInterval = Mathf.Lerp(minStepInterval, initialStepInterval, ratio);
    }

    void ShootRandomBullet()
    {
        if (enemies.Count == 0 || enemyBulletPrefab == null) return;

        // Pick a random enemy to shoot
        Enemy shooter = enemies[Random.Range(0, enemies.Count)];
        if (shooter != null)
        {
            Vector3 spawnPos = shooter.transform.position;
            GameObject bullet = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
            Destroy(bullet, 5f); // Clean up after 5 seconds
        }
    }
}
