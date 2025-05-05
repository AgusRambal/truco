using System.Collections;
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
    Mentiroso,
    Adaptativo
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
    [SerializeField, Range(0f, 1f)] private float chanceResponderEnvidoTrasTruco = 0.4f;

    [Header("Probabilidades de Irse")]
    [SerializeField] private bool usarEstiloParaChancesIrse = true;
    [SerializeField, Range(0f, 1f)] private float chanceDeIrse = 0.5f;

    private enum EstiloJugada
    {
        Fuerte,
        Débil,
        Amague
    }

    private class IAContextoDeDecision
    {
        public bool TieneCartasFuertes;
        public bool TieneCartasDebiles;
        public bool TieneCartasMedias;
        public bool PerdioPrimeraMano;
        public bool EstaEnPeligro;
        public bool EsMano;
        public int PuntosJugador;
        public int PuntosOponente;
    }

    public void CargarConfiguracionIA(ConfiguracionIA config)
    {
        estilo = config.estilo;

        // Cargar valores a las variables del inspector para debugging/edición
        chanceCantarEnvido = config.chanceCantarEnvido;
        chanceDeQueSeaReal = config.chanceDeQueSeaReal;
        chanceResponderConSubida = config.chanceResponderConSubida;
        chanceResponderEnvidoTrasTruco = config.chanceResponderEnvidoTrasTruco;

        chanceCantarTruco = config.chanceCantarTruco;
        chanceCantarRetruco = config.chanceCantarRetruco;
        chanceCantarValeCuatro = config.chanceCantarValeCuatro;
        chanceResponderTruco = config.chanceResponderTruco;

        chanceDeIrse = config.chanceDeIrse;
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
            ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} decide irse antes de jugar carta", TipoMensaje.Sistema);
            GameManager.Instance.MeVoy(false); // false = IA
            yield break;
        }

        var contexto = AnalizarContexto();

        if (!GameManager.Instance.EnvidoCantado &&
            GameManager.Instance.CantidadCartasOponenteJugadas == 0 &&
            GameManager.Instance.TrucoState == 0)
        {
            float chanceFinal = chanceCantarEnvido;

            // Ejemplo de lógica por estilo con contexto
            if (estilo == EstiloIA.Conservador && contexto.EstaEnPeligro)
                chanceFinal *= 1.2f;

            if (estilo == EstiloIA.Agresivo && contexto.TieneCartasFuertes)
                chanceFinal *= 1.3f;

            if (estilo == EstiloIA.Canchero && contexto.EsMano)
                chanceFinal *= 1.1f;

            chanceFinal = Mathf.Clamp01(chanceFinal);

            if (Random.value < chanceFinal)
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
            var ctx = AnalizarContexto();

            if (trucoState == 0)
            {
                float chanceFinal = chanceCantarTruco;

                if (estilo == EstiloIA.Canchero && ctx.TieneCartasMedias && ctx.EsMano)
                    chanceFinal *= 1.2f;

                if (estilo == EstiloIA.Agresivo && ctx.TieneCartasFuertes)
                    chanceFinal *= 1.4f;

                if (estilo == EstiloIA.Conservador && ctx.EstaEnPeligro)
                    chanceFinal *= 0.6f;

                if (estilo == EstiloIA.Mentiroso && ctx.TieneCartasDebiles)
                    chanceFinal *= 1.1f;

                chanceFinal = Mathf.Clamp01(chanceFinal);

                if (Random.value < chanceFinal)
                {
                    ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} canto truco", TipoMensaje.Sistema);
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
            else if (trucoState == 1 && Random.value < chanceCantarRetruco)
            {
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} canto retruco", TipoMensaje.Sistema);
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
            else if (trucoState == 2 && Random.value < chanceCantarValeCuatro)
            {
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} canto vale cuatro", TipoMensaje.Sistema);
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

        // Si se empató la subronda anterior
        if (GameManager.Instance.UltimaManoFueEmpate)
        {
            // Si ya ganamos la primera → no se debe jugar más (IA no debería estar jugando)
            if (GameManager.Instance.ManosGanadasOponente == 1 &&
                GameManager.Instance.ManosGanadasJugador == 0 &&
                GameManager.Instance.CantidadCartasOponenteJugadas == 2)
            {
                Debug.LogWarning("IA: Se intentó jugar carta cuando ya había ganado por empate en segunda mano. Ignorando jugada.");
                yield break;
            }

            // Si el empate fue en la primera, y hay que definir → tirar la más fuerte
            elegida = disponibles.OrderByDescending(c => c.GetComponent<Carta>().jerarquiaTruco).First();
        }
        else
        {
            var ctx = AnalizarContexto();
            var cartasOrdenadasIA = disponibles.OrderByDescending(c => c.GetComponent<Carta>().jerarquiaTruco).ToList();

            EstiloJugada estiloJugada;

            if (estilo == EstiloIA.Conservador)
            {
                estiloJugada = ctx.EstaEnPeligro ? EstiloJugada.Fuerte : EstiloJugada.Débil;
            }
            else if (estilo == EstiloIA.Agresivo)
            {
                estiloJugada = ctx.TieneCartasFuertes ? EstiloJugada.Fuerte : EstiloJugada.Amague;
            }
            else if (estilo == EstiloIA.Canchero)
            {
                estiloJugada = Random.value < 0.6f ? EstiloJugada.Amague : EstiloJugada.Fuerte;
            }
            else if (estilo == EstiloIA.Mentiroso)
            {
                estiloJugada = Random.value < 0.5f ? EstiloJugada.Amague : EstiloJugada.Débil;
            }
            else if (estilo == EstiloIA.Calculador)
            {
                if (ctx.TieneCartasFuertes && ctx.EsMano)
                    estiloJugada = EstiloJugada.Fuerte;
                else if (ctx.TieneCartasDebiles)
                    estiloJugada = EstiloJugada.Débil;
                else
                    estiloJugada = EstiloJugada.Amague;
            }
            else // Caótico u otro
            {
                estiloJugada = (EstiloJugada)Random.Range(0, 3);
            }

            switch (estiloJugada)
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

    public void ResponderTruco()
    {
        StartCoroutine(ResponderTrucoCoroutine());
    }

    private IEnumerator ResponderTrucoCoroutine()
    {
        float delay = Random.Range(minTrucoResponseTime, maxTrucoResponseTime);
        yield return new WaitForSeconds(delay);

        if (!GameManager.Instance.SeJugoCartaDesdeUltimoCanto &&
            !GameManager.Instance.EnvidoCantado &&
            GameManager.Instance.EnvidoCantos.Count == 0 &&
            GameManager.Instance.estadoRonda == EstadoRonda.EsperandoRespuesta)
        {
            var ctx2 = AnalizarContexto();

            float chanceFinal = chanceResponderEnvidoTrasTruco;

            // Podés modificar la probabilidad con el contexto si querés:
            if (estilo == EstiloIA.Canchero && ctx2.EsMano)
                chanceFinal *= 1.1f;

            if (ctx2.TieneCartasFuertes)
                chanceFinal *= 1.2f;

            chanceFinal = Mathf.Clamp01(chanceFinal);

            if (Random.value < chanceFinal)
            {
                GameManager.TipoEnvido tipoACantar = Random.value < chanceDeQueSeaReal
                    ? GameManager.TipoEnvido.RealEnvido
                    : GameManager.TipoEnvido.Envido;

                GameManager.Instance.CantarEnvido(tipoACantar, false);
                yield break;
            }
        }

        var ctx = AnalizarContexto();

        int estadoActual = GameManager.Instance.TrucoState;
        bool quiereSubir = false;

        // Subir Truco (Retruco o Vale Cuatro) con contexto
        if (estadoActual == 0)
            quiereSubir = ctx.TieneCartasFuertes && Random.value < chanceCantarRetruco;
        else if (estadoActual == 1)
            quiereSubir = ctx.TieneCartasFuertes && Random.value < chanceCantarValeCuatro;

        if (quiereSubir)
        {
            if (estadoActual == 1)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Retruco);
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} canta {UIManager.TrucoMensajeTipo.Retruco}", TipoMensaje.Sistema);
            }

            else if (estadoActual == 2)
            {
                uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.ValeCuatro);
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} canta {UIManager.TrucoMensajeTipo.ValeCuatro}", TipoMensaje.Sistema);
            }

            GameManager.Instance.TrucoState++;
            GameManager.Instance.puntosEnJuego += 1;
            GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;
            GameManager.Instance.ChangeTruco();
            GameManager.Instance.uiManager.MostrarOpcionesTruco();
            GameManager.Instance.SeJugoCartaDesdeUltimoCanto = false;
            GameManager.Instance.UltimoCantoFueDelJugador = false;
            GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();
            yield break;
        }

        bool acepta;

        if (ctx.TieneCartasFuertes)
        {
            acepta = true;
        }
        else if (ctx.TieneCartasMedias && ctx.PuntosOponente < 27)
        {
            acepta = Random.value < chanceResponderTruco;
        }
        else
        {
            acepta = Random.value < (chanceResponderTruco * 0.5f); // más conservador si tiene cartas malas
        }

        if (acepta)
        {
            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.Quiero);

            string nombreCanto = GameManager.Instance.TrucoState switch
            {
                1 => "Truco",
                2 => "Retruco",
                3 => "Vale Cuatro",
                _ => "Truco"
            };

            ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} acepto el {nombreCanto}", TipoMensaje.Sistema); 
            GameManager.Instance.puntosEnJuego = GameManager.Instance.TrucoState + 1;
            GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
            GameManager.Instance.ChangeTruco();

            if (GameManager.Instance.TrucoState < 3)
                GameManager.Instance.PuedeResponderTruco = false;

            // Forzar jugada de IA si le toca después de aceptar
            if (GameManager.Instance.turnoActual == TurnoActual.Oponente &&
                GameManager.Instance.estadoRonda == EstadoRonda.Jugando &&
                GameManager.Instance.CantidadCartasOponenteJugadas < 3)
            {
                GameManager.Instance.iaOponente.JugarCarta();
            }
        }
        else
        {
            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);

            string nombreCanto = GameManager.Instance.TrucoState switch
            {
                1 => "Truco",
                2 => "Retruco",
                3 => "Vale Cuatro",
                _ => "Truco"
            };

            ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} no acepto el {nombreCanto}", TipoMensaje.Sistema);
            GameManager.Instance.puntosEnJuego = GameManager.Instance.TrucoState;
            GameManager.Instance.SumarPuntos(true, true);
        }
    }

    public IEnumerator ResponderEnvidoCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        var ctx = AnalizarContexto();

        // Si el jugador tiene 29, la IA tiene que aceptar para no perder
        bool quiere = ctx.PuntosJugador == GameManager.Instance.PointsToEnd - 1 ||
                      ctx.TieneCartasFuertes ||
                      (ctx.TieneCartasMedias && ctx.PuntosOponente < 25);

        int envidoJugador = GameManager.Instance.CalcularPuntosEnvido(true);
        int envidoOponente = GameManager.Instance.CalcularPuntosEnvido(false);

        if (GameManager.Instance.TipoDeEnvidoActual == GameManager.TipoEnvido.FaltaEnvido)
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

            ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} no quiso Falta Envido", TipoMensaje.Sistema);
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

            string nombreEnvido = GameManager.Instance.TipoDeEnvidoActual switch
            {
                GameManager.TipoEnvido.Envido => "Envido",
                GameManager.TipoEnvido.RealEnvido => "Real Envido",
                GameManager.TipoEnvido.FaltaEnvido => "Falta Envido",
                _ => "Envido"
            };

            ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} acepto el {nombreEnvido}", TipoMensaje.Sistema);

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
                GameManager.Instance.SumarPuntos(false, puntosAGanar, false);
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} gano el {nombreEnvido} (+{puntosAGanar})", TipoMensaje.Sistema);
            }

            else
            {
                GameManager.Instance.SumarPuntos(true, puntosAGanar, false);
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(true)} gano el {nombreEnvido} (+{puntosAGanar})", TipoMensaje.Sistema);
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
                GameManager.Instance.SumarPuntos(true, puntosPorNoQuerer, false);
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(true)} no quiso el envido (+{puntosPorNoQuerer})", TipoMensaje.Sistema);
            }

            else
            {
                GameManager.Instance.SumarPuntos(false, puntosPorNoQuerer, false);
                ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} no quiso el envido (+{puntosPorNoQuerer})", TipoMensaje.Sistema);
            }

            uiManager.MostrarTrucoMensaje(false, UIManager.TrucoMensajeTipo.NoQuiero);
            uiManager.SetPointsInScreen(GameManager.Instance.puntosJugador, GameManager.Instance.puntosOponente);
        }

        GameManager.Instance.EnvidoRespondido = true;
        GameManager.Instance.EnvidoCantado = false;

        if (GameManager.Instance.estadoRonda != EstadoRonda.Finalizado)
        {
            if (GameManager.Instance.TrucoState > 0 &&
                GameManager.Instance.PuedeResponderTruco &&
                !GameManager.Instance.SeJugoCartaDesdeUltimoCanto)
            {
                GameManager.Instance.estadoRonda = EstadoRonda.EsperandoRespuesta;

                if (GameManager.Instance.UltimoCantoFueDelJugador)
                    ResponderTruco();
                else
                    GameManager.Instance.uiManager.MostrarOpcionesTruco();

                yield break;
            }

            GameManager.Instance.estadoRonda = EstadoRonda.Jugando;
            GameManager.Instance.uiManager.ActualizarBotonesSegunEstado();

            if (GameManager.Instance.turnoActual == TurnoActual.Oponente &&
                GameManager.Instance.CantidadCartasOponenteJugadas < 3)
            {
                JugarCarta();
            }
        }

    }

    public IEnumerator ResponderEnvidoExtendido()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

        if (GameManager.Instance.TipoDeEnvidoActual == GameManager.TipoEnvido.FaltaEnvido)
        {
            ChatManager.Instance.AgregarMensaje($"{GameManager.Instance.NombreJugador(false)} no quiso falta envido", TipoMensaje.Sistema);
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

        // Solo una subida permitida
        if (GameManager.Instance.EnvidoCantos.Count >= 2)
        {
            Debug.LogWarning(" Ya se subió una vez, la IA no puede subir más.");
            yield return StartCoroutine(ResponderEnvidoCoroutine());
            yield break;
        }

        // No se puede bajar: si ya estamos en Real o Falta, no subir con Envido
        if (GameManager.Instance.TipoDeEnvidoActual != GameManager.TipoEnvido.Envido)
        {
            Debug.LogWarning(" Último canto no fue Envido. No se puede subir con Envido.");
            yield return StartCoroutine(ResponderEnvidoCoroutine());
            yield break;
        }

        var ctx = AnalizarContexto();

        // Solo si tiene cartas buenas y no está al borde de perder
        bool deberiaSubir = ctx.TieneCartasFuertes && ctx.PuntosJugador < GameManager.Instance.PointsToEnd - 1;

        if (GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.Envido) && deberiaSubir && Random.value < chanceResponderConSubida)
        {
            if (Random.value < chanceCantarEnvido)
            {
                GameManager.TipoEnvido tipoSubida = Random.value < chanceDeQueSeaReal
                    ? GameManager.TipoEnvido.RealEnvido
                    : GameManager.TipoEnvido.Envido;

                // No se puede repetir lo mismo
                if (GameManager.Instance.YaCantoEsteJugador(tipoSubida, false))
                {
                    Debug.LogWarning($" IA ya cantó {tipoSubida}, no puede repetir.");
                    yield return StartCoroutine(ResponderEnvidoCoroutine());
                    yield break;
                }

                // No se puede subir con Real Envido si ya fue dicho (por cualquiera)
                if (tipoSubida == GameManager.TipoEnvido.RealEnvido &&
                    GameManager.Instance.EnvidoCantos.Contains(GameManager.TipoEnvido.RealEnvido))
                {
                    Debug.LogWarning(" Real Envido ya fue cantado, no puede repetirse.");
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

    private IAContextoDeDecision AnalizarContexto()
    {
        var ctx = new IAContextoDeDecision();

        var disponibles = GameManager.Instance.AllCards
            .Where(c => c != null && c.isOpponent && !c.hasBeenPlayed && c.gameObject != null)
            .Select(c => c.GetComponent<Carta>())
            .ToList();

        ctx.TieneCartasFuertes = disponibles.Any(c => c.jerarquiaTruco >= 12);
        ctx.TieneCartasDebiles = disponibles.All(c => c.jerarquiaTruco < 7);
        ctx.TieneCartasMedias = disponibles.Any(c => c.jerarquiaTruco >= 7 && c.jerarquiaTruco < 12);

        ctx.PerdioPrimeraMano = GameManager.Instance.ManosGanadasJugador > GameManager.Instance.ManosGanadasOponente;
        ctx.EstaEnPeligro = GameManager.Instance.puntosJugador >= 27;
        ctx.EsMano = GameManager.Instance.CantidadCartasOponenteJugadas == 0 && GameManager.Instance.turnoActual == TurnoActual.Oponente;
        ctx.PuntosJugador = GameManager.Instance.puntosJugador;
        ctx.PuntosOponente = GameManager.Instance.puntosOponente;

        return ctx;
    }

}
