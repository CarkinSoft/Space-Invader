using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    private int currentScore = 0;
    private int highScore = 0;

    private const string HIGH_SCORE_KEY = "HighScore";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Load high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateUI();

        // Sign up for notification about enemy death
        Enemy.OnEnemyDied += OnEnemyDied;
    }

    void OnDestroy()
    {
        Enemy.OnEnemyDied -= OnEnemyDied;
    }

    void OnEnemyDied(float score)
    {
        AddScore((int)score);
        Debug.Log($"Killed enemy worth {score}");
    }

    public void AddScore(int points)
    {
        currentScore += points;

        // Check if new high score
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
        // Format with leading zeros (4 digits)
        if (scoreText != null)
            scoreText.text = currentScore.ToString("D4");

        if (highScoreText != null)
            highScoreText.text = highScore.ToString("D4");
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }
}
