using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    [Header("Audios")]
    public List<AudioClip> menuTracks = new List<AudioClip>();
    public AudioClip hoverSound;

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadVolumeSettings();
    }

    public void PlayRandomMenuTrack()
    {
        if (menuTracks == null || menuTracks.Count == 0)
            return;

        int index = Random.Range(0, menuTracks.Count);
        PlayMusic(menuTracks[index]);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip)
    {
        uiSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetUIVolume(float value)
    {
        audioMixer.SetFloat("UIVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("UIVolume", value);
    }

    public void LoadVolumeSettings()
    {
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1));
        SetUIVolume(PlayerPrefs.GetFloat("UIVolume", 1));
    }
}
