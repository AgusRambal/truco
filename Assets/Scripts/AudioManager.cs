using System.Collections;
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

    private int indiceAnterior = -1;

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
        if (musicSource.isPlaying && musicSource.clip != null)
            return;

        if (menuTracks == null || menuTracks.Count == 0)
            return;

        int index;
        do
        {
            index = Random.Range(0, menuTracks.Count);
        } while (index == indiceAnterior && menuTracks.Count > 1);

        indiceAnterior = index;
        PlayMusic(menuTracks[index]);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.isPlaying && musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.loop = false;
        musicSource.Play();

        StartCoroutine(EsperarFinDeMusica());
    }

    private IEnumerator EsperarFinDeMusica()
    {
        float duracion = musicSource.clip.length;
        yield return new WaitForSeconds(duracion + 0.5f);

        PlayRandomMenuTrack();
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
