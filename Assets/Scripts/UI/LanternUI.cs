using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Display-only UI controller for Lantern mini-game.
/// Shows brightness bar, harmony progress, and success message.
/// Never calculates game state - only displays what LanternGame tells it.
/// </summary>
public class LanternUI : MonoBehaviour
{
  [Header("UI Elements")]
  [Tooltip("Parent panel to show/hide entire UI")]
  public GameObject rootPanel;

  [Tooltip("Vertical slider showing current brightness")]
  public Slider brightnessSlider;

  [Tooltip("Fill image that changes color (red/green)")]
  public Image brightnessFill;

  [Tooltip("Green overlay showing target harmony zone")]
  public RectTransform harmonyZoneOverlay;

  [Tooltip("Horizontal slider showing time progress")]
  public Slider harmonyProgressSlider;

  [Tooltip("Title text")]
  public TextMeshProUGUI titleText;

  [Tooltip("Instructions panel (can be hidden)")]
  public GameObject instructionsPanel;

  [Tooltip("Success panel (hidden until win)")]
  public GameObject successPanel;

  [Header("Colors")]
  public Color inHarmonyColor = Color.green;
  public Color outOfHarmonyColor = Color.red;

  void Awake()
  {
    InitializeSliders();
  }

  /// <summary>
  /// Initialize slider settings
  /// </summary>
  void InitializeSliders()
  {
    if (brightnessSlider != null)
    {
      brightnessSlider.direction = Slider.Direction.BottomToTop;
      brightnessSlider.interactable = false;
      brightnessSlider.minValue = 0f;
      brightnessSlider.maxValue = 1f;
    }

    if (harmonyProgressSlider != null)
    {
      harmonyProgressSlider.interactable = false;
      harmonyProgressSlider.minValue = 0f;
      harmonyProgressSlider.maxValue = 1f;
    }
  }

  /// <summary>
  /// Show the UI panel
  /// </summary>
  public void Show()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(true);
    }

    if (instructionsPanel != null)
    {
      instructionsPanel.SetActive(true);
    }

    if (successPanel != null)
    {
      successPanel.SetActive(false);
    }

    Debug.Log("[LanternUI] UI shown");
  }

  /// <summary>
  /// Hide the UI panel
  /// </summary>
  public void Hide()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(false);
    }

    Debug.Log("[LanternUI] UI hidden");
  }

  /// <summary>
  /// Update all display elements based on game state
  /// </summary>
  /// <param name="brightness">Current brightness value (0-1)</param>
  /// <param name="isInHarmony">Whether player is in harmony zone</param>
  /// <param name="currentTime">Time accumulated in harmony</param>
  /// <param name="maxTime">Time needed to win</param>
  public void UpdateDisplay(float brightness, bool isInHarmony, float currentTime, float maxTime)
  {
    // Update brightness bar
    if (brightnessSlider != null)
    {
      brightnessSlider.value = brightness;
    }

    // Update color based on harmony state
    if (brightnessFill != null)
    {
      brightnessFill.color = isInHarmony ? inHarmonyColor : outOfHarmonyColor;
    }

    // Update progress bar (shows how much time accumulated)
    if (harmonyProgressSlider != null)
    {
      harmonyProgressSlider.value = currentTime / maxTime;
    }
  }

  /// <summary>
  /// Setup the harmony zone overlay position and size
  /// </summary>
  /// <param name="min">Lower bound of zone (0-1)</param>
  /// <param name="max">Upper bound of zone (0-1)</param>
  public void SetupHarmonyZone(float min, float max)
  {
    if (harmonyZoneOverlay == null || brightnessSlider == null)
    {
      Debug.LogWarning("[LanternUI] Cannot setup harmony zone - references missing");
      return;
    }

    RectTransform sliderRect = brightnessSlider.GetComponent<RectTransform>();
    float barHeight = sliderRect.rect.height;

    // Calculate zone dimensions
    float zoneHeight = (max - min) * barHeight;
    float zoneBottomY = min * barHeight;

    // Setup anchoring (assuming overlay is child of slider)
    harmonyZoneOverlay.anchorMin = new Vector2(0.25f, 0);
    harmonyZoneOverlay.anchorMax = new Vector2(0.75f, 0);
    harmonyZoneOverlay.pivot = new Vector2(0.5f, 0);

    // Set size and position
    harmonyZoneOverlay.sizeDelta = new Vector2(0, zoneHeight);
    harmonyZoneOverlay.anchoredPosition = new Vector2(0, zoneBottomY);

    Debug.Log($"[LanternUI] Harmony zone setup: {min:F2}-{max:F2} (Height: {zoneHeight:F1}px, Y: {zoneBottomY:F1}px)");
  }

  /// <summary>
  /// Show the success panel
  /// </summary>
  public void ShowSuccess()
  {
    if (successPanel != null)
    {
      successPanel.SetActive(true);
    }

    Debug.Log("[LanternUI] Success panel shown");
  }
}
