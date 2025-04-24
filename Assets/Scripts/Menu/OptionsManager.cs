using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Dropdown dropdownResolucion;
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownGraficos;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private Resolution[] resoluciones;

    void Start()
    {
        CargarResoluciones();
        CargarCalidadesGraficas();

        // Leer configuración guardada
        int resolIndex = PlayerPrefs.GetInt("ResolucionIndex", -1);
        int fullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        int calidad = PlayerPrefs.GetInt("CalidadGrafica", QualitySettings.GetQualityLevel());

        bool pantallaCompletaSeleccionada = fullscreen == 1;
        togglePantallaCompleta.isOn = pantallaCompletaSeleccionada;
        Screen.fullScreen = pantallaCompletaSeleccionada;

        dropdownGraficos.value = calidad;
        dropdownGraficos.RefreshShownValue();
        QualitySettings.SetQualityLevel(calidad);

        if (resolIndex >= 0 && resolIndex < resoluciones.Length)
        {
            dropdownResolucion.value = resolIndex;
            dropdownResolucion.RefreshShownValue();
            AplicarResolucion(resolIndex);
        }

        // Obtener valores guardados de volumen
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = Mathf.Clamp(musicVol, 0.0001f, 1f);
        SFXSlider.value = Mathf.Clamp(sfxVol, 0.0001f, 1f);

        AudioManager.Instance.SetMusicVolume(musicVol);
        AudioManager.Instance.SetSFXVolume(sfxVol);

        dropdownResolucion.onValueChanged.AddListener((i) =>
        {
            PlayerPrefs.SetInt("ResolucionIndex", i);
            PlayerPrefs.Save();
            AplicarResolucion(i);
        });

        togglePantallaCompleta.onValueChanged.AddListener((b) =>
        {
            PlayerPrefs.SetInt("Fullscreen", b ? 1 : 0);
            PlayerPrefs.Save();
            AplicarPantallaCompleta(b);
        });

        dropdownGraficos.onValueChanged.AddListener((i) =>
        {
            PlayerPrefs.SetInt("CalidadGrafica", i);
            PlayerPrefs.Save();
            QualitySettings.SetQualityLevel(i);
        });
    }

    public void CargarResoluciones()
    {
        resoluciones = Screen.resolutions;
        dropdownResolucion.ClearOptions();

        int indexActual = 0;
        var opciones = new System.Collections.Generic.List<string>();

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                indexActual = i;
            }
        }

        dropdownResolucion.AddOptions(opciones);
        dropdownResolucion.RefreshShownValue();
    }

    private void AplicarResolucion(int index)
    {
        Resolution resol = resoluciones[index];
        Screen.SetResolution(resol.width, resol.height, Screen.fullScreen);
    }

    private void AplicarPantallaCompleta(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void CargarCalidadesGraficas()
    {
        dropdownGraficos.ClearOptions();

        var opciones = new System.Collections.Generic.List<string>(QualitySettings.names);
        dropdownGraficos.AddOptions(opciones);
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }

    public void OnUIVolumeChanged(float value)
    {
        AudioManager.Instance.SetUIVolume(value);
    }

    [ContextMenu("Resetear Volumen")]
    public void ResetAudioConfig()
    {
        AudioManager.Instance.SetMusicVolume(1f);
        AudioManager.Instance.SetSFXVolume(1f);
        AudioManager.Instance.SetUIVolume(1f);

        musicSlider.value = 1f;
        SFXSlider.value = 1f;

        PlayerPrefs.Save();
    }
}