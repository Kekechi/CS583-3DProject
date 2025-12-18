using UnityEngine;

/// <summary>
/// Singleton manager for tracking unlocks, player selections, and save/load.
/// Persists across scenes with DontDestroyOnLoad.
/// </summary>
public class UnlockManager : MonoBehaviour
{
  public static UnlockManager Instance { get; private set; }

  [Header("Unlock Data")]
  [SerializeField] private UnlockData unlockData = new UnlockData();

  [Header("Design References")]
  [SerializeField] private CalligraphyDesign calligraphyDefault;
  [SerializeField] private CalligraphyDesign calligraphyStyleB;
  [SerializeField] private OrigamiDesign origamiDefault;
  [SerializeField] private OrigamiDesign origamiStyleB;
  [SerializeField] private LanternDesign lanternDefault;
  [SerializeField] private LanternDesign lanternStyleB;

  private const string SAVE_KEY = "UnlockData";

  // ─────────────────────────────────────────────────────────────────────────
  // Initialization
  // ─────────────────────────────────────────────────────────────────────────

  private void Awake()
  {
    // Singleton pattern
    if (Instance != null && Instance != this)
    {
      Debug.Log("[UnlockManager] Duplicate UnlockManager found, destroying this one");
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    LoadData();
    Debug.Log("[UnlockManager] Initialized");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Room Completion
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Called when player completes a room. Increments count and checks for new unlocks.
  /// </summary>
  public void CompleteRoom()
  {
    Debug.Log($"[UnlockManager] CompleteRoom() called. Current count: {unlockData.roomCompletions}");

    unlockData.roomCompletions++;

    Debug.Log($"[UnlockManager] ★ Room completed! Total completions: {unlockData.roomCompletions} ★");

    CheckUnlocks();
    SaveData();

    // Debug: Print current status
    DebugPrintStatus();
  }

  /// <summary>
  /// Check unlock conditions and update unlock states.
  /// </summary>
  private void CheckUnlocks()
  {
    // 1st completion → Lantern B
    if (unlockData.roomCompletions >= 1 && !unlockData.lanternBUnlocked)
    {
      unlockData.lanternBUnlocked = true;
      Debug.Log("[UnlockManager] 🎉 Unlocked: Lantern Style B!");
    }

    // 3rd completion → Origami B
    if (unlockData.roomCompletions >= 3 && !unlockData.origamiBUnlocked)
    {
      unlockData.origamiBUnlocked = true;
      Debug.Log("[UnlockManager] 🎉 Unlocked: Origami Style B!");
    }

    // 6th completion → Calligraphy B
    if (unlockData.roomCompletions >= 6 && !unlockData.calligraphyBUnlocked)
    {
      unlockData.calligraphyBUnlocked = true;
      Debug.Log("[UnlockManager] 🎉 Unlocked: Calligraphy Style B!");
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Unlock Queries
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Check if a specific item variant is unlocked.
  /// </summary>
  public bool IsUnlocked(MiniGameType type, ItemVariant variant)
  {
    // Default is always unlocked
    if (variant == ItemVariant.Default) return true;

    switch (type)
    {
      case MiniGameType.Lantern:
        return unlockData.lanternBUnlocked;
      case MiniGameType.Origami:
        return unlockData.origamiBUnlocked;
      case MiniGameType.Calligraphy:
        return unlockData.calligraphyBUnlocked;
      default:
        return false;
    }
  }

  /// <summary>
  /// Get the latest unlock message (for RoomCompleteUI notification).
  /// Returns empty string if no new unlock.
  /// </summary>
  public string GetLatestUnlock()
  {
    // Check which unlock just happened based on completion count
    if (unlockData.roomCompletions == 1)
      return lanternStyleB != null ? lanternStyleB.designName : "Lantern Style B";
    if (unlockData.roomCompletions == 3)
      return origamiStyleB != null ? origamiStyleB.designName : "Origami Style B";
    if (unlockData.roomCompletions == 6)
      return calligraphyStyleB != null ? calligraphyStyleB.phraseReading : "Calligraphy Style B";

    return string.Empty;
  }

  /// <summary>
  /// Get the preview icon sprite for the latest unlock.
  /// Returns null if no new unlock or if sprite is not assigned.
  /// </summary>
  public Sprite GetLatestUnlockIcon()
  {
    // Check which unlock just happened based on completion count
    if (unlockData.roomCompletions == 1)
      return lanternStyleB != null ? lanternStyleB.previewIcon : null;
    if (unlockData.roomCompletions == 3)
      return origamiStyleB != null ? origamiStyleB.previewIcon : null;
    if (unlockData.roomCompletions == 6)
      return calligraphyStyleB != null ? calligraphyStyleB.previewIcon : null;

    return null;
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Selection
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Set player's selected variant for a mini-game type.
  /// </summary>
  public void SetSelectedVariant(MiniGameType type, ItemVariant variant)
  {
    // Don't allow selecting locked items
    if (!IsUnlocked(type, variant))
    {
      Debug.LogWarning($"[UnlockManager] Cannot select locked variant: {type} - {variant}");
      return;
    }

    switch (type)
    {
      case MiniGameType.Lantern:
        unlockData.selectedLantern = variant;
        break;
      case MiniGameType.Origami:
        unlockData.selectedOrigami = variant;
        break;
      case MiniGameType.Calligraphy:
        unlockData.selectedCalligraphy = variant;
        break;
    }

    Debug.Log($"[UnlockManager] Selected: {type} - {variant}");
    SaveData();
  }

  /// <summary>
  /// Get player's selected variant for a mini-game type.
  /// </summary>
  public ItemVariant GetSelectedVariant(MiniGameType type)
  {
    switch (type)
    {
      case MiniGameType.Lantern:
        return unlockData.selectedLantern;
      case MiniGameType.Origami:
        return unlockData.selectedOrigami;
      case MiniGameType.Calligraphy:
        return unlockData.selectedCalligraphy;
      default:
        return ItemVariant.Default;
    }
  }

  /// <summary>
  /// Get the appropriate CalligraphyDesign based on player's selection.
  /// </summary>
  public CalligraphyDesign GetCalligraphyDesign()
  {
    ItemVariant selected = unlockData.selectedCalligraphy;

    if (selected == ItemVariant.StyleB && calligraphyStyleB != null)
    {
      return calligraphyStyleB;
    }

    return calligraphyDefault;
  }

  /// <summary>
  /// Get the appropriate OrigamiDesign based on player's selection.
  /// </summary>
  public OrigamiDesign GetOrigamiDesign()
  {
    ItemVariant selected = unlockData.selectedOrigami;

    if (selected == ItemVariant.StyleB && origamiStyleB != null)
    {
      return origamiStyleB;
    }

    return origamiDefault;
  }

  /// <summary>
  /// Get the appropriate LanternDesign based on player's selection.
  /// </summary>
  public LanternDesign GetLanternDesign()
  {
    ItemVariant selected = unlockData.selectedLantern;

    if (selected == ItemVariant.StyleB && lanternStyleB != null)
    {
      return lanternStyleB;
    }

    return lanternDefault;
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Save/Load
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Save unlock data to PlayerPrefs.
  /// </summary>
  private void SaveData()
  {
    string json = JsonUtility.ToJson(unlockData);
    PlayerPrefs.SetString(SAVE_KEY, json);
    PlayerPrefs.Save();

    Debug.Log("[UnlockManager] Data saved");
  }

  /// <summary>
  /// Load unlock data from PlayerPrefs.
  /// </summary>
  private void LoadData()
  {
    if (PlayerPrefs.HasKey(SAVE_KEY))
    {
      string json = PlayerPrefs.GetString(SAVE_KEY);
      unlockData = JsonUtility.FromJson<UnlockData>(json);

      Debug.Log($"[UnlockManager] Data loaded. Completions: {unlockData.roomCompletions}");
    }
    else
    {
      Debug.Log("[UnlockManager] No save data found. Starting fresh.");
    }
  }

  /// <summary>
  /// Clear all unlock data (for testing).
  /// </summary>
  [ContextMenu("Clear Save Data")]
  public void ClearSaveData()
  {
    PlayerPrefs.DeleteKey(SAVE_KEY);
    unlockData = new UnlockData();
    Debug.Log("[UnlockManager] Save data cleared!");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Debug Helpers
  // ─────────────────────────────────────────────────────────────────────────

  [ContextMenu("Add Room Completion")]
  public void DebugAddCompletion()
  {
    CompleteRoom();
  }

  [ContextMenu("Print Unlock Status")]
  public void DebugPrintStatus()
  {
    Debug.Log("─────────────────────────────────────────");
    Debug.Log($"Room Completions: {unlockData.roomCompletions}");
    Debug.Log($"Lantern B: {(unlockData.lanternBUnlocked ? "✓ Unlocked" : "🔒 Locked")}");
    Debug.Log($"Origami B: {(unlockData.origamiBUnlocked ? "✓ Unlocked" : "🔒 Locked")}");
    Debug.Log($"Calligraphy B: {(unlockData.calligraphyBUnlocked ? "✓ Unlocked" : "🔒 Locked")}");
    Debug.Log("─────────────────────────────────────────");
    Debug.Log($"Selected Lantern: {unlockData.selectedLantern}");
    Debug.Log($"Selected Origami: {unlockData.selectedOrigami}");
    Debug.Log($"Selected Calligraphy: {unlockData.selectedCalligraphy}");
    Debug.Log("─────────────────────────────────────────");
  }
}
