using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    public enum MiniGameType
    {
        Origami,
        Calligraphy,
        Lantern
    }

    [Header("Current Mini-Game")]
    public MiniGameType currentMiniGame;
    private int miniGameRotationIndex = 0;

    [Header("Mini-Game References")]
    public OrigamiGame origamiGame;
    public CalligraphyGame calligraphyGame;
    public LanternGame lanternGame;

    void Start()
    {
        // Hide all mini-games initially
        DeactivateAllMiniGames();
    }

    public void StartCurrentMiniGame()
    {
        // Cycle through mini-games in order
        MiniGameType[] gameOrder = { MiniGameType.Origami, MiniGameType.Calligraphy, MiniGameType.Lantern };
        currentMiniGame = gameOrder[miniGameRotationIndex % 3];
        miniGameRotationIndex++;

        DeactivateAllMiniGames();
        ActivateMiniGame(currentMiniGame);
    }

    void ActivateMiniGame(MiniGameType gameType)
    {
        switch (gameType)
        {
            case MiniGameType.Origami:
                if (origamiGame != null)
                {
                    origamiGame.gameObject.SetActive(true);
                    origamiGame.StartGame();
                }
                break;

            case MiniGameType.Calligraphy:
                if (calligraphyGame != null)
                {
                    calligraphyGame.gameObject.SetActive(true);
                    calligraphyGame.StartGame();
                }
                break;

            case MiniGameType.Lantern:
                if (lanternGame != null)
                {
                    lanternGame.gameObject.SetActive(true);
                    lanternGame.StartGame();
                }
                break;
        }

        Debug.Log($"Started {gameType} mini-game");
    }

    void DeactivateAllMiniGames()
    {
        if (origamiGame != null) origamiGame.gameObject.SetActive(false);
        if (calligraphyGame != null) calligraphyGame.gameObject.SetActive(false);
        if (lanternGame != null) lanternGame.gameObject.SetActive(false);
    }

    public void OnMiniGameComplete()
    {
        DeactivateAllMiniGames();

        if (GameManager.Instance != null)
            GameManager.Instance.OnMiniGameComplete();
    }

    public void ResetMiniGameRotation()
    {
        miniGameRotationIndex = 0;
    }
}
