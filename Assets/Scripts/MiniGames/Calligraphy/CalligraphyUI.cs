using System.Collections;
using UnityEngine;

/// <summary>
/// UI controller for the Calligraphy mini-game.
/// Manages success panel display.
/// </summary>
public class CalligraphyUI : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("UI Elements")]
  [Tooltip("Root panel to show/hide entire UI (child of this GameObject)")]
  [SerializeField] private GameObject rootPanel;

  [Tooltip("Success panel content (child of rootPanel)")]
  [SerializeField] private GameObject successPanel;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Awake()
  {
    // Ensure panels start hidden
    if (rootPanel != null)
    {
      rootPanel.SetActive(false);
    }
    if (successPanel != null)
    {
      successPanel.SetActive(false);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Show the UI (activates root panel).
  /// </summary>
  public void Show()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(true);
      Debug.Log("[CalligraphyUI] UI shown");
    }
  }

  /// <summary>
  /// Hide the UI (deactivates root panel).
  /// </summary>
  public void Hide()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(false);
    }
    Debug.Log("[CalligraphyUI] UI hidden");
  }

  /// <summary>
  /// Show the success panel (must call Show() first or rootPanel must be active).
  /// </summary>
  public void ShowSuccess()
  {
    // Ensure root is active first
    if (rootPanel != null && !rootPanel.activeSelf)
    {
      rootPanel.SetActive(true);
    }

    if (successPanel != null)
    {
      successPanel.SetActive(true);
      Debug.Log("[CalligraphyUI] Success panel shown");
    }
    else
    {
      Debug.LogWarning("[CalligraphyUI] successPanel is NULL!");
    }
  }

  /// <summary>
  /// Hide the success panel.
  /// </summary>
  public void HideSuccess()
  {
    if (successPanel != null)
    {
      successPanel.SetActive(false);
    }
    Debug.Log("[CalligraphyUI] Success panel hidden");
  }

  /// <summary>
  /// Show success panel (coroutine version for consistency).
  /// </summary>
  public IEnumerator ShowSuccessAsync()
  {
    ShowSuccess();
    yield return null;
  }

  /// <summary>
  /// Hide success panel (coroutine version for consistency).
  /// </summary>
  public IEnumerator HideSuccessAsync()
  {
    HideSuccess();
    yield return null;
  }

  /// <summary>
  /// Immediately hide everything (for cleanup).
  /// </summary>
  public void HideImmediate()
  {
    if (successPanel != null)
      successPanel.SetActive(false);
    if (rootPanel != null)
      rootPanel.SetActive(false);
  }
}
