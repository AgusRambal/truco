using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionsManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Dropdown dropdownResolucion;
    [SerializeField] private TMP_Dropdown dropdownGraficos;
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private Resolution[] resoluciones;

    void Start()
    {
        CargarResoluciones();
        CargarCalidadesGraficas();

        // Leer configuración guardada
        int resolIndex = PlayerPrefs.GetInt("ResolucionIndex", -1);
        int ventana = PlayerPrefs.GetInt("Ventana", 0); // 0 = maximizada (default), 1 = ventana común
        int calidad = PlayerPrefs.GetInt("CalidadGrafica", QualitySettings.GetQualityLevel());

        bool ponerEnVentana = ventana == 1;
        togglePantallaCompleta.isOn = ponerEnVentana;

        if (dropdownGraficos != null)
        {
            dropdownGraficos.value = calidad;
            dropdownGraficos.RefreshShownValue();
            QualitySettings.SetQualityLevel(calidad);
        }

        if (dropdownResolucion != null)
        {
            if (resolIndex >= 0 && resolIndex < resoluciones.Length)
            {
                dropdownResolucion.value = resolIndex;
                dropdownResolucion.RefreshShownValue();
                AplicarResolucion(resolIndex);
            }
        }

        // Volumen
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = Mathf.Clamp(musicVol, 0.0001f, 1f);
        SFXSlider.value = Mathf.Clamp(sfxVol, 0.0001f, 1f);

        AudioManager.Instance.SetMusicVolume(musicVol);
        AudioManager.Instance.SetSFXVolume(sfxVol);

        // Listeners
        if (dropdownResolucion != null)
        {
            dropdownResolucion.onValueChanged.AddListener((i) =>
            {
                PlayerPrefs.SetInt("ResolucionIndex", i);
                PlayerPrefs.Save();
                AplicarResolucion(i);
            });
        }

        togglePantallaCompleta.onValueChanged.AddListener((b) =>
        {
            PlayerPrefs.SetInt("Ventana", b ? 1 : 0); // true = ventana común, false = maximizada
            PlayerPrefs.Save();
            AplicarResolucion(dropdownResolucion.value);
        });

        if (dropdownGraficos != null)
        {
            dropdownGraficos.onValueChanged.AddListener((i) =>
            {
                PlayerPrefs.SetInt("CalidadGrafica", i);
                PlayerPrefs.Save();
                QualitySettings.SetQualityLevel(i);
            });
        }
    }

    public void CargarResoluciones()
    {
        Resolution[] todas = Screen.resolutions;

        if (dropdownResolucion != null)
        {
            dropdownResolucion.ClearOptions();
        }

        int indexActual = 0;
        var opciones = new List<string>();
        var resolucionesUnicas = new List<Resolution>();

        HashSet<string> tamañosUnicos = new();

        foreach (var r in todas)
        {
            string clave = $"{r.width}x{r.height}";

            if (!tamañosUnicos.Contains(clave))
            {
                tamañosUnicos.Add(clave);
                resolucionesUnicas.Add(r);
                opciones.Add($"{r.width} x {r.height}");

                if (r.width == Screen.currentResolution.width &&
                    r.height == Screen.currentResolution.height)
                {
                    indexActual = opciones.Count - 1;
                }
            }
        }

        resoluciones = resolucionesUnicas.ToArray(); // reemplazamos el array interno

        if (dropdownResolucion != null)
        {
            dropdownResolucion.AddOptions(opciones);
            dropdownResolucion.value = indexActual;
            dropdownResolucion.RefreshShownValue();
        }
    }

    private void AplicarResolucion(int index)
    {
        if (index < 0 || index >= resoluciones.Length) return;

        Resolution baseRes = resoluciones[index];
        RefreshRate refreshActual = Screen.currentResolution.refreshRateRatio;

        FullScreenMode modo = PlayerPrefs.GetInt("Ventana", 0) == 1
        ? FullScreenMode.Windowed
        : FullScreenMode.FullScreenWindow;

        Screen.SetResolution(baseRes.width, baseRes.height, modo, refreshActual);
    }


    private void AplicarPantallaCompleta(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    public void CargarCalidadesGraficas()
    {
        if (dropdownGraficos != null)
        {
            dropdownGraficos.ClearOptions();

            var opciones = new System.Collections.Generic.List<string>(QualitySettings.names);
            dropdownGraficos.AddOptions(opciones);
        }
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