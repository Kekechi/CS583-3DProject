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

  [Header("Multi-Stroke Configuration")]
  [Tooltip("Array of strokes that make up this character")]
  [SerializeField] private System.Collections.Generic.List<StrokeData> strokes = new System.Collections.Generic.List<StrokeData>();

  [Header("Line Drawing")]
  [Tooltip("Width of the stroke line")]
  [SerializeField] private float lineWidth = 0.05f;

  [Tooltip("Z offset to render line above paper/characters (negative = towards camera)")]
  [SerializeField] private float lineZOffset = -0.01f;

  [Tooltip("Color while drawing (green = in progress)")]
  [SerializeField] private Color drawingColor = Color.green;

  [Tooltip("Color when stroke complete (black = ink)")]
  [SerializeField] private Color completedColor = Color.black;

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
  private int currentStrokeIndex = 0;
  private bool isDrawing = false;
  private Coroutine fadeCoroutine;
  private bool isFading = false;
  private LineRenderer activeLineRenderer;

  // Events
  public event Action<int> OnStrokeCompleted;
  public event Action OnAllStrokesCompleted;

  /// <summary>
  /// Get the current stroke being worked on.
  /// </summary>
  public StrokeData CurrentStroke => (currentStrokeIndex >= 0 && currentStrokeIndex < strokes.Count) ? strokes[currentStrokeIndex] : null;

  /// <summary>
  /// Get the total number of strokes in this character.
  /// </summary>
  public int TotalStrokes => strokes.Count;

  /// <summary>
  /// Check if all strokes are complete.
  /// </summary>
  public bool AllStrokesComplete => currentStrokeIndex >= strokes.Count;

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
    if (CurrentStroke == null || CurrentStroke.pathPoints.Count == 0)
    {
      Debug.LogWarning("[CalligraphyPaper] Current stroke has no path points!");
      return transform.position;
    }
    return CurrentStroke.StartPoint;
  }

  /// <summary>
  /// Get the world position of the current stroke's end point.
  /// </summary>
  public Vector3 GetCurrentStrokeEnd()
  {
    if (CurrentStroke == null || CurrentStroke.pathPoints.Count == 0)
    {
      Debug.LogWarning("[CalligraphyPaper] Current stroke has no path points!");
      return transform.position;
    }
    return CurrentStroke.EndPoint;
  }

  // ============================================================
  // Stub methods for future phases - do nothing in Phase 1
  // ============================================================

  /// <summary>
  /// Show guide visuals for the current stroke (highlights for all points).
  /// </summary>
  public void ShowStrokeGuide()
  {
    if (CurrentStroke == null) return;

    // Show all point highlights
    for (int i = 0; i < CurrentStroke.pointHighlights.Count; i++)
    {
      if (CurrentStroke.pointHighlights[i] != null)
      {
        CurrentStroke.pointHighlights[i].enabled = true;

        // Color coding: start/end = green, corners = yellow
        if (i == 0 || i == CurrentStroke.pointHighlights.Count - 1)
        {
          CurrentStroke.pointHighlights[i].color = Color.green;
        }
        else
        {
          CurrentStroke.pointHighlights[i].color = Color.yellow;
        }
      }
    }

    Debug.Log($"[CalligraphyPaper] ShowStrokeGuide - Displayed {CurrentStroke.pointHighlights.Count} point highlights");
  }

  /// <summary>
  /// Show or hide the start point highlight.
  /// </summary>
  public void ShowStartHighlight(bool show)
  {
    if (CurrentStroke == null || CurrentStroke.pointHighlights.Count == 0) return;

    if (CurrentStroke.pointHighlights[0] != null)
    {
      CurrentStroke.pointHighlights[0].enabled = show;
    }
  }

  /// <summary>
  /// Show or hide the end point highlight.
  /// </summary>
  public void ShowEndHighlight(bool show)
  {
    if (CurrentStroke == null || CurrentStroke.pointHighlights.Count == 0) return;

    int endIndex = CurrentStroke.pointHighlights.Count - 1;
    if (CurrentStroke.pointHighlights[endIndex] != null)
    {
      CurrentStroke.pointHighlights[endIndex].enabled = show;
    }

    isDrawing = true;
    activeLineRenderer = CurrentStroke.lineRenderer;

    // Configure LineRenderer
    activeLineRenderer.positionCount = 2;
    activeLineRenderer.startWidth = lineWidth;
    activeLineRenderer.endWidth = lineWidth;
    activeLineRenderer.startColor = drawingColor;
    activeLineRenderer.endColor = drawingColor;

    // Set both points to start position with Z offset
    Vector3 startWorld = ApplyZOffset(GetCurrentStrokeStart());
    activeLineRenderer.SetPosition(0, startWorld);
    activeLineRenderer.SetPosition(1, startWorld);

    // Make visible
    activeLineRenderer.enabled = true;

    Debug.Log($"[CalligraphyPaper] StartDrawing - Line initialized at {startWorld} for stroke {currentStrokeIndex + 1}/{TotalStrokes}");
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
    if (!isDrawing || activeLineRenderer == null)
      return;

    // Update second point to cursor position with Z offset
    activeLineRenderer.SetPosition(1, ApplyZOffset(worldPoint));
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
  /// Complete the current stroke. Changes line to black and advances to next stroke.
  /// </summary>
  public void CompleteStroke()
  {
    if (!isDrawing)
      return;

    isDrawing = false;

    // Turn line black (completed color)
    if (activeLineRenderer != null)
    {
      activeLineRenderer.startColor = completedColor;
      activeLineRenderer.endColor = completedColor;
    }

    Debug.Log($"[CalligraphyPaper] CompleteStroke - Stroke {currentStrokeIndex + 1}/{TotalStrokes} completed");

    // Fire stroke completed event
    OnStrokeCompleted?.Invoke(currentStrokeIndex);

    // Advance to next stroke
    currentStrokeIndex++;

    // Check if all strokes complete
    if (AllStrokesComplete)
    {
      Debug.Log("[CalligraphyPaper] All strokes completed!");
      OnAllStrokesCompleted?.Invoke();
    }
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
    if (activeLineRenderer != null)
    {
      activeLineRenderer.enabled = false;
      activeLineRenderer.positionCount = 0;
    }

    Debug.Log("[CalligraphyPaper] CancelStroke - Line hidden");
  }

  /// <summary>
  /// Reset to first stroke (used when restarting the character).
  /// </summary>
  public void ResetStrokes()
  {
    currentStrokeIndex = 0;
    isDrawing = false;
    activeLineRenderer = null;

    // Hide all guides
    foreach (var stroke in strokes)
    {
      if (stroke != null)
      {
        foreach (var highlight in stroke.pointHighlights)
        {
          if (highlight != null) highlight.enabled = false;
        }

        if (stroke.lineRenderer != null)
        {
          stroke.lineRenderer.enabled = false;
        }
      }
    }

    Debug.Log("[CalligraphyPaper] ResetStrokes - Ready for first stroke");
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
