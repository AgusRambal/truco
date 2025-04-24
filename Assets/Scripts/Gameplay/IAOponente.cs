using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public enum EstiloIA
{
    Canchero,
    Conservador,
    Caotico,
    Agresivo,
    Calculador,
    Mentiroso
}

public class IAOponente : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    [Header("Parameters")]
    public EstiloIA estilo = EstiloIA.Canchero;
    [SerializeField] private float minResponseTime = 0.5f;
    [SerializeField] private float maxResponseTime = 3f;
    [SerializeField] private float minTrucoResponseTime = 0.25f;
    [SerializeField] private float maxTrucoResponseTime = 1f;

    [Header("Probabilidades de Envido")]
    [SerializeField] private bool usarEstiloParaChancesEnvido = true;
    [SerializeField, Range(0f, 1f)] private float chanceCantarEnvido = 0.5f;
    [SerializeField, Range(0f, 1f)] private float chanceDeQueSeaReal = 0.5f;
    [SerializeField, Range(0f, 1f)] private float chanceResponderConSubida = 0.5f;

    [Header("Probabilidades de Truco")]
    [SerializeField] private bool usarEstiloParaChancesTruco = true;
    [SerializeField, Range(0f, 1f)] private float chanceCantarTruco = 0.5f;
    [SerializeField, Range(0f, 1f)] private float chanceCantarRetruco = 0.5f;
    [SerializeField, Range(0f, 1f)] private float chanceCantarValeCuatro = 0.5f;
    [SerializeField, Range(0f, 1f)] private float chanceResponderTruco = 0.5f;

    [Header("Probabilidades de Irse")]
    [SerializeField] private bool usarEstiloParaChancesIrse = true;
    [SerializeField, Range(0f, 1f)] private float chanceDeIrse = 0.5f;

    private enum EstiloJugada
    {
        Fuerte,
        Débil,
        Amague
    }

    private void Awake()
    {
        estilo = Utils.ParametrosDePartida.estiloSeleccionado;
        SelectIAChance();     
    }

    private void SelectIAChance()
    {
        if (usarEstiloParaChancesEnvido)
            SetChancesEnvido(estilo);

        if (usarEstiloParaChancesTruco)
            SetChancesTruco(estilo);

        if (usarEstiloParaChancesIrse)
            SetChancesIrse(estilo);
    }

    private void SetChancesEnvido(EstiloIA estilo)
    {
        switch (estilo)
        {
            case EstiloIA.Canchero:
                (chanceCantarEnvido, chanceDeQueSeaReal, chanceResponderConSubida) = (0.6f, 0.4f, 0.8f);
                break;
            case EstiloIA.Conservador:
                (chanceCantarEnvido, chanceDeQueSeaReal, chanceResponderConSubida) = (0.3f, 0.2f, 0.4f);
                break;
            case EstiloIA.Caotico:
                (chanceCantarEnvido, chanceDeQueSeaReal, chanceResponderConSubida) = (0.9f, 0.6f, 1f);
                break;
            case EstiloIA.Agresivo:
                (chanceCantarEnvido, chanceDeQueSeaReal, chanceResponderConSubida) = (0.7f, 0.5f, 0.9f);
                break;
            case EstiloIA.Calculador:
                (chanceCantarEnvido, chanceDeQueSeaReal, chanceResponderConSubida) = (0.4f, 0.6f, 0.3f);
                break;
            case EstiloIA.Mentiroso:
                (chanceCantarEnvido, chanceDeQueSeaReal, chanceResponderConSubida) = (0.5f, 0.2f, 0.7f);
                break;
        }
    }

    private void SetChancesTruco(EstiloIA estilo)
    {
        switch (estilo)
        {
            case EstiloIA.Canchero:
                (chanceCantarTruco, chanceCantarRetruco, chanceCantarValeCuatro, chanceResponderTruco) = (0.5f, 0.5f, 0.3f, 0.9f);
                break;
            case EstiloIA.Conservador:
                (chanceCantarTruco, chanceCantarRetruco, chanceCantarValeCuatro, chanceResponderTruco) = (0.3f, 0.1f, 0.05f, 0.6f);
                break;
            case EstiloIA.Caotico:
                (chanceCantarTruco, chanceCantarRetruco, chanceCantarValeCuatro, chanceResponderTruco) = (0.9f, 0.7f, 0.7f, 1f);
                break;
            case EstiloIA.Agresivo:
                (chanceCantarTruco, chanceCantarRetruco, chanceCantarValeCuatro, chanceResponderTruco) = (0.8f, 0.6f, 0.4f, 1f);
                break;
            case EstiloIA.Calculador:
                (chanceCantarTruco, chanceCantarRetruco, chanceCantarValeCuatro, chanceResponderTruco) = (0.4f, 0.3f, 0.2f, 0.8f);
                break;
            case EstiloIA.Mentiroso:
                (chanceCantarTruco, chanceCantarRetruco, chanceCantarValeCuatro, chanceResponderTruco) = (0.6f, 0.5f, 0.1f, 0.7f);
                break;
        }
    }

    private void SetChancesIrse(EstiloIA estilo)
    {
        switch (estilo)
        {
            case EstiloIA.Canchero:
                chanceDeIrse = 0.1f;
                break;
            case EstiloIA.Conservador:
                chanceDeIrse = 0.4f;
                break;
            case EstiloIA.Caotico:
                chanceDeIrse = 0.05f;
                break;
            case EstiloIA.Agresivo:
                chanceDeIrse = 0.05f;
                break;
            case EstiloIA.Calculador:
                chanceDeIrse = 0.2f;
                break;
            case EstiloIA.Mentiroso:
                chanceDeIrse = 0.15f;
                break;
        }
    }

    public void JugarCarta()
    {
        StartCoroutine(JugarCartaCoroutine());
    }

    private IEnumerator JugarCartaCoroutine()
    {
        float delay = Random.Range(minResponseTime, maxResponseTime);
        yield return new WaitForSeconds(delay);

        if (EvaluarSiIrse())
        {
            Debug.Log("IA decide irse antes de jugar carta.");
            GameManager.Instance.MeVoy(false); // false = IA
            yield break;
        }

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
            if (r < chanceCantarEnvido)
            {
                GameManager.TipoEnvido tipoACantar = Random.value < chanceDeQueSeaReal
                    ? GameManager.TipoEnvido.RealEnvido
                    : GameManager.TipoEnvido.Envido;

                GameManager.Instance.CantarEnvido(tipoACantar, false);
            }
        }

        // Si cantó Envido, no juega la carta todavía
        if (GameManager.Instance.EnvidoCantado || GameManager.Instance.estadoRonda != EstadoRonda.Jugando)
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
            if (trucoState == 0)
            {
                float chanceFinal = chanceCantarTruco;

                if (tieneCartasFuertes)
                    chanceFinal *= 1.2f; // aumenta chance si tiene buena mano
                else if (tieneCartasMalas)
                    chanceFinal *= 0.8f; // reduce chance si tiene mala mano

                chanceFinal = Mathf.Clamp01(chanceFinal);  // asegura entre 0 y 1

                if (Random.value < chanceFinal)
                {
                    uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Truco);
                    GameManager.Instance.TrucoState++;
                    GameManager.Instance.puntosEnJuego++;
                    GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                    GameManager.Instance.ChangeTruco();
                    GameManager.Instance.uiManager.MostrarOpcionesTruco();
                    GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
                    GameManager.Instance.UltimoCantoFueDelJugador = false;
                    GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
                    yield break;
                }
            }
            else if (trucoState == 1 && chance < chanceCantarRetruco)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Retruco);
                GameManager.Instance.TrucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
                GameManager.Instance.UltimoCantoFueDelJugador = false;
                GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
                yield break;
            }
            else if (trucoState == 2 && chance < chanceCantarValeCuatro)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.ValeCuatro);
                GameManager.Instance.TrucoState++;
                GameManager.Instance.puntosEnJuego++;
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
                GameManager.Instance.ChangeTruco();
                GameManager.Instance.uiManager.MostrarOpcionesTruco();
                GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
                GameManager.Instance.UltimoCantoFueDelJugador = false;
                GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
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

        if (estadoActual == 0)
            quiereSubir = Random.value < chanceCantarRetruco;
        else if (estadoActual == 1)
            quiereSubir = Random.value < chanceCantarValeCuatro;

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
            GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
        }

        else
        {
            bool acepta = Random.value < chanceResponderTruco;

            if (acepta)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Quiero);
                Debug.Log("Oponente: ¡Quiero!");
                GameManager.Instance.puntosEnJuego = GameManager.Instance.TrucoState + 1;
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

        if (GameManager.Instance.TipoDeEnvidoActual == GameManager.TipoEnvido.FaltaEnvido)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            Debug.Log("IA: No quiero Falta Envido (canto directo)");
            GameManager.Instance.puntosJugador += 1;
            GameManager.Instance.uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);
            GameManager.Instance.uiManager.SetPointsInScreen(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
            GameManager.Instance.EnvidoRespondido = true;
            GameManager.Instance.EnvidoCantado = false;
            GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
            GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
            yield break;
        }

        if (quiere)
        {
            bool ganaOponente = envidoOponente > envidoJugador ||
                    (envidoOponente == envidoJugador && !GameManager.Instance.EnvidoFueDelJugador);

            GameManager.Instance.ganoJugador = !ganaOponente;

            GameManager.Instance.ShowEnvidoResults(envidoJugador, envidoOponente);

            int puntosAGanar = 0;

            var cantos = GameManager.Instance.EnvidoCantos;

            if (cantos.Contains(GameManager.TipoEnvido.FaltaEnvido))
            {
                puntosAGanar = GameManager.Instance.PointsToEnd -
                               Mathf.Max(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
            }
            else
            {
                foreach (var canto in cantos)
                {
                    switch (canto)
                    {
                        case GameManager.TipoEnvido.Envido:
                            puntosAGanar += 2;
                            break;
                        case GameManager.TipoEnvido.RealEnvido:
                            puntosAGanar += 3;
                            break;
                    }
                }
            }

            if (ganaOponente)
            {
                GameManager.Instance.puntosOponente += puntosAGanar;
                Debug.Log($"Resultado Envido: Gana la IA (+{puntosAGanar} puntos)");
            }
            else
            {
                GameManager.Instance.puntosJugador += puntosAGanar;
                Debug.Log($"Resultado Envido: Gana el jugador (+{puntosAGanar} puntos)");
            }

            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Quiero);
            uiManager.ActualizarBotonesSegunEstado();
        }

        else
        {
            int puntosPorNoQuerer = 1;

            var cantos = GameManager.Instance.EnvidoCantos;

            if (cantos.Count == 2)
            {
                if (cantos.Contains(GameManager.TipoEnvido.Envido) &&
                    cantos.Contains(GameManager.TipoEnvido.RealEnvido))
                {
                    puntosPorNoQuerer = 2;
                }
                else if (cantos[0] == GameManager.TipoEnvido.Envido &&
                         cantos[1] == GameManager.TipoEnvido.Envido)
                {
                    puntosPorNoQuerer = 2;
                }
            }
            else if (cantos.Count == 3)
            {
                if (cantos[0] == GameManager.TipoEnvido.Envido &&
                    cantos[1] == GameManager.TipoEnvido.Envido &&
                    cantos[2] == GameManager.TipoEnvido.RealEnvido)
                {
                    puntosPorNoQuerer = 3;
                }
            }

            if (GameManager.Instance.EnvidoFueDelJugador)
            {
                GameManager.Instance.puntosJugador += puntosPorNoQuerer;
                Debug.Log($"IA no quiso: +{puntosPorNoQuerer} punto(s) para el jugador");
            }
            else
            {
                GameManager.Instance.puntosOponente += puntosPorNoQuerer;
                Debug.Log($"IA no quiso: +{puntosPorNoQuerer} punto(s) para la IA");
            }

            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);
            uiManager.SetPointsInScreen(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
        }


        GameManager.Instance.EnvidoRespondido = true;
        GameManager.Instance.EnvidoCantado = false;
        GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
        GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();

        if (GameManager.Instance.turnoActual == TurnoActual.Oponente &&
            GameManager.Instance.estadoRonda == EstadoRonda.Jugando &&
            GameManager.Instance.CantidadCartasOponenteJugadas < 3)
        {
            JugarCarta();
        }
    }

    public IEnumerator ResponderEnvidoExtendido()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        if (GameManager.Instance.TipoDeEnvidoActual == GameManager.TipoEnvido.FaltaEnvido)
        {
            Debug.Log("IA: No quiero Falta Envido");
            GameManager.Instance.puntosJugador += 1;
            GameManager.Instance.uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);
            GameManager.Instance.uiManager.SetPointsInScreen(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
            GameManager.Instance.EnvidoRespondido = true;
            GameManager.Instance.EnvidoCantado = false;
            GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
            GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();

            if (GameManager.Instance.turnoActual == TurnoActual.Oponente &&
            GameManager.Instance.estadoRonda == EstadoRonda.Jugando &&
            GameManager.Instance.CantidadCartasOponenteJugadas < 3)
            {
                JugarCarta();
            }

            yield break;
        }

        float chance = Random.value;

        GameManager.Instance.DebugEstadoCantos();

        // ⚠️ Solo una subida permitida
        if (GameManager.Instance.EnvidoCantos.Count >= 2)
        {
            Debug.LogWarning("❌ Ya se subió una vez, la IA no puede subir más.");
            yield return StartCoroutine(ResponderEnvidoCoroutine());
            yield break;
        }

        // 🛑 No se puede bajar: si ya estamos en Real o Falta, no subir con Envido
        if (GameManager.Instance.TipoDeEnvidoActual != GameManager.TipoEnvido.Envido)
        {
            Debug.LogWarning("❌ Último canto no fue Envido. No se puede subir con Envido.");
            yield return StartCoroutine(ResponderEnvidoCoroutine());
            yield break;
        }

        if (GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.Envido) &&
            Random.value < chanceResponderConSubida)
        {
            if (Random.value < chanceCantarEnvido)
            {
                GameManager.TipoEnvido tipoSubida = Random.value < chanceDeQueSeaReal
                    ? GameManager.TipoEnvido.RealEnvido
                    : GameManager.TipoEnvido.Envido;

                // ❌ No se puede repetir lo mismo
                if (GameManager.Instance.YaCantoEsteJugador(tipoSubida, false))
                {
                    Debug.LogWarning($"❌ IA ya cantó {tipoSubida}, no puede repetir.");
                    yield return StartCoroutine(ResponderEnvidoCoroutine());
                    yield break;
                }

                // ❌ No se puede subir con Real Envido si ya fue dicho (por cualquiera)
                if (tipoSubida == GameManager.TipoEnvido.RealEnvido &&
                    GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.RealEnvido))
                {
                    Debug.LogWarning("❌ Real Envido ya fue cantado, no puede repetirse.");
                    yield return StartCoroutine(ResponderEnvidoCoroutine());
                    yield break;
                }

                GameManager.Instance.CantarEnvido(tipoSubida, false);
                yield break;
            }
        }

        // Si no sube más → simplemente responde
        yield return StartCoroutine(ResponderEnvidoCoroutine());
    }

    private bool EvaluarSiIrse()
    {
        bool yaPerdioPrimera = GameManager.Instance.ManosGanadasJugador > GameManager.Instance.ManosGanadasOponente;

        var disponibles = GameManager.Instance.AllCards
            .Where(c => c != null && c.isOpponent && !c.hasBeenPlayed && c.gameObject != null)
            .ToList();

        bool tieneCartasMalas = disponibles.All(c => c.GetComponent<Carta>().jerarquiaTruco < 7);
        bool estaEnPeligro = GameManager.Instance.puntosJugador >= 27;

        if (yaPerdioPrimera && tieneCartasMalas && estaEnPeligro)
        {
            float r = Random.value;
            Debug.Log($"Evaluando irse: chanceDeIrse = {chanceDeIrse}, random = {r}");
            return r < chanceDeIrse;
        }

        return false;
    }

}
