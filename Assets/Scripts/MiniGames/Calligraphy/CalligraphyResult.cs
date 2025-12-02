using System;
using UnityEngine;

/// <summary>
/// Data container for completed calligraphy mini-game result.
/// Passed to MiniGameController when game completes.
/// </summary>
[Serializable]
public class CalligraphyResult : MiniGameResult
{
  public override GameObject ItemInstance => roomItemPrefab;
  public override MiniGameType GameType => MiniGameType.Calligraphy;

  /// <summary>
  /// The design that was completed
  /// </summary>
  public CalligraphyDesign design;

  /// <summary>
  /// The prefab to instantiate in the room
  /// </summary>
  public GameObject roomItemPrefab;
}
