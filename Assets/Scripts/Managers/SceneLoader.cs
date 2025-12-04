using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static utility class for scene loading with fade transitions.
/// Delegates actual transition work to TransitionManager.
/// </summary>
public static class SceneLoader
{
    /// <summary>
    /// Load a scene with fade transition using default duration.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public static void LoadScene(string sceneName)
    {
        TransitionManager transitionManager = TransitionManager.Instance;

        if (transitionManager != null)
        {
            transitionManager.TransitionToScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"[SceneLoader] TransitionManager not found. Loading '{sceneName}' directly.");
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Load a scene with fade transition using custom duration.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    /// <param name="fadeDuration">Duration of fade in/out</param>
    public static void LoadScene(string sceneName, float fadeDuration)
    {
        TransitionManager transitionManager = TransitionManager.Instance;

        if (transitionManager != null)
        {
            transitionManager.TransitionToScene(sceneName, fadeDuration);
        }
        else
        {
            Debug.LogWarning($"[SceneLoader] TransitionManager not found. Loading '{sceneName}' directly.");
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Reload the current scene with fade transition.
    /// </summary>
    public static void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    /// <summary>
    /// Get the name of the currently active scene.
    /// </summary>
    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
}
