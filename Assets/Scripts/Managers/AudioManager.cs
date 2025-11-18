using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambienceSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip completionMusic;

    [Header("SFX Clips")]
    public AudioClip paperFoldSFX;
    public AudioClip brushStrokeSFX;
    public AudioClip lanternGlowSFX;
    public AudioClip itemPlacedSFX;
    public AudioClip harmonyChimeSFX;
    public AudioClip buttonClickSFX;

    [Header("Ambience Clips")]
    public AudioClip zenAmbienceLoop;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float ambienceVolume = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateVolumes();
        PlayAmbience(zenAmbienceLoop);
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (ambienceSource != null && clip != null)
        {
            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void StopAmbience()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();
    }

    public void UpdateVolumes()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume;

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        if (ambienceSource != null)
            ambienceSource.volume = ambienceVolume;
    }

    // Convenience methods for common sounds
    public void PlayPaperFold() => PlaySFX(paperFoldSFX);
    public void PlayBrushStroke() => PlaySFX(brushStrokeSFX);
    public void PlayLanternGlow() => PlaySFX(lanternGlowSFX);
    public void PlayItemPlaced() => PlaySFX(itemPlacedSFX);
    public void PlayHarmonyChime() => PlaySFX(harmonyChimeSFX);
    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
}
