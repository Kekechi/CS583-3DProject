using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Display-only UI controller for Origami mini-game.
/// Spawns arrow icons based on sequence and updates their colors on progress.
/// Never calculates game state - only displays what OrigamiGame tells it.
/// </summary>
public class OrigamiUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Parent panel to show/hide entire UI")]
    public GameObject rootPanel;

    [Tooltip("Container for arrow icons (should have HorizontalLayoutGroup)")]
    public Transform arrowContainer;

    [Tooltip("Prefab for individual arrow icon")]
    public GameObject arrowIconPrefab;

    [Header("Arrow Sprite")]
    [Tooltip("Single arrow sprite (pointing UP). Will be rotated for other directions.")]
    public Sprite arrowSprite;

    [Header("Colors")]
    [Tooltip("Color for arrows not yet completed")]
    public Color upcomingColor = Color.gray;

    [Tooltip("Color for completed arrows")]
    public Color completedColor = new Color(1f, 0.84f, 0f); // Gold

    // Runtime state
    private List<Image> arrowImages = new List<Image>();

    /// <summary>
    /// Setup the arrow sequence display.
    /// Creates arrow icons for each key in the sequence.
    /// </summary>
    /// <param name="sequence">Array of KeyCodes to display</param>
    public void Setup(KeyCode[] sequence)
    {
        // Clear any existing arrows
        Clear();

        if (sequence == null || sequence.Length == 0)
        {
            Debug.LogWarning("[OrigamiUI] Cannot setup - sequence is null or empty");
            return;
        }

        if (arrowIconPrefab == null)
        {
            Debug.LogError("[OrigamiUI] Cannot setup - arrowIconPrefab is not assigned");
            return;
        }

        if (arrowContainer == null)
        {
            Debug.LogError("[OrigamiUI] Cannot setup - arrowContainer is not assigned");
            return;
        }

        // Create arrow icons
        foreach (KeyCode key in sequence)
        {
            GameObject iconObj = Instantiate(arrowIconPrefab, arrowContainer);
            Image iconImage = iconObj.GetComponent<Image>();
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();

            if (iconImage != null)
            {
                iconImage.sprite = arrowSprite;
                iconImage.color = upcomingColor;

                // Rotate based on direction (sprite should point UP by default)
                if (rectTransform != null)
                {
                    rectTransform.localRotation = Quaternion.Euler(0, 0, GetRotationForKey(key));
                }

                arrowImages.Add(iconImage);
            }
            else
            {
                Debug.LogWarning("[OrigamiUI] Arrow prefab missing Image component");
            }
        }

        Debug.Log($"[OrigamiUI] Setup complete - {arrowImages.Count} arrows created");
    }

    /// <summary>
    /// Mark an arrow as completed (change to gold color).
    /// </summary>
    /// <param name="index">Index of the arrow to mark complete</param>
    public void MarkComplete(int index)
    {
        if (index < 0 || index >= arrowImages.Count)
        {
            Debug.LogWarning($"[OrigamiUI] Cannot mark complete - index {index} out of range (0-{arrowImages.Count - 1})");
            return;
        }

        arrowImages[index].color = completedColor;
        Debug.Log($"[OrigamiUI] Arrow {index} marked complete");
    }

    /// <summary>
    /// Clear all spawned arrow icons.
    /// </summary>
    public void Clear()
    {
        foreach (Image img in arrowImages)
        {
            if (img != null)
            {
                Destroy(img.gameObject);
            }
        }
        arrowImages.Clear();
    }

    /// <summary>
    /// Show the UI panel.
    /// </summary>
    public void Show()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(true);
        }
        Debug.Log("[OrigamiUI] UI shown");
    }

    /// <summary>
    /// Hide the UI panel.
    /// </summary>
    public void Hide()
    {
        if (rootPanel != null)
        {
            rootPanel.SetActive(false);
        }
        Debug.Log("[OrigamiUI] UI hidden");
    }

    /// <summary>
    /// Get the Z rotation for an arrow key.
    /// Assumes the sprite points UP (0 degrees) by default.
    /// </summary>
    private float GetRotationForKey(KeyCode key)
    {
        return key switch
        {
            KeyCode.UpArrow => 90f,       // Points up
            KeyCode.RightArrow => 0f,  // Rotate clockwise 90
            KeyCode.DownArrow => -90f,   // Rotate 180
            KeyCode.LeftArrow => 180f,    // Rotate counter-clockwise 90
            _ => 0f                      // Fallback to up
        };
    }

    /// <summary>
    /// Cleanup when destroyed.
    /// </summary>
    void OnDestroy()
    {
        Clear();
    }
}
