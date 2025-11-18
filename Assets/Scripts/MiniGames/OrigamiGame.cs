using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrigamiGame : MonoBehaviour
{
    // TODO: Implement Origami mini-game
    // Press correct arrow keys for accuracy

    public void StartGame()
    {
        Debug.Log("Origami game started - Placeholder");
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
        Debug.Log("Origami game completed!");

        // Play audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPaperFold();

        // Trigger game manager event
        // GameManager.OnMiniGameCompleted?.Invoke();
    }
}
