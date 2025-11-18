using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanternGame : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Time player must keep brightness in green zone to win")]
    public float goalTime = 10f;

    [Tooltip("How fast brightness changes when holding/releasing")]
    public float brightnessChangeSpeed = 0.5f;

    [Tooltip("Input key to increase brightness")]
    public KeyCode inputKey = KeyCode.Space;

    [Header("Green Zone Thresholds")]
    [Range(0f, 1f)] public float greenZoneMin = 0.4f;
    [Range(0f, 1f)] public float greenZoneMax = 0.6f;

    [Header("Lantern Prefab")]
    public GameObject lanternPrefab; // The lantern prefab to spawn
    public Transform spawnPoint; // Where to spawn the lantern (optional)
    public string paperChildName = "Paper"; // Name of the paper cube child in prefab

    [Header("Lantern Visual")]
    public string emissionProperty = "_EmissionColor"; // Standard shader emission property
    public Color emissionColorMin = Color.black; // Dim/off state
    public Color emissionColorMax = Color.yellow; // Bright state
    [Range(0f, 10f)] public float emissionIntensityMultiplier = 2f;

    [Header("Progress Bar - TODO")]
    public GameObject progressBarPlaceholder; // Placeholder object for now

    // Game state
    private float brightness = 0.5f;
    private float timeInGreenZone = 0f;
    private bool isPlaying = false;

    // Spawned lantern references
    private GameObject spawnedLantern;
    private Renderer lanternPaperRenderer;

    void Update()
    {
        if (!isPlaying) return;

        // Input: Hold to increase brightness, release to decrease
        if (Input.GetKey(inputKey))
        {
            brightness += brightnessChangeSpeed * Time.deltaTime;
        }
        else
        {
            brightness -= brightnessChangeSpeed * Time.deltaTime;
        }

        // Clamp brightness between 0 and 1
        brightness = Mathf.Clamp01(brightness);

        // Update lantern emission
        UpdateLanternEmission();

        // Track time in green zone
        if (IsInGreenZone())
        {
            timeInGreenZone += Time.deltaTime;
            Debug.Log($"Time in green zone: {timeInGreenZone:F2}s / {goalTime:F2}s");
        }
        else
        {
            // Optional: could reset or slowly decrease time if out of zone
            // timeInGreenZone = 0f; // Uncomment for harder difficulty
        }

        // Check win condition
        if (timeInGreenZone >= goalTime)
        {
            OnGameComplete();
        }
    }

    public void StartGame()
    {
        isPlaying = true;
        brightness = 0.5f;
        timeInGreenZone = 0f;

        Debug.Log("[LanternGame] Mini-game started! Hold SPACE to brighten the lantern.");

        // Spawn the lantern prefab
        SpawnLantern();

        if (progressBarPlaceholder != null)
            progressBarPlaceholder.SetActive(true);
    }

    void SpawnLantern()
    {
        if (lanternPrefab == null)
        {
            Debug.LogError("[LanternGame] Lantern prefab is not assigned!");
            return;
        }

        // Determine spawn position
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Instantiate the prefab
        spawnedLantern = Instantiate(lanternPrefab, spawnPosition, spawnRotation);
        Debug.Log($"[LanternGame] Lantern spawned at {spawnPosition}");

        // Find the paper renderer in children
        FindPaperRenderer();
    }

    void FindPaperRenderer()
    {
        if (spawnedLantern == null) return;

        // Try to find by name first
        Transform paperTransform = spawnedLantern.transform.Find(paperChildName);

        if (paperTransform != null)
        {
            lanternPaperRenderer = paperTransform.GetComponent<Renderer>();
            Debug.Log($"[LanternGame] Found paper renderer by name: {paperChildName}");
        }
        else
        {
            // Fallback: search all children for a renderer
            Renderer[] renderers = spawnedLantern.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                // Look for renderer with emissive material
                if (r.material.HasProperty(emissionProperty))
                {
                    lanternPaperRenderer = r;
                    Debug.Log($"[LanternGame] Found paper renderer on: {r.gameObject.name}");
                    break;
                }
            }
        }

        if (lanternPaperRenderer == null)
        {
            Debug.LogWarning($"[LanternGame] Could not find paper renderer! Looking for child named '{paperChildName}' or renderer with emission property.");
        }
    }

    void UpdateLanternEmission()
    {
        if (lanternPaperRenderer == null) return;

        // Lerp between min and max emission colors based on brightness
        Color baseEmission = Color.Lerp(emissionColorMin, emissionColorMax, brightness);

        // Multiply by intensity for HDR glow effect
        Color finalEmission = baseEmission * emissionIntensityMultiplier;

        // Apply to material
        lanternPaperRenderer.material.SetColor(emissionProperty, finalEmission);

        // Enable emission keyword for Standard shader
        lanternPaperRenderer.material.EnableKeyword("_EMISSION");
    }

    bool IsInGreenZone()
    {
        return brightness >= greenZoneMin && brightness <= greenZoneMax;
    }

    void OnGameComplete()
    {
        isPlaying = false;
        Debug.Log("[LanternGame] Game completed! Perfect harmony achieved!");

        if (progressBarPlaceholder != null)
            progressBarPlaceholder.SetActive(false);

        // Play audio
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayLanternGlow();

        // Trigger game manager event
        if (GameManager.Instance != null)
            GameManager.Instance.OnMiniGameComplete();

        // Keep lantern visible briefly before cleanup
        StartCoroutine(CleanupLanternAfterDelay(2f));
    }

    IEnumerator CleanupLanternAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroySpawnedLantern();
    }

    public void StopGame()
    {
        isPlaying = false;
        timeInGreenZone = 0f;
        DestroySpawnedLantern();
        Debug.Log("[LanternGame] Game stopped");
    }

    void DestroySpawnedLantern()
    {
        if (spawnedLantern != null)
        {
            Destroy(spawnedLantern);
            lanternPaperRenderer = null;
            Debug.Log("[LanternGame] Spawned lantern destroyed");
        }
    }

    // Debug visualization in editor
    void OnGUI()
    {
        if (!isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"Brightness: {brightness:F2}");
        GUILayout.Label($"In Green Zone: {IsInGreenZone()}");
        GUILayout.Label($"Progress: {timeInGreenZone:F2}s / {goalTime:F2}s");
        GUILayout.EndArea();
    }
}
