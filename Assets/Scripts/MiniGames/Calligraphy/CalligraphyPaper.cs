using System;
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
  [Tooltip("Transform child that defines where camera should move to view this paper")]
  [SerializeField] private Transform cameraPosition;

  [Header("Stroke Positions (Local Space)")]
  [Tooltip("Start position of the stroke in local coordinates")]
  [SerializeField] private Vector3 strokeStartLocal = new Vector3(-0.3f, 0.9f, 0f);

  [Tooltip("End position of the stroke in local coordinates")]
  [SerializeField] private Vector3 strokeEndLocal = new Vector3(0.3f, 0.9f, 0f);

  [Header("Line Drawing (Phase 2)")]
  [Tooltip("LineRenderer component for drawing the stroke")]
  [SerializeField] private LineRenderer strokeLineRenderer;

  [Tooltip("Width of the stroke line")]
  [SerializeField] private float lineWidth = 0.05f;

  [Tooltip("Color while drawing (green = in progress)")]
  [SerializeField] private Color drawingColor = Color.green;

  [Tooltip("Color when stroke complete (black = ink)")]
  [SerializeField] private Color completedColor = Color.black;

  // Runtime state
  private bool isDrawing = false;

  // Events (for future phases)
  public event Action<int> OnStrokeCompleted;
  public event Action OnAllStrokesCompleted;

  /// <summary>
  /// Get the camera target transform for this paper.
  /// </summary>
  public Transform GetCameraPosition()
  {
    return cameraPosition;
  }

  /// <summary>
  /// Get the world position of the current stroke's start point.
  /// </summary>
  public Vector3 GetCurrentStrokeStart()
  {
    return transform.TransformPoint(strokeStartLocal);
  }

  /// <summary>
  /// Get the world position of the current stroke's end point.
  /// </summary>
  public Vector3 GetCurrentStrokeEnd()
  {
    return transform.TransformPoint(strokeEndLocal);
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

    // Set both points to start position initially
    Vector3 startWorld = GetCurrentStrokeStart();
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

    // Update second point to cursor position
    strokeLineRenderer.SetPosition(1, worldPoint);
  }

  /// <summary>
  /// Complete the current stroke. (Phase 3)
  /// </summary>
  public void CompleteStroke()
  {
    Debug.Log("[CalligraphyPaper] CompleteStroke called (stub)");
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
  /// Show or hide the start point highlight. (Phase 4)
  /// </summary>
  public void ShowStartHighlight(bool show)
  {
    // Stub - will control SpriteRenderer in Phase 4
  }

  /// <summary>
  /// Play the magic reveal effect. (Phase 8)
  /// </summary>
  public void PlayRevealEffect()
  {
    Debug.Log("[CalligraphyPaper] PlayRevealEffect called (stub)");
  }
}
