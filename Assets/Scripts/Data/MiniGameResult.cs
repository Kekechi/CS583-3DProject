using UnityEngine;

/// <summary>
/// Types of mini-games in the room
/// </summary>
public enum MiniGameType
{
  Origami,
  Calligraphy,
  Lantern
}

/// <summary>
/// Base class for all mini-game results.
/// Guarantees each result has an item to place and tracks completion metadata.
/// </summary>
public abstract class MiniGameResult
{
  /// <summary>
  /// The GameObject prefab/instance to place in the room
  /// </summary>
  public abstract GameObject ItemInstance { get; }

  /// <summary>
  /// Which mini-game produced this result
  /// </summary>
  public abstract MiniGameType GameType { get; }

  /// <summary>
  /// How long it took to complete (in seconds)
  /// </summary>
  public float CompletionTime { get; set; }
}
