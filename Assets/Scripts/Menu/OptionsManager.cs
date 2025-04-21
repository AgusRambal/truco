using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Dropdown dropdownResolucion;
    [SerializeField] private Toggle togglePantallaCompleta;
    [SerializeField] private TMP_Dropdown dropdownGraficos;
    [SerializeField] private Button botonAplicar;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;


    private Resolution[] resoluciones;
    private int resolucionSeleccionadaIndex;
    private bool pantallaCompletaSeleccionada;
    private int calidadGraficaSeleccionada;

    void Start()
    {
        CargarResoluciones();
        CargarCalidadesGraficas();

        pantallaCompletaSeleccionada = Screen.fullScreen;
        togglePantallaCompleta.isOn = pantallaCompletaSeleccionada;

        // Obtener valores guardados de volumen
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = Mathf.Clamp(PlayerPrefs.GetFloat("MusicVolume", 1f), 0.0001f, 1f);
        SFXSlider.value = Mathf.Clamp(PlayerPrefs.GetFloat("SFXVolume", 1f), 0.0001f, 1f);

        // También actualizar el AudioManager (por si no se hizo aún)
        AudioManager.Instance.SetMusicVolume(musicVol);
        AudioManager.Instance.SetSFXVolume(sfxVol);

        // Listeners → actualizan solo variables locales
        dropdownResolucion.onValueChanged.AddListener((i) => { resolucionSeleccionadaIndex = i; });
        togglePantallaCompleta.onValueChanged.AddListener((b) => { pantallaCompletaSeleccionada = b; });
        dropdownGraficos.onValueChanged.AddListener((i) => { calidadGraficaSeleccionada = i; });
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
        dropdownResolucion.value = indexActual;
        dropdownResolucion.RefreshShownValue();

        resolucionSeleccionadaIndex = indexActual;
    }

    public void CargarCalidadesGraficas()
    {
        dropdownGraficos.ClearOptions();

        var opciones = new System.Collections.Generic.List<string>(QualitySettings.names);
        dropdownGraficos.AddOptions(opciones);

        int actual = QualitySettings.GetQualityLevel();
        dropdownGraficos.value = actual;
        dropdownGraficos.RefreshShownValue();

        calidadGraficaSeleccionada = actual;
    }

    public void AplicarCambios()
    {
        Resolution resol = resoluciones[resolucionSeleccionadaIndex];
        Screen.SetResolution(resol.width, resol.height, pantallaCompletaSeleccionada);

        Screen.fullScreen = pantallaCompletaSeleccionada;
        QualitySettings.SetQualityLevel(calidadGraficaSeleccionada);

        Debug.Log("Opciones aplicadas: " + resol.width + "x" + resol.height +
                  " - Fullscreen: " + pantallaCompletaSeleccionada +
                  " - Calidad: " + QualitySettings.names[calidadGraficaSeleccionada]);
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
