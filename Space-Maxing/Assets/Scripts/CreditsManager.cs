using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a GameObject in the Credits scene.
/// Automatically returns to the Main Menu after a set duration.
/// </summary>
public class CreditsManager : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Exact name of your Main Menu scene (must match Build Settings).")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Timing")]
    [Tooltip("How many seconds to show the Credits before returning to Main Menu.")]
    public float displayDuration = 5f;

    void Start()
    {
        StartCoroutine(ReturnToMainMenuAfterDelay());
    }

    private IEnumerator ReturnToMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
