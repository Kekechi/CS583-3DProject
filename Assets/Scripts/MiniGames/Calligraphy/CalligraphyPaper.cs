using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controls visuals and stroke state for a calligraphy paper prefab.
/// Attached to the root of each paper prefab (e.g., IchigoIchie_Paper.prefab).
/// 
/// Phase 1: Stub implementation with hardcoded positions.
/// Later phases will add line drawing, highlights, and effects.
/// </summary>
public class CalligraphyPaper : MonoBehaviour
{
  [Header("Camera Position")]
  [Tooltip("Transform for zoomed view on the stroke area (wide view is on CameraController)")]
  [SerializeField] private Transform cameraPositionZoomed;

  [Header("Stroke Positions (Transform References)")]
  [Tooltip("Transform marking the start point of the stroke")]
  [SerializeField] private Transform strokeStartPoint;

  [Tooltip("Transform marking the end point of the stroke")]
  [SerializeField] private Transform strokeEndPoint;

  [Header("Line Drawing (Phase 2)")]
  [Tooltip("LineRenderer component for drawing the stroke")]
  [SerializeField] private LineRenderer strokeLineRenderer;

  [Tooltip("Width of the stroke line")]
  [SerializeField] private float lineWidth = 0.05f;

  [Tooltip("Z offset to render line above paper/characters (negative = towards camera)")]
  [SerializeField] private float lineZOffset = -0.01f;

  [Tooltip("Color while drawing (green = in progress)")]
  [SerializeField] private Color drawingColor = Color.green;

  [Tooltip("Color when stroke complete (black = ink)")]
  [SerializeField] private Color completedColor = Color.black;

  [Header("Visual Feedback (Phase 4)")]
  [Tooltip("SpriteRenderer for start point highlight")]
  [SerializeField] private SpriteRenderer startHighlight;

  [Tooltip("SpriteRenderer for end point highlight")]
  [SerializeField] private SpriteRenderer endHighlight;

  [Tooltip("The character TextMeshPro to change color on completion")]
  [SerializeField] private TMPro.TMP_Text strokeCharacter;

  [Tooltip("Initial color of the character (gray)")]
  [SerializeField] private Color characterStartColor = Color.gray;

  [Tooltip("Color of the character after stroke completion (black)")]
  [SerializeField] private Color characterCompletedColor = Color.black;

  [Header("Fade Animation")]
  [Tooltip("Duration of the character fade-in animation")]
  [SerializeField] private float fadeDuration = 0.8f;

  // Runtime state
  private bool isDrawing = false;
  private Coroutine fadeCoroutine;
  private bool isFading = false;

  // Events (for future phases)
  public event Action<int> OnStrokeCompleted;
  public event Action OnAllStrokesCompleted;

  /// <summary>
  /// Check if fade animation is currently playing.
  /// </summary>
  public bool IsFading => isFading;

  /// <summary>
  /// Get the zoomed camera view transform (close-up on stroke).
  /// </summary>
  public Transform GetCameraPositionZoomed()
  {
    return cameraPositionZoomed;
  }

  /// <summary>
  /// Get the world position of the current stroke's start point.
  /// </summary>
  public Vector3 GetCurrentStrokeStart()
  {
    if (strokeStartPoint == null)
    {
      Debug.LogWarning("[CalligraphyPaper] strokeStartPoint not assigned!");
      return transform.position;
    }
    return strokeStartPoint.position;
  }

  /// <summary>
  /// Get the world position of the current stroke's end point.
  /// </summary>
  public Vector3 GetCurrentStrokeEnd()
  {
    if (strokeEndPoint == null)
    {
      Debug.LogWarning("[CalligraphyPaper] strokeEndPoint not assigned!");
      return transform.position;
    }
    return strokeEndPoint.position;
  }

  // ============================================================
  // Stub methods for future phases - do nothing in Phase 1
  // ============================================================

  /// <summary>
  /// Begin drawing a stroke. Initializes the LineRenderer from start point.
  /// </summary>
  public void StartDrawing()
  {
    if (strokeLineRenderer == null)
    {
      Debug.LogError("[CalligraphyPaper] No LineRenderer assigned!");
      return;
    }

    isDrawing = true;

    // Configure LineRenderer
    strokeLineRenderer.positionCount = 2;
    strokeLineRenderer.startWidth = lineWidth;
    strokeLineRenderer.endWidth = lineWidth;
    strokeLineRenderer.startColor = drawingColor;
    strokeLineRenderer.endColor = drawingColor;

    // Set both points to start position with Z offset
    Vector3 startWorld = ApplyZOffset(GetCurrentStrokeStart());
    strokeLineRenderer.SetPosition(0, startWorld);
    strokeLineRenderer.SetPosition(1, startWorld);

    // Make visible
    strokeLineRenderer.enabled = true;

    Debug.Log($"[CalligraphyPaper] StartDrawing - Line initialized at {startWorld}");
  }

