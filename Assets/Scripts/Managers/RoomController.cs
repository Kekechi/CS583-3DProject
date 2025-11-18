using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomController : MonoBehaviour
{
    // [Header("Room Data")]
    // public RoomData currentRoomData;

    [Header("Placement System")]
    public Transform[] placementSpots; // 3 spots per room
    public GameObject placementHighlight;
    private int nextPlacementIndex = 0;

    [Header("Harmony Meter")]
    public Slider harmonyMeterSlider;
    public Image harmonyMeterFill;
    public Color harmonyStartColor = Color.yellow;
    public Color harmonyCompleteColor = Color.green;
    private int totalItemsNeeded = 3;

    [Header("Lighting")]
    public Light ambientLight;
    public Light[] lanternLights;

    [Header("Camera")]
    public Camera mainCamera;
    public Transform menuCameraPosition;
    public Transform gameCameraPosition;
    public Transform completionCameraPosition;
    public float cameraTransitionSpeed = 2f;

    private Coroutine cameraTransitionCoroutine;

    void Start()
    {
        InitializeRoom();
    }

    void InitializeRoom()
    {
        nextPlacementIndex = 0;

        if (harmonyMeterSlider != null)
        {
            harmonyMeterSlider.maxValue = totalItemsNeeded;
            harmonyMeterSlider.value = 0;
        }

        if (placementHighlight != null)
            placementHighlight.SetActive(false);
    }

    public void UpdateHarmonyMeter(int itemsPlaced)
    {
        if (harmonyMeterSlider != null)
        {
            harmonyMeterSlider.value = itemsPlaced;

            // Update color based on progress
            if (harmonyMeterFill != null)
            {
                float t = (float)itemsPlaced / totalItemsNeeded;
                harmonyMeterFill.color = Color.Lerp(harmonyStartColor, harmonyCompleteColor, t);
            }
        }

        if (itemsPlaced >= totalItemsNeeded)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHarmonyChime();
        }
    }

    public void ShowPlacementUI()
    {
        if (nextPlacementIndex < placementSpots.Length)
        {
            // Highlight the next placement spot
            if (placementHighlight != null)
            {
                placementHighlight.transform.position = placementSpots[nextPlacementIndex].position;
                placementHighlight.SetActive(true);
            }

            // Move camera to placement view
            MoveCameraTo(gameCameraPosition);

            // Auto-place after a moment (simplified - you can add interaction later)
            StartCoroutine(AutoPlaceItem());
        }
    }

    IEnumerator AutoPlaceItem()
    {
        yield return new WaitForSeconds(1.5f);

        PlaceItem();
    }

    void PlaceItem()
    {
        if (placementHighlight != null)
            placementHighlight.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayItemPlaced();

        nextPlacementIndex++;

        if (GameManager.Instance != null)
            GameManager.Instance.OnItemPlaced();
    }

    public void PlayCompletionSequence()
    {
        StartCoroutine(CompletionSequenceCoroutine());
    }

    IEnumerator CompletionSequenceCoroutine()
    {
        // Move camera to completion view
        MoveCameraTo(completionCameraPosition);

        // Enhance lighting
        if (ambientLight != null)
        {
            float originalIntensity = ambientLight.intensity;
            float targetIntensity = originalIntensity * 1.5f;
            float elapsed = 0f;
            float duration = 2f;

            while (elapsed < duration)
            {
                ambientLight.intensity = Mathf.Lerp(originalIntensity, targetIntensity, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Turn on lantern lights
        foreach (Light lantern in lanternLights)
        {
            if (lantern != null)
                lantern.enabled = true;
        }

        yield return new WaitForSeconds(3f);

        // Show summary
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            GameManager.Instance.uiManager.ShowSummary();
    }

    public void MoveCameraTo(Transform targetPosition)
    {
        if (cameraTransitionCoroutine != null)
            StopCoroutine(cameraTransitionCoroutine);

        cameraTransitionCoroutine = StartCoroutine(CameraTransitionCoroutine(targetPosition));
    }

    IEnumerator CameraTransitionCoroutine(Transform targetPosition)
    {
        if (mainCamera == null || targetPosition == null)
            yield break;

        Transform camTransform = mainCamera.transform;
        Vector3 startPos = camTransform.position;
        Quaternion startRot = camTransform.rotation;
        float elapsed = 0f;
        float duration = 1f / cameraTransitionSpeed;

        while (elapsed < duration)
        {
            camTransform.position = Vector3.Lerp(startPos, targetPosition.position, elapsed / duration);
            camTransform.rotation = Quaternion.Lerp(startRot, targetPosition.rotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.position = targetPosition.position;
        camTransform.rotation = targetPosition.rotation;
    }

    // public void LoadRoomData(RoomData data)
    // {
    //     // currentRoomData = data;
    //     // Apply room-specific settings (lighting, theme, etc.)
    //     // This will be expanded when ScriptableObjects are implemented
    // }
}
