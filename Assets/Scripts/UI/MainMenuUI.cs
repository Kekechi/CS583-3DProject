using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles Main Menu UI interactions.
/// Wire button OnClick events to this script's public methods.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("Buttons")]
  [SerializeField] private Button startButton;
  [SerializeField] private Button storeButton;
  [SerializeField] private Button exitButton;

  [Header("Scene Names")]
  [SerializeField] private string gameSceneName = "SampleScene";

  [Header("Visual Feedback")]
  [Tooltip("Text to show on disabled buttons")]
  [SerializeField] private string disabledButtonText = "(Coming Soon)";

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Awake()
  {
    SetupButtons();
  }

  private void Start()
  {
    Debug.Log("[MainMenuUI] Main Menu loaded");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Setup
  // ─────────────────────────────────────────────────────────────────────────
  private void SetupButtons()
  {
    // Start button - enabled
    if (startButton != null)
    {
      startButton.onClick.AddListener(OnStartClicked);
      startButton.interactable = true;
    }
    else
    {
      Debug.LogWarning("[MainMenuUI] startButton not assigned");
    }

    // Store button - disabled (coming soon)
    if (storeButton != null)
    {
      storeButton.onClick.AddListener(OnStoreClicked);
      storeButton.interactable = false;

      // Update button text to show it's disabled
      TMP_Text buttonText = storeButton.GetComponentInChildren<TMP_Text>();
      if (buttonText != null)
      {
        buttonText.text = $"Store {disabledButtonText}";
      }
    }

    // Exit button - enabled
    if (exitButton != null)
    {
      exitButton.onClick.AddListener(OnExitClicked);
      exitButton.interactable = true;
    }
    else
    {
      Debug.LogWarning("[MainMenuUI] exitButton not assigned");
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Button Handlers
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Start button clicked - load game scene.
  /// </summary>
  public void OnStartClicked()
  {
    Debug.Log("[MainMenuUI] Start button clicked");

    // Prevent multiple clicks during transition
    if (TransitionManager.Instance != null && TransitionManager.Instance.IsTransitioning)
    {
      Debug.Log("[MainMenuUI] Already transitioning, ignoring click");
      return;
    }

    // Disable buttons during transition
    SetButtonsInteractable(false);

    // Load game scene
    SceneLoader.LoadScene(gameSceneName);
  }

  /// <summary>
  /// Store button clicked - placeholder for future feature.
  /// </summary>
  public void OnStoreClicked()
  {
    Debug.Log("[MainMenuUI] Store button clicked (not implemented)");
    // Future: Open store/skills screen
  }

  /// <summary>
  /// Exit button clicked - quit application.
  /// </summary>
  public void OnExitClicked()
  {
    Debug.Log("[MainMenuUI] Exit button clicked");

#if UNITY_EDITOR
    // Stop play mode in editor
    UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit application in build
        Application.Quit();
#endif
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Utility
  // ─────────────────────────────────────────────────────────────────────────

  private void SetButtonsInteractable(bool interactable)
  {
    if (startButton != null) startButton.interactable = interactable;
    if (exitButton != null) exitButton.interactable = interactable;
    // Store button stays disabled
  }
}
