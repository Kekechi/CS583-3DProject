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

  [Header("Orbit Controls")]
  [Tooltip("Mouse sensitivity for camera rotation")]
  public float mouseSensitivity = 2f;

  [Tooltip("Minimum horizontal rotation angle (degrees)")]
  public float minHorizontalAngle = -90f;

  [Tooltip("Maximum horizontal rotation angle (degrees)")]
  public float maxHorizontalAngle = 90f;

  [Tooltip("Minimum vertical rotation angle (degrees)")]
  public float minVerticalAngle = -30f;

  [Tooltip("Maximum vertical rotation angle (degrees)")]
  public float maxVerticalAngle = 30f;

  public bool IsMoving { get; private set; }

  private Coroutine currentTransition;
  private float currentYaw = 0f;   // Horizontal rotation
  private float currentPitch = 0f; // Vertical rotation

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

  void Update()
  {
    // Only allow controls when not transitioning
    if (!IsMoving)
    {
      // Right-click to pan camera
      if (Input.GetMouseButton(1))
      {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY; // Inverted for natural feel

        // Clamp rotation angles
        currentYaw = Mathf.Clamp(currentYaw, minHorizontalAngle, maxHorizontalAngle);
        currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

        // Apply rotation (position stays fixed, only rotation changes)
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
      }
    }
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

    // Sync rotation tracking with new rotation
    Vector3 eulerAngles = endRotation.eulerAngles;
    currentYaw = eulerAngles.y;
    currentPitch = eulerAngles.x;

    // Normalize angles to -180 to 180 range
    if (currentYaw > 180f) currentYaw -= 360f;
    if (currentPitch > 180f) currentPitch -= 360f;

    IsMoving = false;
    currentTransition = null;

    Debug.Log($"[CameraController] Arrived at {target.name}");
  }
}