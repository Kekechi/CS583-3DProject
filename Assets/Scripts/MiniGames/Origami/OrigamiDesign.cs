using UnityEngine;

/// <summary>
/// ScriptableObject defining a single origami design.
/// Contains the arrow sequence, final model, and room item prefab.
/// Create new designs via: Right-click → Create → MiniGames → Origami Design
/// </summary>
[CreateAssetMenu(fileName = "NewOrigamiDesign", menuName = "MiniGames/Origami Design")]
public class OrigamiDesign : ScriptableObject
{
  [Header("Design Info")]
  [Tooltip("Name of this origami design (e.g., 'Crane', 'Boat')")]
  public string designName = "New Origami";

  [Header("Gameplay")]
  [Tooltip("Sequence of arrow keys the player must press")]
  public KeyCode[] arrowSequence = new KeyCode[]
  {
        KeyCode.UpArrow,
        KeyCode.RightArrow,
        KeyCode.DownArrow,
        KeyCode.DownArrow,
        KeyCode.LeftArrow,
        KeyCode.UpArrow
  };

  [Header("Visuals")]
  [Tooltip("3D model displayed after magic reveal")]
  public GameObject finalModelPrefab;

  [Tooltip("Prefab placed in the room after completion")]
  public GameObject roomItemPrefab;

  /// <summary>
  /// Total number of inputs required to complete this design
  /// </summary>
  public int SequenceLength => arrowSequence != null ? arrowSequence.Length : 0;

  /// <summary>
  /// Validate the design in editor
  /// </summary>
  private void OnValidate()
  {
    if (arrowSequence != null)
    {
      // Ensure only arrow keys are used 
      for (int i = 0; i < arrowSequence.Length; i++)
      {
        KeyCode key = arrowSequence[i];
        if (key != KeyCode.UpArrow &&
            key != KeyCode.DownArrow &&
            key != KeyCode.LeftArrow &&
            key != KeyCode.RightArrow)
        {
          Debug.LogWarning($"[OrigamiDesign] {designName}: Element {i} should be an arrow key, got {key}");
        }
      }
    }

    if (string.IsNullOrEmpty(designName))
    {
      designName = "Unnamed Design";
    }
  }
}
