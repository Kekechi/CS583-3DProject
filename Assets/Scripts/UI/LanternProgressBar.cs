using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Vertical progress bar for the Lantern mini-game showing brightness level and green zone
/// </summary>
public class LanternProgressBar : MonoBehaviour
{
  [Header("UI References")]
  public Slider brightnessSlider;
  public Image fillImage;

  [Header("Green Zone Visual")]
  public RectTransform greenZoneMarker; // Visual overlay showing the target zone
  public Image greenZoneImage;

  [Header("Colors")]
  public Color successColor = Color.green;
  public Color failColor = Color.red;
  public Color greenZoneColor = new Color(0f, 1f, 0f, 0.3f); // Semi-transparent green

  // Green zone range (set by LanternGame)
  private float greenZoneMin = 0.4f;
  private float greenZoneMax = 0.6f;

  void Awake()
  {
    InitializeSlider();
  }

  void InitializeSlider()
  {
    if (brightnessSlider != null)
    {
      brightnessSlider.direction = Slider.Direction.BottomToTop;
      brightnessSlider.interactable = false; // Read-only
      brightnessSlider.minValue = 0f;
      brightnessSlider.maxValue = 1f;
      brightnessSlider.value = 0.5f;
    }
    else
    {
      Debug.LogError("[LanternProgressBar] Brightness slider is not assigned!");
    }
  }

  void SetupGreenZone()
  {
    if (greenZoneMarker == null || greenZoneImage == null) return;

    // Get the slider's fill area height
    RectTransform sliderRect = brightnessSlider.GetComponent<RectTransform>();
    float barHeight = sliderRect.rect.height;

    // Calculate green zone dimensions
    float zoneHeight = (greenZoneMax - greenZoneMin) * barHeight;
    float zoneBottom = greenZoneMin * barHeight;

    // Position and size the green zone marker
    greenZoneMarker.sizeDelta = new Vector2(greenZoneMarker.sizeDelta.x, zoneHeight);
    greenZoneMarker.anchoredPosition = new Vector2(0, zoneBottom - (barHeight / 2f));

    // Set color
    greenZoneImage.color = greenZoneColor;

    Debug.Log($"[LanternProgressBar] Green zone set: {greenZoneMin:F2} - {greenZoneMax:F2}");
  }

  /// <summary>
  /// Update the brightness bar value and return if in green zone
  /// </summary>
  public bool UpdateBrightness(float value)
  {
    if (brightnessSlider == null) return false;

    float clampedValue = Mathf.Clamp01(value);
    brightnessSlider.value = clampedValue;

    // Check if in green zone
    bool inGreenZone = clampedValue >= greenZoneMin && clampedValue <= greenZoneMax;

    // Change fill color based on whether in green zone
    if (fillImage != null)
    {
      fillImage.color = inGreenZone ? successColor : failColor;
    }

    return inGreenZone;
  }

  /// <summary>
  /// Set the green zone range dynamically
  /// </summary>
  public void SetGreenZone(float min, float max)
  {
    greenZoneMin = Mathf.Clamp01(min);
    greenZoneMax = Mathf.Clamp01(max);

    if (greenZoneMin >= greenZoneMax)
    {
      Debug.LogWarning($"[LanternProgressBar] Invalid green zone: min ({min}) >= max ({max})");
      return;
    }

    SetupGreenZone();
  }

  /// <summary>
  /// Show the progress bar
  /// </summary>
  public void Show()
  {
    gameObject.SetActive(true);
  }

  /// <summary>
  /// Hide the progress bar
  /// </summary>
  public void Hide()
  {
    gameObject.SetActive(false);
  }
}
