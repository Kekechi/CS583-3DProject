using UnityEngine;

/// <summary>
/// Interface for all mini-games
/// Allows MiniGameController to manage games uniformly
/// </summary>
public interface IMiniGame
{
  /// <summary>
  /// The type of mini-game (for identification)
  /// </summary>
  MiniGameType GameType { get; }

  /// <summary>
  /// Start the mini-game
  /// Called when player triggers this game
  /// </summary>
  void StartGame();

  /// <summary>
  /// Stop the mini-game
  /// Called when game completes or is cancelled
  /// </summary>
  void StopGame();
}
