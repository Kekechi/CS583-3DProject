using UnityEngine;

/// <summary>
/// Interface for items that can receive customization data from mini-game results.
/// Allows placed items to apply player-created customizations (brightness, colors, patterns, etc.)
/// </summary>
public interface ICustomizableItem
{
    /// <summary>
    /// Apply customization data from a completed mini-game result to this item.
    /// Implementations should cast the result to the appropriate type to extract customization data.
    /// </summary>
    /// <param name="result">The mini-game result containing customization data</param>
    void ApplyCustomization(MiniGameResult result);
}
