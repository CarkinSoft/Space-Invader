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

    private Coroutine startOverlayRoutine;

    private int currentScore = 0;
    private int highScore = 0;

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

        BeginStartPause();
    }

    void Update()
    {
        // If we're paused at start, any user input can begin the game early.
        if (IsPausedAtStart && HasStartInput())
        {
            EndStartPause();
        }
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
        Debug.Log($"Killed enemy worth {score}");
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

    public void RestartScene(float delaySeconds = 0f)
    {
        StartCoroutine(RestartSceneRoutine(delaySeconds));
    }

    private IEnumerator RestartSceneRoutine(float delaySeconds)
    {
        // Always restart in unpaused state
        Time.timeScale = 1f;
        IsPausedAtStart = false;

        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BeginStartPause()
    {
        if (startOverlay != null)
            startOverlay.SetActive(true);

        IsPausedAtStart = true;
        Time.timeScale = 0f;

        if (startOverlayRoutine != null)
            StopCoroutine(startOverlayRoutine);

        startOverlayRoutine = StartCoroutine(AutoStartAfterRealtimeSeconds(startDelaySeconds));
    }

    private void EndStartPause()
    {
        if (!IsPausedAtStart) return;

        IsPausedAtStart = false;
        Time.timeScale = 1f;

        if (startOverlay != null)
            startOverlay.SetActive(false);

        if (startOverlayRoutine != null)
        {
            StopCoroutine(startOverlayRoutine);
            startOverlayRoutine = null;
        }
    }

    private bool HasStartInput()
    {
        // Keyboard: any key
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        // Mouse: any button
        if (Mouse.current != null &&
            (Mouse.current.leftButton.wasPressedThisFrame ||
             Mouse.current.rightButton.wasPressedThisFrame ||
             Mouse.current.middleButton.wasPressedThisFrame))
            return true;

        // Touch: first touch begins
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        // Gamepad: any standard button
        if (Gamepad.current != null &&
            (Gamepad.current.buttonSouth.wasPressedThisFrame ||
             Gamepad.current.buttonNorth.wasPressedThisFrame ||
             Gamepad.current.buttonEast.wasPressedThisFrame ||
             Gamepad.current.buttonWest.wasPressedThisFrame ||
             Gamepad.current.startButton.wasPressedThisFrame))
            return true;

        return false;
    }

    private IEnumerator AutoStartAfterRealtimeSeconds(float seconds)
    {
        if (seconds > 0f)
            yield return new WaitForSecondsRealtime(seconds);

        // If the player hasn't started yet, start automatically.
        if (IsPausedAtStart)
            EndStartPause();

        startOverlayRoutine = null;
    }
}
