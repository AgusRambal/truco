using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public enum EstiloIA
{
    Canchero,
    Conservador,
    Caotico
}

public class IAOponente : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float responseTime = 0.5f;
    [SerializeField] private float trucoResponseTime = 1f;
    [SerializeField] private EstiloIA estilo = EstiloIA.Canchero;

    private enum EstiloJugada
    {
        Fuerte,
        Débil,
        Amague
    }

    public void JugarCarta()
    {
        StartCoroutine(JugarCartaCoroutine());
    }

    private IEnumerator JugarCartaCoroutine()
    {
        float delay = Random.Range(1f, 4f);
        yield return new WaitForSeconds(delay);

        var disponibles = new List<CardSelector>();
        foreach (var carta in GameManager.Instance.allCards)
        {
            if (carta.isOpponent && !carta.hasBeenPlayed)
                disponibles.Add(carta);
        }

        if (disponibles.Count == 0)
        {
            Debug.Log("Oponente no tiene más cartas.");
            yield break;
        }

        // 🔥 Lógica de canto Truco (con bluff)
        bool puedeCantar = GameManager.Instance.estadoRonda == EstadoRonda.Jugando;
        int trucoState = GameManager.Instance.trucoState;
        bool tieneCartasMalas = disponibles.All(c => c.GetComponent<Carta>().jerarquiaTruco < 7);
        bool tieneCartasFuertes = disponibles.Any(c => c.GetComponent<Carta>().jerarquiaTruco >= 12);

        float chance = Random.value;

        if (puedeCantar && GameManager.Instance.seJugoCartaDesdeUltimoCanto)
        {
            if (trucoState == 0 && ((tieneCartasFuertes && chance < 0.25f) || (tieneCartasMalas && chance < 0.15f)))
            {
                GameManager.Instance.trucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                yield break;
            }
            else if (trucoState == 1 && chance < 0.3f)
            {
                GameManager.Instance.trucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                yield break;
            }
            else if (trucoState == 2 && chance < 0.35f)
            {
                GameManager.Instance.trucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                yield break;
            }
        }

        // 🧠 Elegir carta con intención
        var cartasOrdenadas = disponibles.OrderByDescending(c => c.GetComponent<Carta>().jerarquiaTruco).ToList();
        EstiloJugada estiloJugada = DecidirEstiloJugada(disponibles);

        CardSelector elegida = null;

        switch (estiloJugada)
        {
            case EstiloJugada.Fuerte:
                elegida = cartasOrdenadas[0];
                break;
            case EstiloJugada.Débil:
                elegida = cartasOrdenadas[^1];
                break;
            case EstiloJugada.Amague:
                elegida = cartasOrdenadas[Mathf.Clamp(Random.Range(1, cartasOrdenadas.Count - 1), 0, cartasOrdenadas.Count - 1)];
                break;
        }

        elegida.hasBeenPlayed = true;

        Vector3 midPos = elegida.transform.position + Vector3.up * 0.5f;
        Vector3 finalPos = GameManager.Instance.target.position;
        finalPos.z += GameManager.Instance.GetZOffset();

        Vector3 finalRot = GameManager.Instance.target.rotation.eulerAngles;
        finalRot.z += Random.Range(-10f, 10f);

        elegida.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 0f, 0f);
        elegida.transform.GetChild(0).localPosition = new Vector3(0f, 0f, 0.1f);

        Sequence s = DOTween.Sequence();
        s.Append(elegida.transform.DOMove(midPos, 0.15f).SetEase(Ease.OutSine));
        s.Append(elegida.transform.DOMove(finalPos, 0.2f).SetEase(Ease.InOutCubic));
        s.Join(elegida.transform.DORotate(finalRot, 0.2f).SetEase(Ease.InOutCubic));

        yield return s.WaitForCompletion();

        bool ultimaCarta = true;
        foreach (var carta in GameManager.Instance.allCards)
        {
            if (carta.isOpponent && !carta.hasBeenPlayed)
            {
                ultimaCarta = false;
                break;
            }
        }

        GameManager.Instance.CartaJugada(elegida);
    }

    private EstiloJugada DecidirEstiloJugada(List<CardSelector> disponibles)
    {
        int jugadas = GameManager.Instance.allCards.Count(c => c.isOpponent && c.hasBeenPlayed);

        switch (estilo)
        {
            case EstiloIA.Canchero:
                if (jugadas == 0) return Random.value < 0.7f ? EstiloJugada.Amague : EstiloJugada.Fuerte;
                if (jugadas == 1) return Random.value < 0.4f ? EstiloJugada.Débil : EstiloJugada.Fuerte;
                return EstiloJugada.Fuerte;

            case EstiloIA.Conservador:
                if (jugadas == 0) return EstiloJugada.Fuerte;
                if (jugadas == 1) return Random.value < 0.3f ? EstiloJugada.Fuerte : EstiloJugada.Débil;
                return EstiloJugada.Fuerte;

            case EstiloIA.Caotico:
                return (EstiloJugada)Random.Range(0, 3);
        }

        return EstiloJugada.Fuerte;
    }

    public void ResponderTruco()
    {
        StartCoroutine(ResponderTrucoCoroutine());
    }

    private IEnumerator ResponderTrucoCoroutine()
    {
        yield return new WaitForSeconds(trucoResponseTime);

        int estadoActual = GameManager.Instance.trucoState;
        bool quiereSubir = false;

        if (estadoActual < 2)
        {
            quiereSubir = Random.value < 0.4f;
        }

        if (quiereSubir)
        {
            Debug.Log("Oponente: ¡RETRUCO o VALE CUATRO!");
            GameManager.Instance.trucoState++;
            GameManager.Instance.puntosEnJuego += 1;
            GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
            GameManager.Instance.ChangeTruco();
            GameManager.Instance.uiManager.MostrarOpcionesTruco();
            GameManager.Instance.seJugoCartaDesdeUltimoCanto = false;
            GameManager.Instance.ultimoCantoFueDelJugador = false;
        }
        else
        {
            bool acepta = Random.value > 0.4f;

            if (acepta)
            {
                Debug.Log("Oponente: ¡Quiero!");
                GameManager.Instance.puntosEnJuego += 1;
                GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
                GameManager.Instance.ChangeTruco();
            }
            else
            {
                Debug.Log("Oponente: ¡No quiero!");
                GameManager.Instance.SumarPuntosJugador();
                GameManager.Instance.FinalizarRonda();
            }
        }
    }
}
