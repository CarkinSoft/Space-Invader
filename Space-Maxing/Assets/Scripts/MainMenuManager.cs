using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a GameObject in the Main Menu scene.
/// Handles the Start button and any menu-specific setup.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Exact name of your Main Game scene (must match Build Settings).")]
    public string gameSceneName = "MainGame";

    /// <summary>
    /// Wire this to your Start Button's OnClick() in the Inspector.
    /// </summary>
    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
