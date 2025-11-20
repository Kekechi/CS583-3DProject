using UnityEngine;

/// <summary>
/// Controls the visual appearance of the lantern.
/// Maps brightness value to emission intensity.
/// Attached to the lantern prefab.
/// </summary>
public class LanternVisual : MonoBehaviour
{
  [Header("Visual Settings")]
  [Tooltip("Renderer of the paper cube (has emissive material)")]
  public Renderer paperRenderer;

  [Header("Emission Settings")]
  [Tooltip("Maps input brightness to visual intensity (allows non-linear response)")]
  public AnimationCurve brightnessResponse = AnimationCurve.Linear(0, 0, 1, 1);

  [Tooltip("Gradient controlling both color and intensity from brightness 0 to 1")]
  public Gradient emissionGradient;

  // Runtime material instance
  private Material paperMaterial;
  private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

  void Awake()
  {
    InitializeMaterial();
    InitializeDefaultGradient();
  }

  /// <summary>
  /// Setup default gradient if not configured
  /// </summary>
  void InitializeDefaultGradient()
  {
    if (emissionGradient == null)
    {
      Debug.Log("Emission Gradient Not Found, Using Default");
      emissionGradient = new Gradient();

      // Setup color keys (warm orange to bright yellow-white)
      GradientColorKey[] colorKeys = new GradientColorKey[3];
      colorKeys[0] = new GradientColorKey(new Color(0.5f, 0.2f, 0.1f), 0f); // Dark orange at 0
      colorKeys[1] = new GradientColorKey(new Color(1f, 0.9f, 0.7f), 0.5f); // Warm glow at 0.5
      colorKeys[2] = new GradientColorKey(new Color(2f, 1.8f, 1.4f), 1f); // Bright HDR at 1

      // Setup alpha keys (always opaque)
      GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
      alphaKeys[0] = new GradientAlphaKey(1f, 0f);
      alphaKeys[1] = new GradientAlphaKey(1f, 1f);

      emissionGradient.SetKeys(colorKeys, alphaKeys);
    }
  }

  /// <summary>
  /// Get material instance and enable emission
  /// </summary>
  void InitializeMaterial()
  {
    if (paperRenderer == null)
    {
      Debug.LogError("[LanternVisual] Paper renderer is not assigned!");
      return;
    }

    // Create instance to avoid modifying shared material
    paperMaterial = paperRenderer.material;
    paperMaterial.EnableKeyword("_EMISSION");

    Debug.Log("[LanternVisual] Material initialized");
  }

  /// <summary>
  /// Update the lantern's visual brightness
  /// </summary>
  /// <param name="inputBrightness">Brightness value from game (0-1)</param>
  public void SetBrightness(float inputBrightness)
  {
    if (paperMaterial == null) return;

    // Map input through curve for flexible response
    float mappedBrightness = brightnessResponse.Evaluate(inputBrightness);

    // Get color from gradient (includes intensity via HDR values)
    Color emissionColor = emissionGradient.Evaluate(mappedBrightness);

    // Apply emission directly (gradient already contains intensity)
    paperMaterial.SetColor(EmissionColor, emissionColor);
  }

  void OnDestroy()
  {
    // Clean up material instance to prevent memory leak
    if (paperMaterial != null)
    {
      Destroy(paperMaterial);
    }
  }
}
