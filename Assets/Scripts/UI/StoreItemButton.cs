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
  [SerializeField] private float hoverScale = 1.05f;
  [SerializeField] private float animationSpeed = 0.1f;

  [Header("Item Data")]
  [SerializeField] private string customItemName = ""; // Optional: leave empty to auto-generate
  [SerializeField] private Sprite itemIcon;

  private MiniGameType gameType;
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

      // Add hover effects using EventTrigger or button navigation
      var buttonTransform = button.transform;
      var selectable = button;

      // We'll use Button's built-in navigation to detect hover
      // But for better control, let's add pointer enter/exit handlers
      var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
      if (trigger == null)
      {
        trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
      }

      // Pointer Enter
      var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
      entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
      entryEnter.callback.AddListener((data) => { OnPointerEnter(); });
      trigger.triggers.Add(entryEnter);

      // Pointer Exit
      var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
      entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
      entryExit.callback.AddListener((data) => { OnPointerExit(); });
      trigger.triggers.Add(entryExit);
    }
  }

  private void OnPointerEnter()
  {
    if (button != null && button.interactable)
    {
      StartCoroutine(ScaleButton(hoverScale));

      // Play hover sound
      if (AudioManager.Instance != null)
      {
        AudioManager.Instance.PlaySpotClick(); // Subtle hover sound
      }
    }
  }

  private void OnPointerExit()
  {
    if (button != null)
    {
      StartCoroutine(ScaleButton(1f));
    }
  }

  private System.Collections.IEnumerator ScaleButton(float targetScale)
  {
    Vector3 startScale = transform.localScale;
    Vector3 endScale = Vector3.one * targetScale;
    float elapsed = 0f;

    while (elapsed < animationSpeed)
    {
      elapsed += Time.deltaTime;
      float t = elapsed / animationSpeed;
      transform.localScale = Vector3.Lerp(startScale, endScale, t);
      yield return null;
    }

    transform.localScale = endScale;
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
    // Play selection bounce animation
    if (isUnlocked)
    {
      StartCoroutine(SelectionBounce());
    }

    OnClicked?.Invoke();
  }

  /// <summary>
  /// Bounce animation when button is selected.
  /// </summary>
  private System.Collections.IEnumerator SelectionBounce()
  {
    // Quick scale up
    yield return ScaleButton(1.15f);

    // Scale back to normal
    yield return ScaleButton(1f);
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
