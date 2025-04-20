using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Referencias")]
    public TMP_Dropdown dropdownResolucion;
    public Toggle togglePantallaCompleta;
    public TMP_Dropdown dropdownGraficos;
    public Button botonAplicar;

    private Resolution[] resoluciones;

    // 🔒 Variables temporales
    private int resolucionSeleccionadaIndex;
    private bool pantallaCompletaSeleccionada;
    private int calidadGraficaSeleccionada;

    void Start()
    {
        CargarResoluciones();
        CargarCalidadesGraficas();

        pantallaCompletaSeleccionada = Screen.fullScreen;
        togglePantallaCompleta.isOn = pantallaCompletaSeleccionada;

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
}
