using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternGame : MonoBehaviour
{
    // TODO: Implement Lantern mini-game
    // Hold/release to keep brightness bar in green "harmony" zone

    public void StartGame()
    {
        Debug.Log("Lantern game started - Placeholder");
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
        Debug.Log("Lantern game completed!");

        // Play audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLanternGlow();

        // Trigger game manager event
        // GameManager.OnMiniGameCompleted?.Invoke();
    }
}
