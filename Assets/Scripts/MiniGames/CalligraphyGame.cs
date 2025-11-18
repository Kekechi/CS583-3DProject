using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalligraphyGame : MonoBehaviour
{
    // TODO: Implement Calligraphy mini-game
    // Keep brush speed consistent (hold/release rhythm)

    public void StartGame()
    {
        Debug.Log("Calligraphy game started - Placeholder");
        // Temporary auto-complete for testing
        StartCoroutine(AutoComplete());
    }

    IEnumerator AutoComplete()
    {
        yield return new WaitForSeconds(2f);
        OnGameComplete();
    }

    void OnGameComplete()
    {
        Debug.Log("Calligraphy game completed!");

        // Play audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBrushStroke();

        // Trigger game manager event
        // GameManager.OnMiniGameCompleted?.Invoke();
    }
}
