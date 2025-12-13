using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Individual store item button component.
/// Shows item preview, lock state, and selection indicator.
/// </summary>
public class StoreItemButton : MonoBehaviour
{
  [Header("References")]
  [SerializeField] private Button button;
  [SerializeField] private Image itemImage;
  [SerializeField] private GameObject lockIcon;
  [SerializeField] private GameObject checkmarkIcon;
  [SerializeField] private TextMeshProUGUI itemNameText;
  [SerializeField] private TextMeshProUGUI unlockRequirementText;

  [Header("Visual States")]
  [SerializeField] private Color lockedTint = new Color(0.5f, 0.5f, 0.5f, 1f);
  [SerializeField] private Color unlockedTint = Color.white;
  [SerializeField] private Color selectedBorderColor = new Color(1f, 0.84f, 0f, 1f); // Gold

  [Header("Item Data")]
  [SerializeField] private string customItemName = ""; // Optional: leave empty to auto-generate
  [SerializeField] private Sprite itemIcon; private MiniGameType gameType;
  private ItemVariant variant;
  private bool isUnlocked;
  private bool isSelected;

  // Event for button clicks
  public event Action OnClicked;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────

  private void Awake()
  {
    if (button != null)
    {
      button.onClick.AddListener(HandleClick);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Setup the button with game type and variant info.
  /// </summary>
  public void Setup(MiniGameType type, ItemVariant itemVariant)
  {
    gameType = type;
    variant = itemVariant;

    // Set item name
    if (itemNameText != null)
    {
      itemNameText.text = GetItemName();
    }

    // Set item icon
    if (itemImage != null && itemIcon != null)
    {
      itemImage.sprite = itemIcon;
    }

    // Set unlock requirement text
    if (unlockRequirementText != null)
    {
      unlockRequirementText.text = GetUnlockRequirementText();
    }
  }

  /// <summary>
  /// Update button visual state based on unlock/selection status.
  /// </summary>
  public void SetState(bool unlocked, bool selected)
  {
    isUnlocked = unlocked;
    isSelected = selected;

    // Update button interactability
    if (button != null)
    {
      button.interactable = isUnlocked;
    }

    // Update visual tint
    if (itemImage != null)
    {
      itemImage.color = isUnlocked ? unlockedTint : lockedTint;
    }

    // Show/hide lock icon
    if (lockIcon != null)
    {
      lockIcon.SetActive(!isUnlocked);
    }

    // Show/hide checkmark
    if (checkmarkIcon != null)
    {
      checkmarkIcon.SetActive(isSelected);
    }

    // Show/hide unlock requirement text
    if (unlockRequirementText != null)
    {
      unlockRequirementText.gameObject.SetActive(!isUnlocked);
    }

    // Update border color (optional - requires Image component on button)
    UpdateBorderColor();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Handle button click.
  /// </summary>
  private void HandleClick()
  {
    OnClicked?.Invoke();
  }

  /// <summary>
  /// <summary>
  /// Get display name for this item.
  /// </summary>
  private string GetItemName()
  {
    // Use custom name if provided, otherwise auto-generate
    if (!string.IsNullOrEmpty(customItemName))
    {
      return customItemName;
    }

    string typeName = gameType.ToString();
    string variantName = variant == ItemVariant.Default ? "Default" : "Style B";
    return $"{typeName}\n{variantName}";
  }  /// Get unlock requirement text for locked items.
     /// </summary>
  private string GetUnlockRequirementText()
  {
    if (variant == ItemVariant.Default)
    {
      return ""; // Default is always unlocked
    }

    switch (gameType)
    {
      case MiniGameType.Lantern:
        return "Complete 1 room";
      case MiniGameType.Origami:
        return "Complete 3 rooms";
      case MiniGameType.Calligraphy:
        return "Complete 6 rooms";
      default:
        return "Locked";
    }
  }

  /// <summary>
  /// Update border color for selected state.
  /// </summary>
  private void UpdateBorderColor()
  {
    // Optional: Add border Image component and color it
    Image borderImage = button?.GetComponent<Image>();
    if (borderImage != null && isSelected && isUnlocked)
    {
      // Could set border color here if using Outline component
      // For now, checkmark is the primary selection indicator
    }
  }
}
