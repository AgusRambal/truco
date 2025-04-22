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
    public UIManager uiManager;

    [Header("Parameters")]
    [SerializeField] private float minResponseTime = 0.5f;
    [SerializeField] private float maxResponseTime = 3f;
    [SerializeField] private float minTrucoResponseTime = 0.25f;
    [SerializeField] private float maxTrucoResponseTime = 1f;
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
        float delay = Random.Range(minResponseTime, maxResponseTime);
        yield return new WaitForSeconds(delay);

        bool cantoEnvido = false;

        if (!GameManager.Instance.EnvidoCantado && GameManager.Instance.CantidadCartasOponenteJugadas == 0 && GameManager.Instance.TrucoState == 0)
        {
            float chanceBase = estilo switch
            {
                EstiloIA.Canchero => 0.5f,
                EstiloIA.Conservador => 0.2f,
                EstiloIA.Caotico => 0.8f,
                _ => 0.3f
            };

            float r = Random.value;
            if (r < chanceBase / 2f)
            {
                GameManager.Instance.CantarEnvido(GameManager.TipoEnvido.RealEnvido, false);
                cantoEnvido = true;
            }
            else if (r < chanceBase)
            {
                GameManager.Instance.CantarEnvido(GameManager.TipoEnvido.Envido, false);
                cantoEnvido = true;
            }
        }

        // Si cantó Envido, no juega la carta todavía
        if (cantoEnvido || GameManager.Instance.estadoRonda != EstadoRonda.Jugando)
        {
            yield break;
        }


        if (GameManager.Instance.estadoRonda != EstadoRonda.Jugando)
        {
            Debug.Log("IA: se canceló la jugada porque ya no estamos jugando.");
            yield break;
        }

        var disponibles = GameManager.Instance.AllCards
        .Where(c => c != null && c.isOpponent && !c.hasBeenPlayed && c.gameObject != null)
        .ToList();

        if (disponibles.Count == 0)
        {
            Debug.Log("Oponente no tiene más cartas.");
            yield break;
        }

        // Lógica de canto Truco (con restricciones)
        bool puedeCantar = GameManager.Instance.estadoRonda == EstadoRonda.Jugando;
        int trucoState = GameManager.Instance.TrucoState;
        bool tieneCartasMalas = disponibles.All(c => c.GetComponent<Carta>().jerarquiaTruco < 7);
        bool tieneCartasFuertes = disponibles.Any(c => c.GetComponent<Carta>().jerarquiaTruco >= 12);
        float chance = Random.value;

        if (puedeCantar && GameManager.Instance.SeJugoCartaDesdeUltimoCanto)
        {
            if (trucoState == 0 && ((tieneCartasFuertes && chance < 0.25f) || (tieneCartasMalas && chance < 0.15f)))
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Truco);
                GameManager.Instance.TrucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
                GameManager.Instance.UltimoCantoFueDelJugador = false;
                yield break;
            }
            else if (trucoState == 1 && chance < 0.3f)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Retruco);
                GameManager.Instance.TrucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
                GameManager.Instance.UltimoCantoFueDelJugador = false;
                yield break;
            }
            else if (trucoState == 2 && chance < 0.35f)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.ValeCuatro);
                GameManager.Instance.TrucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
                GameManager.Instance.UltimoCantoFueDelJugador = false;
                yield break;
            }
        }

        //  Elegir carta con intención
        CardSelector elegida = null;

        if (GameManager.Instance.UltimaManoFueEmpate)
        {
            elegida = disponibles.OrderByDescending(c => c.GetComponent<Carta>().jerarquiaTruco).First();
        }
        else
        {
            var cartasOrdenadasIA = disponibles.OrderByDescending(c => c.GetComponent<Carta>().jerarquiaTruco).ToList();
            EstiloJugada estilo = DecidirEstiloJugada(disponibles);

            switch (estilo)
            {
                case EstiloJugada.Fuerte:
                    elegida = cartasOrdenadasIA[0];
                    break;
                case EstiloJugada.Débil:
                    elegida = cartasOrdenadasIA[^1];
                    break;
                case EstiloJugada.Amague:
                    elegida = cartasOrdenadasIA[Mathf.Clamp(Random.Range(1, cartasOrdenadasIA.Count - 1), 0, cartasOrdenadasIA.Count - 1)];
                    break;
            }
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

        GameManager.Instance.CartaJugada(elegida);
    }

    private EstiloJugada DecidirEstiloJugada(List<CardSelector> disponibles)
    {
        int jugadas = GameManager.Instance.AllCards.Count(c => c.isOpponent && c.hasBeenPlayed);

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
        float delay = Random.Range(minTrucoResponseTime, maxTrucoResponseTime);
        yield return new WaitForSeconds(delay);

        int estadoActual = GameManager.Instance.TrucoState;
        bool quiereSubir = false;

        if (estadoActual < 2)
        {
            quiereSubir = Random.value < 0.4f;
        }

        if (quiereSubir)
        {
            if (GameManager.Instance.TrucoState == 1)
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Retruco);
            else if (GameManager.Instance.TrucoState == 2)
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.ValeCuatro);

            Debug.Log("Oponente: ¡RETRUCO o VALE CUATRO!");
            GameManager.Instance.TrucoState++;
            GameManager.Instance.puntosEnJuego += 1;
            GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
            GameManager.Instance.ChangeTruco();
            GameManager.Instance.uiManager.MostrarOpcionesTruco();
            GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
            GameManager.Instance.UltimoCantoFueDelJugador = false;
        }

        else
        {
            bool acepta = Random.value > 0.4f;

            if (acepta)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Quiero);
                Debug.Log("Oponente: ¡Quiero!");
                GameManager.Instance.puntosEnJuego += 1;
                GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
                GameManager.Instance.ChangeTruco();

                if (GameManager.Instance.TrucoState < 3)
                    GameManager.Instance.PuedeResponderTruco = false;
            }
            else
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);
                Debug.Log("Oponente: ¡No quiero!");
                GameManager.Instance.SumarPuntos(true);
            }
        }
    }

    public IEnumerator ResponderEnvidoCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        bool quiere = Random.value > 0.5f;

        int envidoJugador = GameManager.Instance.CalcularPuntosEnvido(true);
        int envidoOponente = GameManager.Instance.CalcularPuntosEnvido(false);

        if (quiere)
        {
            bool ganaOponente = envidoOponente > envidoJugador ||
                    (envidoOponente == envidoJugador && !GameManager.Instance.EnvidoFueDelJugador);

            GameManager.Instance.ganoJugador = !ganaOponente;

            GameManager.Instance.ShowEnvidoResults(envidoJugador, envidoOponente);

            switch (GameManager.Instance.TipoDeEnvidoActual)
            {
                case GameManager.TipoEnvido.Envido:
                    if (ganaOponente)
                    {
                        GameManager.Instance.puntosOponente += 2;
                        Debug.Log("Resultado Envido: Gana el oponente (+2 puntos)");
                    }
                    else
                    {
                        GameManager.Instance.puntosJugador += 2;
                        Debug.Log("Resultado Envido: Gana el jugador (+2 puntos)");
                    }
                    break;

                case GameManager.TipoEnvido.RealEnvido:
                    if (ganaOponente)
                    {
                        GameManager.Instance.puntosOponente += 3;
                        Debug.Log("Resultado Real Envido: Gana el oponente (+3 puntos)");
                    }
                    else
                    {
                        GameManager.Instance.puntosJugador += 3;
                        Debug.Log("Resultado Real Envido: Gana el jugador (+3 puntos)");
                    }
                    break;
            }

            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Quiero);
        }
        else
        {
            // Si no acepta, 1 punto para quien cantó
            if (GameManager.Instance.EnvidoFueDelJugador)
            {
                GameManager.Instance.puntosJugador += 1;
                Debug.Log("IA no quiso: +1 punto para el jugador");
            }
            else
            {
                GameManager.Instance.puntosOponente += 1;
                Debug.Log("IA no quiso: +1 punto para el oponente");
            }

            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);
            uiManager.SetPointsInScreen(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
        }

        GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
        GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
        GameManager.Instance.EnvidoRespondido = true;

        if (GameManager.Instance.turnoActual == TurnoActual.Oponente &&
            GameManager.Instance.CantidadCartasOponenteJugadas < GameManager.Instance.CantidadCartasJugadorJugadas)
        {
            JugarCarta();
        }
    }

    public IEnumerator ResponderEnvidoExtendido()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        float chance = Random.value;

        // Si ya estamos en Envido, chance de subir
        if (GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.Envido) && chance < 0.2f)
        {
            GameManager.Instance.CantarEnvido(GameManager.TipoEnvido.RealEnvido, false);
            yield break;
        }

        // Si no sube más → simplemente responde
        StartCoroutine(ResponderEnvidoCoroutine());
    }
}
