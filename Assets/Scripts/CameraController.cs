using UnityEngine;

public class CameraController : MonoBehaviour
{
  [Header("Camera Positions")]
  public Transform roomViewPosition;      // Overview of the room
  public Transform lanternGamePosition;   // Close-up on lantern table
  public Transform origamiGamePosition;   // Close-up on origami mat
  public Transform calligraphyPosition;   // Close-up on calligraphy desk

  [Header("Movement")]
  public float transitionSpeed = 2f;

  private Transform targetPosition;

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
  public void MoveTo(Transform newTarget)
  {
    targetPosition = newTarget;
  }

  void Update()
  {
    if (targetPosition != null)
    {
      transform.position = Vector3.Lerp(transform.position,
          targetPosition.position, Time.deltaTime * transitionSpeed);
      transform.rotation = Quaternion.Lerp(transform.rotation,
          targetPosition.rotation, Time.deltaTime * transitionSpeed);
    }
  }
}