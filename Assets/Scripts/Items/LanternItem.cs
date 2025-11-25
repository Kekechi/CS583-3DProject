using UnityEngine;

/// <summary>
/// Component for lantern items placed in the room.
/// Applies brightness customization from the LanternGame mini-game result.
/// </summary>
[RequireComponent(typeof(Light))]
public class LanternItem : MonoBehaviour, ICustomizableItem
{
  [Header("Brightness Settings")]
  [Tooltip("Maximum intensity for the light component")]
  public float maxIntensity = 2f;

  [Tooltip("Current brightness value (0-1) from mini-game")]
  [Range(0f, 1f)]
  public float brightness = 0.5f;

  // Component references
  private Light lightComponent;

  void Awake()
  {
    lightComponent = GetComponent<Light>();

    if (lightComponent == null)
    {
      Debug.LogError("[LanternItem] Light component not found!");
    }
  }

  /// <summary>
  /// Apply brightness customization from the lantern mini-game result
  /// </summary>
  public void ApplyCustomization(MiniGameResult result)
  {
    if (result is LanternResult lanternResult)
    {
      brightness = lanternResult.finalBrightness;

      if (lightComponent != null)
      {
        lightComponent.intensity = brightness * maxIntensity;
      }

      Debug.Log($"[LanternItem] Applied customization: brightness={brightness:F2}, intensity={lightComponent.intensity:F2}");
    }
    else
    {
      Debug.LogWarning("[LanternItem] Received non-lantern result, ignoring customization");
    }
  }

  /// <summary>
  /// Manually set brightness (useful for testing or runtime adjustments)
  /// </summary>
  public void SetBrightness(float value)
  {
    brightness = Mathf.Clamp01(value);

    if (lightComponent != null)
    {
      lightComponent.intensity = brightness * maxIntensity;
    }
  }
}
