using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public enum TipoMensaje
{
    Jugador,
    IA,
    Sistema
}

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance;

    [Header("Referencias")]
    [SerializeField] private Transform contenedorMensajes;
    [SerializeField] private GameObject prefabMensaje;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Colores")]
    [SerializeField] private Color colorJugador;
    [SerializeField] private Color colorOponente;
    [SerializeField] private Color colorSistema;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void AgregarMensaje(string texto, TipoMensaje tipo)
    {
        GameObject mensajeGO = Instantiate(prefabMensaje, contenedorMensajes);
        TextMeshProUGUI tmp = mensajeGO.GetComponentInChildren<TextMeshProUGUI>();

        string hexJugador = ColorUtility.ToHtmlStringRGBA(colorJugador);
        string hexIA = ColorUtility.ToHtmlStringRGBA(colorOponente);
        string hexSistema = ColorUtility.ToHtmlStringRGBA(colorSistema);

        string prefijo = tipo switch
        {
            TipoMensaje.Jugador => $"<color=#{hexJugador}>{GameManager.Instance.NombreJugador(true)}:</color> ",
            TipoMensaje.IA => $"<color=#{hexIA}>{GameManager.Instance.NombreJugador(false)}:</color> ",
            TipoMensaje.Sistema => $"<color=#{hexSistema}>Sistema:</color> ",
            _ => ""
        };

        tmp.text = $"{prefijo}{texto}";

        StartCoroutine(ScrollAbajoProximoFrame());
    }

    private System.Collections.IEnumerator ScrollAbajoProximoFrame()
    {
        yield return null;
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
