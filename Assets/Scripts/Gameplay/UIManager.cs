using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using UnityEditor.Experimental.GraphView;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button truco;
    [SerializeField] private Button meVoy;
    [SerializeField] private Button envido;
    [SerializeField] private Button realEnvido;

    [Header("Texts")]
    [SerializeField] private TMP_Text playerPointsText;
    [SerializeField] private TMP_Text opponentPointsText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text creditos;

    [Header("Obejcts")]
    [SerializeField] private Image resultBG;
    [SerializeField] private Image resultBG2;
    [SerializeField] private List<GameObject> resultObjects = new List<GameObject>();
    [SerializeField] private RectTransform popupOpcionesParent;
    [SerializeField] private RectTransform popupOpciones;
    [SerializeField] private Button opciones;
    [SerializeField] private float animationDuration = 0.4f;

    [Header("Respuesta Truco")]
    [SerializeField] private Button botonQuiero;
    [SerializeField] private Button botonNoQuiero;
    [SerializeField] private Button botonQuieroEnvido;
    [SerializeField] private Button botonNoQuieroEnvido;
    [SerializeField] private float distanciaAnimacionRespuesta = 100f;
    [SerializeField] private float tiempoAnimacionRespuesta = 0.3f;

    [Header("Envido")]
    [SerializeField] private GameObject envidoPoints;
    [SerializeField] private GameObject envidoPoints2;

    [Header("Audio")]
    [SerializeField] private AudioClip notification;

    public enum TrucoMensajeTipo
    {
        Truco,
        Retruco,
        ValeCuatro,
        Quiero,
        NoQuiero,
        MeVoy,
        Envido,
        RealEnvido,
        FaltaEnvido
    }

    [SerializeField] private GameObject trucoMessagePlayer;
    [SerializeField] private GameObject trucoMessageOponente;

    private Vector3 quieroOriginalPos;
    private Vector3 noQuieroOriginalPos;

    private void Start()
    {
        quieroOriginalPos = botonQuiero.transform.localPosition;
        noQuieroOriginalPos = botonNoQuiero.transform.localPosition;
    }

    public void ActualizarBotonesSegunEstado()
    {
        bool puedeCantarTruco = false;

        if (GameManager.Instance.estadoRonda == EstadoRonda.Jugando)
        {
            puedeCantarTruco =
                GameManager.Instance.TrucoState < 3 &&
                GameManager.Instance.turnoActual == TurnoActual.Jugador &&
                GameManager.Instance.CantidadCartasJugadorJugadas < 3 &&
                GameManager.Instance.PuedeResponderTruco;
        }

        else if (GameManager.Instance.estadoRonda == EstadoRonda.EsperandoRespuesta &&
         GameManager.Instance.UltimoCantoFueDelJugador == false &&
         GameManager.Instance.TrucoState < 3 &&
         GameManager.Instance.PuedeResponderTruco)
        {
            puedeCantarTruco = true;
        }

        truco.interactable = puedeCantarTruco;
    }

    public void ChangeTrucoState(int state)
    {
        if (state == 1)
        {
            truco.GetComponentInChildren<TMP_Text>().text = $"RETRUCO";
        }
        else if (state == 2)
        {
            truco.GetComponentInChildren<TMP_Text>().text = $"VALE CUATRO";
        }
    }

    public void ResetTruco()
    {
        truco.GetComponentInChildren<TMP_Text>().text = $"TRUCO";
    }

    public void MostrarOpcionesTruco()
    {
        botonQuiero.transform.localPosition = quieroOriginalPos;
        botonNoQuiero.transform.localPosition = noQuieroOriginalPos;

        botonQuiero.transform.DOLocalMoveX(quieroOriginalPos.x + distanciaAnimacionRespuesta, tiempoAnimacionRespuesta).SetDelay(.1f).SetEase(Ease.OutBack);
        botonNoQuiero.transform.DOLocalMoveX(noQuieroOriginalPos.x + distanciaAnimacionRespuesta, tiempoAnimacionRespuesta).SetEase(Ease.OutBack);
    }

    public void OcultarOpcionesTruco()
    {
        botonQuiero.transform.DOLocalMove(quieroOriginalPos, tiempoAnimacionRespuesta).SetEase(Ease.OutBack);
        botonNoQuiero.transform.DOLocalMove(noQuieroOriginalPos, tiempoAnimacionRespuesta).SetDelay(.1f).SetEase(Ease.OutBack);
    }

    public void MostrarOpcionesEnvido()
    {
        botonQuieroEnvido.transform.localPosition = quieroOriginalPos;
        botonNoQuieroEnvido.transform.localPosition = noQuieroOriginalPos;

        botonQuieroEnvido.transform.DOLocalMoveX(quieroOriginalPos.x + distanciaAnimacionRespuesta, tiempoAnimacionRespuesta).SetDelay(.1f).SetEase(Ease.OutBack);
        botonNoQuieroEnvido.transform.DOLocalMoveX(noQuieroOriginalPos.x + distanciaAnimacionRespuesta, tiempoAnimacionRespuesta).SetEase(Ease.OutBack);
    }

    public void OcultarOpcionesEnvido()
    {
        botonQuieroEnvido.transform.DOLocalMove(quieroOriginalPos, tiempoAnimacionRespuesta).SetEase(Ease.OutBack);
        botonNoQuieroEnvido.transform.DOLocalMove(noQuieroOriginalPos, tiempoAnimacionRespuesta).SetDelay(.1f).SetEase(Ease.OutBack);
    }

    public void SetPointsInScreen(int playerPoints, int oponentPoints)
    {
        playerPointsText.text = playerPoints.ToString();
        opponentPointsText.text = oponentPoints.ToString();
    }

    public void MostrarTrucoMensaje(bool esJugador, TrucoMensajeTipo tipo)
    {
        GameObject target = esJugador ? trucoMessagePlayer : trucoMessageOponente;
        TMP_Text texto = target.GetComponentInChildren<TMP_Text>();

        AudioManager.Instance.PlaySFX(notification);

        texto.text = tipo switch
        {
            TrucoMensajeTipo.Truco => "TRUCO",
            TrucoMensajeTipo.Retruco => "RETRUCO",
            TrucoMensajeTipo.ValeCuatro => "QUIERO VALE CUATRO",
            TrucoMensajeTipo.Quiero => "QUIERO",
            TrucoMensajeTipo.NoQuiero => "NO QUIERO",
            TrucoMensajeTipo.MeVoy => "ME VOY",
            TrucoMensajeTipo.Envido => "ENVIDO",
            TrucoMensajeTipo.RealEnvido => "REAL ENVIDO",
            TrucoMensajeTipo.FaltaEnvido => "FALTA ENVIDO",
            _ => ""
        };

        target.transform.localScale = Vector3.zero;
        target.SetActive(true);

        target.transform
            .DOScale(Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                target.transform
                    .DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .SetDelay(0.8f);
            });
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void ReiniciarPartida()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MostrarResultadoFinal(bool ganoJugador, int ganancia)
    {
        Color c = resultBG.color;
        c.a = 0f;
        resultBG.color = c;

        resultBG.DOFade(160f / 255f, 0.5f).SetEase(Ease.InOutCubic);

        foreach (var obj in resultObjects)
        {
            obj.transform.localScale = Vector3.zero;

            float delay = Random.Range(0f, 0.2f);

            obj.transform.DOScale(Vector3.one, 0.4f)
                .SetEase(Ease.OutBack)
                .SetDelay(delay);
        }

        if (ganoJugador)
        {
            resultText.text = $"GANASTE!!";
            creditos.text = $"OBTUVISTE {ganancia} CREDITOS";
        }

        else
        {
            resultText.text = $"PERDISTE..";
            creditos.text = $"NO OBTUVISTE CREDITOS";
        }
    }

    public void AbrirOpciones()
    {
        GameManager.Instance.isPaused = true;

        resultBG2.DOFade(160f / 255f, 0.3f).SetEase(Ease.InOutCubic);

        opciones.interactable = false;
        popupOpcionesParent.gameObject.SetActive(true);
        popupOpciones.localScale = Vector3.zero;

        popupOpciones.DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack);
    }

    public void CerrarOpciones()
    {
        GameManager.Instance.isPaused = false;

        resultBG2.DOFade(0f, 0.5f).SetEase(Ease.InOutCubic);

        opciones.interactable = true;
        popupOpciones.DOScale(Vector3.zero, animationDuration - 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                popupOpcionesParent.gameObject.SetActive(false);
            });
    }

    public void ShowEnvidoPoints(int valor1, int valor2)
    {
        var text1 = envidoPoints.GetComponentInChildren<TMP_Text>();
        var text2 = envidoPoints2.GetComponentInChildren<TMP_Text>();

        if (GameManager.Instance.ganoJugador)
        {
            text1.color = Color.green;
            text2.color = Color.red;
        }

        else
        {
            text1.color = Color.red;
            text2.color = Color.green;
        }

        text1.text = $"{valor1}";
        text2.text = $"{valor2}";

        // Animación 1
        envidoPoints.transform.localScale = Vector3.zero;
        envidoPoints.SetActive(true);

        envidoPoints.transform.DOScale(0.3f * Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                envidoPoints.transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .SetDelay(2f);
            });

        // Animación 2
        envidoPoints2.transform.localScale = Vector3.zero;
        envidoPoints2.SetActive(true);

        envidoPoints2.transform.DOScale(0.3f * Vector3.one, 0.3f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                envidoPoints2.transform.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
                    .SetDelay(2f)
                    .OnComplete(() =>
                    {
                        SetPointsInScreen(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
                    });
            });
    }

    public void ActualizarBotonesEnvido()
    {
        bool envidoYaCantado = GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.Envido);
        bool realEnvidoYaCantado = GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.RealEnvido);

        // Se permite subir una vez si todavía no respondieron y solo hay un canto
        bool puedeSubir = !GameManager.Instance.EnvidoRespondido && GameManager.Instance.EnvidoCantos.Count < 2;

        if (GameManager.Instance.EnvidoFueDelJugador && envidoYaCantado && !puedeSubir)
            envido.interactable = false;
        else
            envido.interactable = true;

        if (GameManager.Instance.EnvidoFueDelJugador && realEnvidoYaCantado && !puedeSubir)
            realEnvido.interactable = false;
        else
            realEnvido.interactable = true;
    }
}