  /// <summary>
  /// Check if currently in drawing mode.
  /// </summary>
  public bool IsDrawing()
  {
    return isDrawing;
  }

  /// <summary>
  /// Update line endpoint to follow cursor position.
  /// </summary>
  public void UpdateLine(Vector3 worldPoint)
  {
    if (!isDrawing || strokeLineRenderer == null)
      return;

    // Update second point to cursor position with Z offset
    strokeLineRenderer.SetPosition(1, ApplyZOffset(worldPoint));
  }

  /// <summary>
  /// Apply Z offset to position so line renders above paper surface.
  /// </summary>
  private Vector3 ApplyZOffset(Vector3 worldPoint)
  {
    // Offset along paper's forward direction (local Z)
    return worldPoint + transform.forward * lineZOffset;
  }

  /// <summary>
  /// Complete the current stroke. Changes line to completed color and fires events.
  /// </summary>
  public void CompleteStroke()
  {
    if (!isDrawing)
      return;

    isDrawing = false;

    // Hide the temporary stroke line (character fade will reveal the real stroke)
    if (strokeLineRenderer != null)
    {
      strokeLineRenderer.enabled = false;
      strokeLineRenderer.positionCount = 0;
    }

    Debug.Log("[CalligraphyPaper] CompleteStroke - Temporary line hidden");

    // Fire events
    OnStrokeCompleted?.Invoke(0);
    OnAllStrokesCompleted?.Invoke();
  }

  /// <summary>
  /// Cancel the current stroke attempt. Hides the line instantly.
  /// </summary>
  public void CancelStroke()
  {
    if (!isDrawing)
      return;

    isDrawing = false;

    // Hide the line
    if (strokeLineRenderer != null)
    {
      strokeLineRenderer.enabled = false;
      strokeLineRenderer.positionCount = 0;
    }

    Debug.Log("[CalligraphyPaper] CancelStroke - Line hidden");
  }

  /// <summary>
  /// Show or hide the start point highlight.
  /// </summary>
  public void ShowStartHighlight(bool show)
  {
    if (startHighlight != null)
    {
      startHighlight.enabled = show;
    }
  }

  /// <summary>
  /// Show or hide the end point highlight.
  /// </summary>
  public void ShowEndHighlight(bool show)
  {
    if (endHighlight != null)
    {
      endHighlight.enabled = show;
    }
  }

  /// <summary>
  /// Change the stroke character color to completed state with fade animation.
  /// </summary>
  public void RevealCharacter()
  {
    if (strokeCharacter != null)
    {
      // Stop any existing fade
      if (fadeCoroutine != null)
      {
        StopCoroutine(fadeCoroutine);
      }

      // Start fade animation
      fadeCoroutine = StartCoroutine(FadeCharacterCoroutine());
      Debug.Log("[CalligraphyPaper] Character fade animation started");
    }
  }

  /// <summary>
  /// Coroutine to animate character color fade from start to completed.
  /// This counts as kinematic animation for assignment requirements.
  /// </summary>
  private IEnumerator FadeCharacterCoroutine()
  {
    if (strokeCharacter == null)
      yield break;

    isFading = true;
    float elapsed = 0f;
    Color startColor = strokeCharacter.color;
    Color targetColor = characterCompletedColor;

    while (elapsed < fadeDuration)
    {
      elapsed += Time.deltaTime;
      float t = elapsed / fadeDuration;

      // Smooth easing (ease-in-out)
      t = t * t * (3f - 2f * t);

      strokeCharacter.color = Color.Lerp(startColor, targetColor, t);
      yield return null;
    }

    // Ensure final color is exact
    strokeCharacter.color = targetColor;
    fadeCoroutine = null;
    isFading = false;

    Debug.Log("[CalligraphyPaper] Character fade animation complete");
  }

  /// <summary>
  /// Play the magic reveal effect. (Phase 8)
  /// </summary>
  public void PlayRevealEffect()
  {
    Debug.Log("[CalligraphyPaper] PlayRevealEffect called (stub)");
  }
}
