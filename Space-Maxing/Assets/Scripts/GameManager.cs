using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Overlays")]
    [Tooltip("Shown at the beginning for 3 seconds OR until the user gives input.")]
    public GameObject startOverlay;

    [Tooltip("How long to wait before auto-starting (real time, not affected by timeScale).")]
    public float startDelaySeconds = 3f;

    [Header("Scene Names")]
    [Tooltip("Exact name of your Credits scene (must match Build Settings).")]
    public string creditsSceneName = "Credits";

    [Header("Audio")]
    [Tooltip("Background music that plays throughout the game.")]
    public AudioSource backgroundMusicSource;

    private Coroutine startOverlayRoutine;

    private int currentScore = 0;
    private int highScore = 0;
    private int totalEnemyCount = 0;
    private int killedEnemyCount = 0;
    private bool gameOver = false;

    private const string HIGH_SCORE_KEY = "HighScore";

    public bool IsPausedAtStart { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateUI();

        Enemy.OnEnemyDied += OnEnemyDied;

        // Count all enemies in the scene at start
        totalEnemyCount = FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length;
        killedEnemyCount = 0;

        BeginStartPause();

        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
            backgroundMusicSource.Play();
    }

    void Update()
    {
        if (IsPausedAtStart && HasStartInput())
            EndStartPause();
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDied -= OnEnemyDied;

        if (IsPausedAtStart)
            Time.timeScale = 1f;
    }

    void OnEnemyDied(float score)
    {
        AddScore((int)score);
        killedEnemyCount++;

        Debug.Log($"Killed enemy worth {score}. Total killed: {killedEnemyCount}/{totalEnemyCount}");

        // NOTE: Wave respawning in EnemyGroup means we do NOT use count to detect win here.
        // Win is signalled explicitly via NotifyAllEnemiesKilled() from EnemyGroup.
    }

    /// <summary>
    /// Called by EnemyGroup when a full wave is cleared (no respawn / final wave).
    /// Alternatively, you can remove EnemyGroup's respawn entirely to make this finite.
    /// </summary>
    public void NotifyAllEnemiesKilled()
    {
        if (gameOver) return;
        gameOver = true;
        Debug.Log("All enemies killed! Going to Credits.");
        StartCoroutine(GoToCreditsAfterDelay(1.5f));
    }

    /// <summary>
    /// Called by Player when the player dies.
    /// </summary>
    public void NotifyPlayerDied()
    {
        if (gameOver) return;
        gameOver = true;
        Debug.Log("Player died! Going to Credits.");
        StartCoroutine(GoToCreditsAfterDelay(2f));
    }

    private IEnumerator GoToCreditsAfterDelay(float delay)
    {
        Time.timeScale = 1f;
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(creditsSceneName);
    }

    public void AddScore(int points)
    {
        currentScore += points;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = currentScore.ToString("D4");
        if (highScoreText != null) highScoreText.text = highScore.ToString("D4");
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }

    // ── Start-screen pause ──────────────────────────────────────────────────

    private void BeginStartPause()
    {
        if (startOverlay != null) startOverlay.SetActive(true);

        IsPausedAtStart = true;
        Time.timeScale = 0f;

        if (startOverlayRoutine != null) StopCoroutine(startOverlayRoutine);
        startOverlayRoutine = StartCoroutine(AutoStartAfterRealtimeSeconds(startDelaySeconds));
    }

    private void EndStartPause()
    {
        if (!IsPausedAtStart) return;

        IsPausedAtStart = false;
        Time.timeScale = 1f;

        if (startOverlay != null) startOverlay.SetActive(false);

        if (startOverlayRoutine != null)
        {
            StopCoroutine(startOverlayRoutine);
            startOverlayRoutine = null;
        }
    }

    private bool HasStartInput()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame)) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        if (Gamepad.current != null &&
            (Gamepad.current.buttonSouth.wasPressedThisFrame ||
             Gamepad.current.buttonNorth.wasPressedThisFrame ||
             Gamepad.current.buttonEast.wasPressedThisFrame ||
             Gamepad.current.buttonWest.wasPressedThisFrame ||
             Gamepad.current.startButton.wasPressedThisFrame)) return true;
        return false;
    }

    private IEnumerator AutoStartAfterRealtimeSeconds(float seconds)
    {
        if (seconds > 0f) yield return new WaitForSecondsRealtime(seconds);
        if (IsPausedAtStart) EndStartPause();
        startOverlayRoutine = null;
    }
}
