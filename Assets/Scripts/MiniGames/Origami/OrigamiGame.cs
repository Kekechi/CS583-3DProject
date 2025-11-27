using System;
using UnityEngine;

/// <summary>
/// Game logic controller for the Origami mini-game.
/// Tracks arrow sequence progress and handles completion.
/// </summary>
public class OrigamiGame : MonoBehaviour, IMiniGame
{
  [Header("Design")]
  [Tooltip("The origami design to use (arrow sequence, final model)")]
  public OrigamiDesign currentDesign;

  [Header("References")]
  [Tooltip("Reference to the UI controller")]
  public OrigamiUI ui;

  [Tooltip("Paper object shown during gameplay")]
  public GameObject flatPaper;

  [Tooltip("Where to spawn the final origami model")]
  public Transform modelSpawnPoint;

  // State
  private int currentIndex = 0;
  private bool isPlaying = false;

  // Spawned model reference
  private GameObject spawnedModel;

  // Events
  public event Action OnGameStarted;
  public event Action<OrigamiResult> OnGameCompleted;

  // IMiniGame implementation
  public MiniGameType GameType => MiniGameType.Origami;

  /// <summary>
  /// Start the mini-game: show paper, show UI, reset state
  /// </summary>
  [ContextMenu("Start Game")]
  public void StartGame()
  {
    isPlaying = true;
    currentIndex = 0;

    // Show paper
    if (flatPaper != null)
    {
      flatPaper.SetActive(true);
    }

    // Cleanup any previous model
    CleanupModel();

    // Setup and show UI
    if (ui != null)
    {
      ui.Setup(currentDesign.arrowSequence);
      ui.Show();
    }

    OnGameStarted?.Invoke();

    Debug.Log("[OrigamiGame] Game started");
  }

  /// <summary>
  /// Stop the game and hide all UI elements
  /// </summary>
  public void StopGame()
  {
    isPlaying = false;

    // Hide UI
    if (ui != null)
    {
      ui.Hide();
      ui.Clear();
    }

    // Hide paper
    if (flatPaper != null)
    {
      flatPaper.SetActive(false);
    }

    // Clean up spawned model
    CleanupModel();

    Debug.Log("[OrigamiGame] Game stopped");
  }

  void Update()
  {
    if (!isPlaying) return;

    CheckInput();
  }

  /// <summary>
  /// Check if player pressed the correct arrow key
  /// </summary>
  void CheckInput()
  {
    if (currentDesign == null || currentDesign.arrowSequence == null) return;
    if (currentIndex >= currentDesign.arrowSequence.Length) return;

    KeyCode expected = currentDesign.arrowSequence[currentIndex];

    // Check for correct input
    if (Input.GetKeyDown(expected))
    {
      OnCorrectInput();
    }
    // Check for wrong input (any other arrow key)
    else if (IsAnyArrowKeyPressed() && !Input.GetKeyDown(expected))
    {
      OnWrongInput();
    }
  }

  /// <summary>
  /// Check if any arrow key was pressed this frame
  /// </summary>
  bool IsAnyArrowKeyPressed()
  {
    return Input.GetKeyDown(KeyCode.UpArrow) ||
           Input.GetKeyDown(KeyCode.DownArrow) ||
           Input.GetKeyDown(KeyCode.LeftArrow) ||
           Input.GetKeyDown(KeyCode.RightArrow);
  }

  /// <summary>
  /// Handle correct arrow input
  /// </summary>
  void OnCorrectInput()
  {
    // Update UI
    if (ui != null)
    {
      ui.MarkComplete(currentIndex);
    }

    Debug.Log($"[OrigamiGame] Correct! Arrow {currentIndex + 1}/{currentDesign.arrowSequence.Length}");

    // Advance progress
    currentIndex++;

    // Check for completion
    if (currentIndex >= currentDesign.arrowSequence.Length)
    {
      CompleteGame();
    }
  }

  /// <summary>
  /// Handle wrong arrow input (placeholder for feedback)
  /// </summary>
  void OnWrongInput()
  {
    Debug.Log("[OrigamiGame] Wrong arrow!");
    // TODO: Add visual/audio feedback later
  }

  /// <summary>
  /// Handle game completion
  /// </summary>
  void CompleteGame()
  {
    isPlaying = false;

    Debug.Log("[OrigamiGame] All arrows complete!");

    // Hide paper
    if (flatPaper != null)
    {
      flatPaper.SetActive(false);
    }

    // Spawn final model
    SpawnModel();

    // Hide UI
    if (ui != null)
    {
      ui.Hide();
    }

    // Fire completion event
    OrigamiResult result = new OrigamiResult
    {
      roomItemPrefab = currentDesign.roomItemPrefab,
      designName = currentDesign.designName,
      CompletionTime = Time.time
    };

    OnGameCompleted?.Invoke(result);

    Debug.Log($"[OrigamiGame] Game completed - {currentDesign.designName}");
  }

  /// <summary>
  /// Spawn the final origami model
  /// </summary>
  void SpawnModel()
  {
    if (currentDesign.finalModelPrefab == null)
    {
      Debug.LogWarning("[OrigamiGame] No final model prefab assigned");
      return;
    }

    Vector3 spawnPosition = modelSpawnPoint != null ? modelSpawnPoint.position : transform.position;
    Quaternion spawnRotation = modelSpawnPoint != null ? modelSpawnPoint.rotation : Quaternion.identity;

    spawnedModel = Instantiate(currentDesign.finalModelPrefab, spawnPosition, spawnRotation);

    Debug.Log("[OrigamiGame] Final model spawned");
  }

  /// <summary>
  /// Cleanup the spawned model
  /// </summary>
  void CleanupModel()
  {
    if (spawnedModel != null)
    {
      Destroy(spawnedModel);
      spawnedModel = null;
    }
  }
}

/// <summary>
/// Data container for completed origami mini-game result
/// </summary>
[Serializable]
public class OrigamiResult : MiniGameResult
{
  public override GameObject ItemInstance => roomItemPrefab;
  public override MiniGameType GameType => MiniGameType.Origami;

  // Origami-specific data
  public GameObject roomItemPrefab;  // The prefab to instantiate in the room
  public string designName;          // Which origami was made
}
