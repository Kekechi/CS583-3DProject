using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomData", menuName = "Zen Atelier/Room Data")]
public class RoomData : ScriptableObject
{
    [Header("Room Info")]
    public string roomName;
    public string roomDescription;

    [Header("Visual Theme")]
    public Color ambientLightColor = new Color(1f, 0.95f, 0.8f);
    public float ambientLightIntensity = 0.7f;
    public Material tatami;
    public Material fusuma;
    public Material wood;

    [Header("Mini-Game Difficulty")]
    [Range(0.1f, 1f)]
    public float origamiDifficulty = 0.5f;

    [Range(0.1f, 1f)]
    public float calligraphyDifficulty = 0.5f;

    [Range(0.1f, 1f)]
    public float lanternDifficulty = 0.5f;

    [Header("Audio")]
    public AudioClip roomAmbience;
}
