using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button truco;
    [SerializeField] private Button meVoy;
    [SerializeField] private Button envido;

    [Header("Texts")]
    [SerializeField] private TMP_Text playerPointsText;
    [SerializeField] private TMP_Text opponentPointsText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text creditos;

    [Header("Obejcts")]
    [SerializeField] private Image resultBG;
    [SerializeField] private List<GameObject> resultObjects = new List<GameObject>();

    [Header("Respuesta Truco")]
    [SerializeField] private Button botonQuiero;
    [SerializeField] private Button botonNoQuiero;
    [SerializeField] private float distanciaAnimacionRespuesta = 100f;
    [SerializeField] private float tiempoAnimacionRespuesta = 0.3f;

    public enum TrucoMensajeTipo
    {
        Truco,
        Retruco,
        ValeCuatro,
        Quiero,
        NoQuiero,
        MeVoy
    }

    [SerializeField] private GameObject trucoMessagePlayer;
    [SerializeField] private GameObject trucoMessageOponente;

    private Vector3 quieroOriginalPos;
    private Vector3 noQuieroOriginalPos;
    private bool trucoFinalState = false;

    private void Start()
    {
        quieroOriginalPos = botonQuiero.transform.localPosition;
        noQuieroOriginalPos = botonNoQuiero.transform.localPosition;
    }

    public void ActualizarBotonesSegunEstado()
    {
        meVoy.interactable = false;
        envido.interactable = false;

        bool puedeCantarTruco = false;

        //  Caso 1: turno normal del jugador
        if (GameManager.Instance.estadoRonda == EstadoRonda.Jugando)
        {
            puedeCantarTruco =
                GameManager.Instance.TrucoState < 3 &&
                GameManager.Instance.turnoActual == TurnoActual.Jugador &&
                GameManager.Instance.CantidadCartasJugadorJugadas < 3;
        }

        //  Caso 2: estoy respondiendo un canto (Truco o Retruco)
        else if (GameManager.Instance.estadoRonda == EstadoRonda.EsperandoRespuesta &&
                 GameManager.Instance.UltimoCantoFueDelJugador == false &&
                 GameManager.Instance.TrucoState < 3)
        {
            puedeCantarTruco = true; // puedo subir el canto
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
        trucoFinalState = false;
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

    public void SetPointsInScreen(int playerPoints, int oponentPoints)
    {
        playerPointsText.text = playerPoints.ToString();
        opponentPointsText.text = oponentPoints.ToString();
    }

    public void MostrarTrucoMensaje(bool esJugador, TrucoMensajeTipo tipo)
    {
        GameObject target = esJugador ? trucoMessagePlayer : trucoMessageOponente;
        TMP_Text texto = target.GetComponentInChildren<TMP_Text>();

        texto.text = tipo switch
        {
            TrucoMensajeTipo.Truco => "TRUCO",
            TrucoMensajeTipo.Retruco => "RETRUCO",
            TrucoMensajeTipo.ValeCuatro => "QUIERO VALE CUATRO",
            TrucoMensajeTipo.Quiero => "QUIERO",
            TrucoMensajeTipo.NoQuiero => "NO QUIERO",
            TrucoMensajeTipo.MeVoy => "ME VOY",
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
}
