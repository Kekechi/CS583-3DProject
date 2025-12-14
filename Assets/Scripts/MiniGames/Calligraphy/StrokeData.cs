using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure for a single calligraphy stroke.
/// Supports both simple strokes (2 points: start→end) and compound strokes (3+ points: start→corners→end).
/// </summary>
[System.Serializable]
public class StrokeData
{
    [Header("Stroke Path")]
    [Tooltip("Ordered list of transform waypoints: [0]=start, [1..n-1]=corners, [n]=end")]
    public List<Transform> pathPoints = new List<Transform>();

    [Header("Visual References")]
    [Tooltip("Highlight sprites for each point (must match pathPoints.Count)")]
    public List<SpriteRenderer> pointHighlights = new List<SpriteRenderer>();

    [Tooltip("LineRenderer for this stroke (drawn while tracing, turns black when complete)")]
    public LineRenderer lineRenderer;

    [Header("Validation")]
    [Tooltip("How close player must be to each point to validate")]
    public float tolerance = 0.15f;

    // Helper properties
    public bool IsCompound => pathPoints.Count > 2;
    public Vector3 StartPoint => (pathPoints.Count > 0 && pathPoints[0] != null) ? pathPoints[0].position : Vector3.zero;
    public Vector3 EndPoint => (pathPoints.Count > 0 && pathPoints[pathPoints.Count - 1] != null) ? pathPoints[pathPoints.Count - 1].position : Vector3.zero;

    /// <summary>
    /// Get all corner waypoints (excludes start and end).
    /// </summary>
    public List<Vector3> GetCornerPoints()
    {
        List<Vector3> corners = new List<Vector3>();
        if (pathPoints.Count > 2)
        {
            for (int i = 1; i < pathPoints.Count - 1; i++)
            {
                if (pathPoints[i] != null)
                {
                    corners.Add(pathPoints[i].position);
                }
            }
        }
        return corners;
    }

    /// <summary>
    /// Validate that the stroke data is properly configured.
    /// </summary>
    public bool Validate(out string error)
    {
        if (pathPoints == null || pathPoints.Count < 2)
        {
            error = "StrokeData must have at least 2 path points (start and end)";
            return false;
        }

        // Check for null transforms
        for (int i = 0; i < pathPoints.Count; i++)
        {
            if (pathPoints[i] == null)
            {
                error = $"StrokeData pathPoints[{i}] is null";
                return false;
            }
        }

        if (pointHighlights == null || pointHighlights.Count != pathPoints.Count)
        {
            error = $"StrokeData must have {pathPoints.Count} pointHighlights to match pathPoints";
            return false;
        }

        if (lineRenderer == null)
        {
            error = "StrokeData must have a LineRenderer assigned";
            return false;
        }

        if (tolerance <= 0)
        {
            error = "StrokeData tolerance must be positive";
            return false;
        }

        error = null;
        return true;
    }
}
