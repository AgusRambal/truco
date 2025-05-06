using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    [Header("Cartas en pantalla")]
    [SerializeField] private List<Transform> cartasMenu;

    [Header("Mazo")]
    [SerializeField] private List<CartaSO> todasLasCartas;   // TODAS (originales + personalizadas)
    [SerializeField] private List<CartaSO> mazoDefault;      // 40 originales
    [SerializeField] private List<CartaSO> mazoPersonalizado;
    public List<CartaSO> cartasCompradas = new();

    [Header("Referencias")]
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private TMP_Text creditosTexto;
    [SerializeField] private GameObject textoAdvertenciaPrefab;
    [SerializeField] private ScrollResetter[] miScrollRects;
    [SerializeField] private Toggle toggleMachineLearning;

    [Header("Botones principales")]
    [SerializeField] private List<Transform> botonesMenuPrincipal;

    [Header("Botones del menú de juego")]
    [SerializeField] private List<Transform> botonesPuntos;
    [SerializeField] private TMP_Dropdown dropdownEstiloIA;
    [SerializeField] private RectTransform coinsPlace;
    [SerializeField] private Vector2 offset;

    [Header("Configuración de animación")]
    [SerializeField] private float delayEntreBotones = 0.05f;
    [SerializeField] private float duracionAnimacion = 0.25f;

    [Header("Zona de personalización")]
    [SerializeField] private Transform contenedorCartasCompradas; 
    [SerializeField] private TMP_Text texto; 
    [SerializeField] private GameObject prefabCartaVisual;

    [Header("Zona de Estadisticas")]
    [SerializeField] private Color colorTextoEstadisticas;
    [SerializeField] private TMP_Text jugadasText;
    [SerializeField] private TMP_Text victoriasText;
    [SerializeField] private TMP_Text derrotasText;
    [SerializeField] private TMP_Text vecesQueTeFuisteText;
    [SerializeField] private TMP_Text trucosCantadosText;
    [SerializeField] private TMP_Text trucosAceptadosText;
    [SerializeField] private TMP_Text retrucosCantadosText;
    [SerializeField] private TMP_Text retrucosAceptadosText;
    [SerializeField] private TMP_Text valeCuatroCantadosText;
    [SerializeField] private TMP_Text valeCuatroAceptadosText;
    [SerializeField] private TMP_Text envidosCantadosText;
    [SerializeField] private TMP_Text envidosAceptadosText;
    [SerializeField] private TMP_Text realEnvidosCantadosText;
    [SerializeField] private TMP_Text realEnvidosAceptadosText;
    [SerializeField] private TMP_Text faltaEnvidosCantadosText;
    [SerializeField] private TMP_Text faltaEnvidosAceptadosText;
    [SerializeField] private TMP_Text trucosGanadosText;
    [SerializeField] private TMP_Text RetrucosGanadosText;
    [SerializeField] private TMP_Text ValeCuatroGanadosText;
    [SerializeField] private TMP_Text trucosPerdidosText;
    [SerializeField] private TMP_Text RetrucosPerdidosText;
    [SerializeField] private TMP_Text ValeCuatroPerdidosText;
    [SerializeField] private TMP_Text EnvidosGanadosText;
    [SerializeField] private TMP_Text RealEnvidosGanadosText;
    [SerializeField] private TMP_Text FaltaEnvidosGanadosText;
    [SerializeField] private TMP_Text EnvidosPerdidosText;
    [SerializeField] private TMP_Text RealEnvidosPerdidosText;
    [SerializeField] private TMP_Text FaltaEnvidosPerdidosText;
    [SerializeField] private TMP_Text rachaActualText;
    [SerializeField] private TMP_Text rachaMaximaText;
    
    private void Awake()
    {
        SaveSystem.CargarDatos();
        ActualizarCreditosUI();

        Time.timeScale = 1f;

        mazoPersonalizado = CartaSaveManager.CargarCartas(todasLasCartas);
        if (mazoPersonalizado.Count != 40)
            mazoPersonalizado = new List<CartaSO>(mazoDefault);

        cartasCompradas = CargarCartasCompradas();
    }

    private List<CartaSO> CargarCartasCompradas()
    {
        var ids = CartaCompraManager.ObtenerIDsComprados();
        return todasLasCartas.Where(c => ids.Contains(c.id)).ToList();
    }

    private void Start()
    {
        AudioManager.Instance.PlayRandomMenuTrack();
        UpdateStats();
        SpawnCartasCompradas();
        SetScrolls();

        SaveSystem.Datos.usarAprendizaje = false;
        SaveSystem.GuardarDatos();

        toggleMachineLearning.onValueChanged.RemoveListener(OnToggleMachineLearning);
        toggleMachineLearning.isOn = false;
        toggleMachineLearning.onValueChanged.AddListener(OnToggleMachineLearning);

        if (cartasCompradas.Count > 0)
        {
            texto.gameObject.SetActive(false);
        }
    }

    private void SetScrolls()
    {
#if UNITY_STANDALONE_OSX
        foreach (var item in miScrollRects)
        {
            item.GetComponent<ScrollRect>().scrollSensitivity = .1f;
        }
#else
        foreach (var item in miScrollRects)
        {
            item.GetComponent<ScrollRect>().scrollSensitivity = 4f;
        }
#endif
    }

    private void OnToggleMachineLearning(bool isOn)
    {
        SaveSystem.Datos.usarAprendizaje = isOn;
        SaveSystem.GuardarDatos();
    }
    
    private void SpawnCartasCompradas()
    {
        foreach (var carta in cartasCompradas)
        {
            GameObject go = Instantiate(prefabCartaVisual, contenedorCartasCompradas);
            var visual = go.GetComponent<CartaVisual>();

            if (visual != null)
            {
                visual.Configurar(carta);

                // Solo prende el marco si esta carta está actualmente en el mazo personalizado
                bool estaEnUso = mazoPersonalizado.Any(c => c.id == carta.id);
                visual.SetMarcoSeleccion(estaEnUso);
            }
        }
    }

    public void AgregarCartaCompradaYSpawnear(CartaSO carta)
    {
        if (cartasCompradas.Any(c => c.id == carta.id))
            return;

        cartasCompradas.Add(carta);

        texto.gameObject.SetActive(false);
        
        GameObject go = Instantiate(prefabCartaVisual, contenedorCartasCompradas);
        var visual = go.GetComponent<CartaVisual>();

        if (visual != null)
        {
            visual.Configurar(carta);

            // Prende el marco solo si se está usando en el mazo
            bool estaEnUso = mazoPersonalizado.Any(c => c.id == carta.id);
            visual.SetMarcoSeleccion(estaEnUso);
        }
    }

    public void Jugar(int puntos)
    {
        Utils.ParametrosDePartida.puntosParaGanar = puntos;
        Utils.ParametrosDePartida.estiloSeleccionado = (EstiloIA)dropdownEstiloIA.value;
        Utils.ParametrosDePartida.gananciaCalculada = CalcularGanancia((EstiloIA)dropdownEstiloIA.value, puntos);
        Utils.ParametrosDePartida.cartasSeleccionadas = new List<CartaSO>(mazoPersonalizado);
        Utils.ParametrosDePartida.usarAprendizaje = SaveSystem.Datos.usarAprendizaje;


        DOTween.KillAll();
        SceneManager.LoadScene("Gameplay");
    }

    public void ReemplazarCartaElegida(CartaSO carta)
    {
        //Sacar otras cartas que sean del mismo valor y palo
        for (int i = 0; i < mazoPersonalizado.Count; i++)
        {
            if (mazoPersonalizado[i].valor == carta.valor && mazoPersonalizado[i].palo == carta.palo)
            {
                mazoPersonalizado[i] = carta;
                break;
            }
        }

        //Forzar que las otras cartas visuales desactiven su marco
        foreach (var visual in contenedorCartasCompradas.GetComponentsInChildren<CartaVisual>())
        {
            if (visual.EsMismaCarta(carta) && visual.CartaId != carta.id)
            {
                visual.ForzarDesmarcar();
            }
        }

        SaveSystem.Datos.cartaSeleccionada = carta.id;
        CartaSaveManager.GuardarCartas(mazoPersonalizado);
        SaveSystem.GuardarDatos();
    }

    public CartaSO ObtenerCartaOriginal(CartaSO cartaNueva)
    {
        return mazoDefault.FirstOrDefault(c => c.valor == cartaNueva.valor && c.palo == cartaNueva.palo);
    }

    public bool EstaCartaEnUso(CartaSO carta)
    {
        return mazoPersonalizado.Any(c => c.id == carta.id);
    }

    private int CalcularGanancia(EstiloIA estilo, int puntosFinales)
    {
        int baseGanancia = estilo switch
        {
            EstiloIA.Conservador => Random.Range(20, 31),   // Muy fácil
            EstiloIA.Canchero => Random.Range(30, 41),      // Normal
            EstiloIA.Calculador => Random.Range(35, 50),    // Estratega medio
            EstiloIA.Agresivo => Random.Range(45, 60),      // Difícil
            EstiloIA.Mentiroso => Random.Range(50, 70),     // Complicado
            EstiloIA.Caotico => Random.Range(60, 80),       // Muy difícil
            _ => 30
        };

        if (puntosFinales == 30)
            baseGanancia = Mathf.RoundToInt(baseGanancia * 1.5f); // Bonus por victoria de partida a 30 puntos

        //Bonus por racha
        int racha = Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RachaVictoriasActual);
        int bonusRacha = racha * 10;
        baseGanancia += bonusRacha;

        return baseGanancia;
    }


    public void MostrarMenuJuego()
    {
        for (int i = 0; i < botonesMenuPrincipal.Count; i++)
        {
            Transform btn = botonesMenuPrincipal[i];
            btn.DOScale(Vector3.zero, duracionAnimacion)
               .SetEase(Ease.InBack)
               .SetDelay(i * delayEntreBotones)
               .OnComplete(() => btn.gameObject.SetActive(false));
        }

        for (int i = 0; i < botonesPuntos.Count; i++)
        {
            Transform btn = botonesPuntos[i];
            btn.localScale = Vector3.zero;
            btn.gameObject.SetActive(true);

            btn.DOScale(Vector3.one, duracionAnimacion)
               .SetEase(Ease.OutBack)
               .SetDelay(botonesMenuPrincipal.Count * delayEntreBotones + i * delayEntreBotones);
        }
    }

    public void VolverAlMenuPrincipal()
    {
        for (int i = 0; i < botonesPuntos.Count; i++)
        {
            Transform btn = botonesPuntos[i];
            btn.DOScale(Vector3.zero, duracionAnimacion)
               .SetEase(Ease.InBack)
               .SetDelay(i * delayEntreBotones)
               .OnComplete(() => btn.gameObject.SetActive(false));
        }

        for (int i = 0; i < botonesMenuPrincipal.Count; i++)
        {
            Transform btn = botonesMenuPrincipal[i];
            btn.localScale = Vector3.zero;
            btn.gameObject.SetActive(true);

            btn.DOScale(Vector3.one, duracionAnimacion)
               .SetEase(Ease.OutBack)
               .SetDelay(botonesPuntos.Count * delayEntreBotones + i * delayEntreBotones);
        }
    }

    public void AbrirPopUp(RectTransform rectTransform)
    {
        if (!rectTransform.gameObject.activeSelf)
        {
            rectTransform.gameObject.SetActive(true);
            rectTransform.localScale = Vector3.zero;
        }

        foreach (ScrollResetter scroll in miScrollRects)
        {
            if (scroll.gameObject.activeInHierarchy)
            {
                scroll.ResetScroll();
            }
        }

        rectTransform.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack);
    }

    public void CerrarPopUp(RectTransform rectTransform)
    {
        rectTransform.DOScale(Vector3.zero, animationDuration - 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                rectTransform.gameObject.SetActive(false);
            });
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ActualizarCreditosUI()
    {
        creditosTexto.text = $"{SaveSystem.Datos.monedas}";
    }

    [ContextMenu("Resetear Créditos")]
    public void ResetearCreditosDesdeEditor()
    {
        SaveSystem.Datos.monedas = 0;
        SaveSystem.GuardarDatos();
        Debug.Log("Créditos reseteados desde el editor.");
        ActualizarCreditosUI();
    }

    [ContextMenu("Sumar 100 Créditos")]
    public void SumarCreditos()
    {
        SaveSystem.Datos.monedas += 100;
        SaveSystem.GuardarDatos();
        Debug.Log("Se sumaron 100 créditos.");
        ActualizarCreditosUI();
    }


    [ContextMenu("Borrar Cartas Compradas")]
    public void BorrarCartasCompradas()
    {
        SaveSystem.Datos.cartasCompradas.Clear();
        SaveSystem.GuardarDatos();
        Debug.Log("Cartas compradas borradas.");
    }

    [ContextMenu("Borrar Mazo Personalizado (usar default)")]
    public void BorrarMazoPersonalizado()
    {
        SaveSystem.Datos.mazoPersonalizado.Clear();
        SaveSystem.GuardarDatos();
        Debug.Log("Mazo personalizado borrado. Se usará el mazo default la próxima vez.");
    }

    [ContextMenu("Borrar Todo")]
    public void DeleteSave()
    {
        SaveManager.BorrarSave();
        PlayerPrefs.DeleteAll();
        Debug.Log("Archivo de guardado eliminado.");
    }

    public void MostrarAdvertencia(string mensaje, Transform puntoReferencia, Vector2? offset = null, float? tamañoTexto = null, Color? colorTexto = null)
    {
        Vector2 finalOffset = offset ?? Vector2.zero;

        GameObject instancia = Instantiate(textoAdvertenciaPrefab, puntoReferencia);
        RectTransform rect = instancia.GetComponent<RectTransform>();
        rect.localPosition = finalOffset;

        TextMeshProUGUI tmp = instancia.GetComponentInChildren<TextMeshProUGUI>();
        tmp.text = mensaje;

        if (tamañoTexto.HasValue)
            tmp.fontSize = tamañoTexto.Value;

        if (colorTexto.HasValue)
            tmp.color = colorTexto.Value;

        float duracion = 1.2f;

        rect.DOLocalMoveY(rect.localPosition.y + 50f, duracion).SetEase(Ease.OutCubic);

        DOTween.ToAlpha(() => tmp.color, x => tmp.color = x, 0f, duracion)
            .SetEase(Ease.OutCubic)
            .OnComplete(() => Destroy(instancia));
    }

    public void ShowLessCredits(string points)
    {
        MostrarAdvertencia(points, coinsPlace, offset, 62, Color.red);
    }

    private void UpdateStats()
    {
        string colorHex = ColorUtility.ToHtmlStringRGB(colorTextoEstadisticas);

        jugadasText.text = $"{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.PartidasJugadas)}";
        victoriasText.text = $"{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.PartidasGanadas)}";
        derrotasText.text = $"{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.PartidasPerdidas)}";
        vecesQueTeFuisteText.text = $"<color=#{colorHex}>Veces que te fuiste: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.VecesQueTeFuiste)}";
        rachaActualText.text = $"<color=#{colorHex}>Racha actual: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RachaVictoriasActual)}";
        rachaMaximaText.text = $"<color=#{colorHex}>Racha maxima: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RachaVictoriasMaxima)}";

        trucosCantadosText.text = $"<color=#{colorHex}>- Trucos cantados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.TrucosCantados)}";
        trucosAceptadosText.text = $"<color=#{colorHex}>- Trucos aceptados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.TrucosAceptados)}";
        retrucosCantadosText.text = $"<color=#{colorHex}>- Retrucos cantados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RetrucosCantados)}";
        retrucosAceptadosText.text = $"<color=#{colorHex}>- Retrucos aceptados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RetrucosAceptados)}";
        valeCuatroCantadosText.text = $"<color=#{colorHex}>- Vale Cuatro cantados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.ValeCuatroCantados)}";
        valeCuatroAceptadosText.text = $"<color=#{colorHex}>- Vale Cuatro aceptados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.ValeCuatroAceptados)}";

        envidosCantadosText.text = $"<color=#{colorHex}>- Envidos cantados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.EnvidosCantados)}";
        envidosAceptadosText.text = $"<color=#{colorHex}>- Envidos aceptados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.EnvidosAceptados)}";
        realEnvidosCantadosText.text = $"<color=#{colorHex}>- Real Envidos cantados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RealEnvidosCantados)}";
        realEnvidosAceptadosText.text = $"<color=#{colorHex}>- Real Envidos aceptados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RealEnvidosAceptados)}";
        faltaEnvidosCantadosText.text = $"<color=#{colorHex}>- Falta Envidos cantados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.FaltaEnvidosCantados)}";
        faltaEnvidosAceptadosText.text = $"<color=#{colorHex}>- Falta Envidos aceptados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.FaltaEnvidosAceptados)}";

        trucosGanadosText.text = $"<color=#{colorHex}>- Trucos ganados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.TrucosGanados)}";
        RetrucosGanadosText.text = $"<color=#{colorHex}>- Retrucos ganados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RetrucosGanados)}";
        ValeCuatroGanadosText.text = $"<color=#{colorHex}>- Vale Cuatro ganados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.ValeCuatroGanados)}";

        trucosPerdidosText.text = $"<color=#{colorHex}>- Trucos perdidos: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.TrucosPerdidos)}";
        RetrucosPerdidosText.text = $"<color=#{colorHex}>- Retrucos perdidos: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RetrucosPerdidos)}";
        ValeCuatroPerdidosText.text = $"<color=#{colorHex}>- Vale Cuatro perdidos: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.ValeCuatroPerdidos)}";

        EnvidosGanadosText.text = $"<color=#{colorHex}>- Envidos ganados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.EnvidosGanados)}";
        RealEnvidosGanadosText.text = $"<color=#{colorHex}>- Real Envidos ganados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RealEnvidosGanados)}";
        FaltaEnvidosGanadosText.text = $"<color=#{colorHex}>- Falta Envidos ganados: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.FaltaEnvidosGanados)}";

        EnvidosPerdidosText.text = $"<color=#{colorHex}>- Envidos perdidos: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.EnvidosPerdidos)}";
        RealEnvidosPerdidosText.text = $"<color=#{colorHex}>- Real Envidos perdidos: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.RealEnvidosPerdidos)}";
        FaltaEnvidosPerdidosText.text = $"<color=#{colorHex}>- Falta Envidos perdidos: </color>{Utils.Estadisticas.Obtener(Utils.Estadisticas.Keys.FaltaEnvidosPerdidos)}";
    }

}
