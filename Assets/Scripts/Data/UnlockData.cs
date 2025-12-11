using UnityEngine;

/// <summary>
/// Serializable data class for tracking unlock progress and player selections.
/// Saved to PlayerPrefs as JSON.
/// </summary>
[System.Serializable]
public class UnlockData
{
    [Header("Progress")]
    public int roomCompletions = 0;
    
    [Header("Unlock States")]
    public bool lanternBUnlocked = false;
    public bool origamiBUnlocked = false;
    public bool calligraphyBUnlocked = false;
    
    [Header("Player Selections")]
    public ItemVariant selectedLantern = ItemVariant.Default;
    public ItemVariant selectedOrigami = ItemVariant.Default;
    public ItemVariant selectedCalligraphy = ItemVariant.Default;
}

/// <summary>
/// Enum for item variant types.
/// </summary>
public enum ItemVariant
{
    Default,
    StyleB
}
