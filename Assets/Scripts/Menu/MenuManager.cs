using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Utils;

public class MenuManager : MonoBehaviour
{
    [Header("Cartas en pantalla")]
    [SerializeField] private List<Transform> cartasMenu;

    [Header("Mazo")]
    [SerializeField] private List<CartaSO> todasLasCartas;   // TODAS (originales + personalizadas)
    [SerializeField] private List<CartaSO> mazoDefault;      // 40 originales
    [SerializeField] private List<CartaSO> mazoPersonalizado;
    public List<CartaSO> cartasCompradas = new(); // visible en el inspector si querés debug

    [Header("Referencias")]
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private TMP_Text creditosTexto;

    [Header("Botones principales")]
    [SerializeField] private List<Transform> botonesMenuPrincipal;

    [Header("Botones del menú de juego")]
    [SerializeField] private List<Transform> botonesPuntos;
    [SerializeField] private TMP_Dropdown dropdownEstiloIA;

    [Header("Configuración de animación")]
    [SerializeField] private float delayEntreBotones = 0.05f;
    [SerializeField] private float duracionAnimacion = 0.25f;

    [Header("Zona de personalización")]
    [SerializeField] private Transform contenedorCartasCompradas; // zona donde se instancian
    [SerializeField] private GameObject prefabCartaVisual;        // un prefab simple con imagen, nombre, etc.

    private const string keyCartaSeleccionada = "CartaSeleccionada";
    private bool popUpState = false;

    private void Awake()
    {
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

        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        creditosTexto.text = $"{creditos}";
        SetIas();
        SpawnCartasCompradas();
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

    private void SetIas()
    {
        dropdownEstiloIA.ClearOptions();
        List<string> nombresEstilos = System.Enum.GetNames(typeof(EstiloIA)).ToList();
        dropdownEstiloIA.AddOptions(nombresEstilos);
    }

    public void Jugar(int puntos)
    {
        ParametrosDePartida.puntosParaGanar = puntos;
        ParametrosDePartida.estiloSeleccionado = (EstiloIA)dropdownEstiloIA.value;
        ParametrosDePartida.gananciaCalculada = CalcularGanancia((EstiloIA)dropdownEstiloIA.value, puntos);
        ParametrosDePartida.cartasSeleccionadas = new List<CartaSO>(mazoPersonalizado);

        DOTween.KillAll();
        SceneManager.LoadScene("Player vs IA");
    }

    //PARA REEMPLAZAR UNA CARTA
    public void ReemplazarCartaElegida(CartaSO carta)
    {
        CartaSaveManager.ReemplazarCarta(carta, mazoPersonalizado);
        CartaSaveManager.GuardarCartas(mazoPersonalizado);

        // Guardar qué carta fue seleccionada como personalizada
        PlayerPrefs.SetString(keyCartaSeleccionada, carta.id);
        PlayerPrefs.Save();
    }

    private int CalcularGanancia(EstiloIA estilo, int puntosFinales)
    {
        int baseGanancia = estilo switch
        {
            EstiloIA.Conservador => Random.Range(50, 101),
            EstiloIA.Canchero => Random.Range(120, 181),
            EstiloIA.Calculador => Random.Range(130, 200),
            EstiloIA.Agresivo => Random.Range(200, 280),
            EstiloIA.Mentiroso => Random.Range(220, 300),
            EstiloIA.Caotico => Random.Range(250, 350),
            _ => 100
        };

        if (puntosFinales == 30)
            baseGanancia = Mathf.RoundToInt(baseGanancia * 1.5f);

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
        popUpState = !popUpState;

        if (popUpState)
        {
            rectTransform.gameObject.SetActive(true);
            rectTransform.localScale = Vector3.zero;

            rectTransform.DOScale(Vector3.one, animationDuration)
                .SetEase(Ease.OutBack);
        }

        else
        {
            rectTransform.DOScale(Vector3.zero, animationDuration - 0.2f)
           .SetEase(Ease.InBack)
           .OnComplete(() =>
           {
               rectTransform.gameObject.SetActive(false);
           });
        }
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
        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        creditosTexto.text = $"{creditos}";
    }

    [ContextMenu("Resetear Créditos")]
    public void ResetearCreditosDesdeEditor()
    {
        PlayerPrefs.SetInt("Creditos", 0);
        PlayerPrefs.Save();
        Debug.Log("Créditos reseteados desde el editor.");
    }

    [ContextMenu("Sumar 100 Créditos")]
    public void SumarCreditos()
    {
        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        creditos += 100;
        PlayerPrefs.SetInt("Creditos", creditos);
        PlayerPrefs.Save();
        Debug.Log("Se sumaron 100 créditos.");
        ActualizarCreditosUI();
    }

    [ContextMenu("Borrar Cartas Compradas")]
    public void BorrarCartasCompradas()
    {
        PlayerPrefs.DeleteKey("CartasCompradas");
        PlayerPrefs.Save();
        Debug.Log("Cartas compradas borradas.");
    }

    [ContextMenu("Borrar Mazo Personalizado (usar default)")]
    public void BorrarMazoPersonalizado()
    {
        PlayerPrefs.DeleteKey("CartasPersonalizadas");
        PlayerPrefs.Save();
        Debug.Log("Mazo personalizado borrado. Se usará el mazo default la próxima vez.");
    }
}
