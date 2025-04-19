using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button truco;
    public Button meVoy;
    public Button envido;

    [Header("Texts")]
    public TMP_Text playerPointsText;
    public TMP_Text opponentPointsText;

    [Header("Respuesta Truco")]
    public Button botonQuiero;
    public Button botonNoQuiero;
    public float distanciaAnimacionRespuesta = 100f;
    public float tiempoAnimacionRespuesta = 0.3f;

    private Vector3 quieroOriginalPos;
    private Vector3 noQuieroOriginalPos;
    private bool trucoFinalState = false;

    private void Start()
    {
        quieroOriginalPos = botonQuiero.transform.localPosition;
        noQuieroOriginalPos = botonNoQuiero.transform.localPosition;
    }

    public void SetBotonesInteractables(bool estado)
    {
        truco.interactable = estado;
        meVoy.interactable = estado;
        envido.interactable = estado;

        if (trucoFinalState && GameManager.Instance.estadoRonda != EstadoRonda.EsperandoRespuesta)
            truco.interactable = false;
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
            trucoFinalState = true;
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
}
