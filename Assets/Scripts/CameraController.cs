using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
  [Header("Camera Positions")]
  public Transform roomViewPosition;      // Overview of the room
  public Transform lanternGamePosition;   // Close-up on lantern table
  public Transform origamiGamePosition;   // Close-up on origami mat
  public Transform calligraphyPosition;   // Close-up on calligraphy desk

  [Header("Movement")]
  [Tooltip("Duration of camera transition in seconds")]
  public float transitionDuration = 1.5f;

  [Tooltip("Easing curve for smooth transitions")]
  public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

  public bool IsMoving { get; private set; }

  private Coroutine currentTransition;

  [ContextMenu("Test Room")]
  void TestRoom()
  {
    MoveTo(roomViewPosition);
  }

  [ContextMenu("Test Lantern")]
  void TestLantern()
  {
    MoveTo(lanternGamePosition);
  }

  /// <summary>
  /// Smoothly move camera to target position using coroutine
  /// </summary>
  public void MoveTo(Transform newTarget)
  {
    if (newTarget == null)
    {
      Debug.LogWarning("[CameraController] Target is null!");
      return;
    }

    // Stop previous transition if any
    if (currentTransition != null)
    {
      StopCoroutine(currentTransition);
    }

    currentTransition = StartCoroutine(TransitionToTarget(newTarget));
  }

  /// <summary>
  /// Coroutine for smooth camera transition
  /// </summary>
  IEnumerator TransitionToTarget(Transform target)
  {
    IsMoving = true;

    Vector3 startPosition = transform.position;
    Quaternion startRotation = transform.rotation;
    Vector3 endPosition = target.position;
    Quaternion endRotation = target.rotation;

    float elapsed = 0f;

    while (elapsed < transitionDuration)
    {
      elapsed += Time.deltaTime;
      float t = elapsed / transitionDuration;
      float curveValue = transitionCurve.Evaluate(t);

      transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);
      transform.rotation = Quaternion.Lerp(startRotation, endRotation, curveValue);

      yield return null;
    }

    // Snap to final position
    transform.position = endPosition;
    transform.rotation = endRotation;

    IsMoving = false;
    currentTransition = null;

    Debug.Log($"[CameraController] Arrived at {target.name}");
  }
}