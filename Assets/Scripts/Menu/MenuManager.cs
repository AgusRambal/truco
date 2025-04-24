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

    private bool popUpState = false;

    private void Awake()
    {
        mazoPersonalizado = CartaSaveManager.CargarCartas(todasLasCartas);

        if (mazoPersonalizado.Count != 40)
        {
            Debug.Log("No hay mazo guardado o está incompleto, usando mazo default");
            mazoPersonalizado = new List<CartaSO>(mazoDefault);
        }
    }

    private void Start()
    {
        AudioManager.Instance.PlayRandomMenuTrack();

        int creditos = PlayerPrefs.GetInt("Creditos", 0);
        creditosTexto.text = $"{creditos}";
        SetIas();
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
    public void ReemplazarCartaElegida(CartaSO nuevaCarta)
    {
        CartaSaveManager.ReemplazarCarta(nuevaCarta, mazoPersonalizado);
        CartaSaveManager.GuardarCartas(mazoPersonalizado);
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

    [ContextMenu("Resetear Créditos")]
    public void ResetearCreditosDesdeEditor()
    {
        PlayerPrefs.SetInt("Creditos", 0);
        PlayerPrefs.Save();
        Debug.Log("Créditos reseteados desde el editor.");
    }
}
