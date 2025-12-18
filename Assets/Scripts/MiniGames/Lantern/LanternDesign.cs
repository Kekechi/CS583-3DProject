using UnityEngine;

/// <summary>
/// ScriptableObject defining a lantern design variant.
/// Contains room item prefab and potential gameplay parameters.
/// Create new designs via: Right-click → Create → MiniGames → Lantern Design
/// </summary>
[CreateAssetMenu(fileName = "NewLanternDesign", menuName = "MiniGames/Lantern Design")]
public class LanternDesign : ScriptableObject
{
    [Header("Design Info")]
    [Tooltip("Name of this lantern design (e.g., 'Classic Cube', 'Cylinder Lantern')")]
    public string designName = "Lantern";

    [Tooltip("Preview icon for Store UI and unlock notifications")]
    public Sprite previewIcon;

    [Header("Room Item")]
    [Tooltip("Prefab placed in the room after lantern game completion")]
    public GameObject roomItemPrefab;

    [Header("Gameplay Parameters (Optional)")]
    [Tooltip("How long player must maintain perfect balance (seconds)")]
    public float goalTime = 3f;

    [Tooltip("Brightness range for the balance mechanic")]
    public Vector2 brightnessRange = new Vector2(0f, 1f);

    /// <summary>
    /// Validate the design in editor
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(designName))
        {
            designName = "Unnamed Lantern Design";
        }

        if (goalTime < 0f)
        {
            goalTime = 0f;
        }

        // Ensure brightness range is valid
        if (brightnessRange.x > brightnessRange.y)
        {
            brightnessRange = new Vector2(brightnessRange.y, brightnessRange.x);
        }
    }
}
