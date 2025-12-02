using UnityEngine;

/// <summary>
/// ScriptableObject containing data for a calligraphy design.
/// Each design represents one Japanese phrase to trace.
/// </summary>
[CreateAssetMenu(fileName = "NewCalligraphyDesign", menuName = "MiniGames/Calligraphy Design")]
public class CalligraphyDesign : ScriptableObject
{
  [Header("Phrase Info")]
  [Tooltip("The Japanese phrase (e.g., 一期一会)")]
  public string phraseName;

  [Tooltip("Romanized reading (e.g., Ichigo Ichie)")]
  public string phraseReading;

  [Tooltip("English meaning (e.g., Once-in-a-lifetime encounter)")]
  public string phraseMeaning;

  [Header("Prefabs")]
  [Tooltip("Paper prefab containing CalligraphyPaper component and all visuals")]
  public GameObject paperPrefab;

  [Tooltip("Scroll prefab to place in room after completion")]
  public GameObject scrollPrefab;
}
