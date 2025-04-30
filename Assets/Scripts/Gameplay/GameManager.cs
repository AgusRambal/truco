using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EstadoRonda
{
    Repartiendo,
    Jugando,
    EsperandoRespuesta,
    Finalizado
}

public enum TurnoActual
{
    Jugador,
    Oponente
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scriptable obejcts")]
    public List<CartaSO> cartas = new List<CartaSO>();
    public List<CartaSO> cartasBackup = new List<CartaSO>();

    [Header("References")]
    public UIManager uiManager;
    public GameObject carta;
    public IAOponente iaOponente;

    [Header("Transforms")]
    public Transform[] handPosition;
    public Transform[] handOponentPosition;
    public Transform target;
    public Transform spawnPosition;

    [Header("Parameters")]
    [SerializeField] private string oponentName;
    [SerializeField] private float setTime = 0.25f;
    [SerializeField] private float resetTime = 0.25f;
    [SerializeField] private float devolverCartasWaitTime = 0.1f;

    [Header("Game stats")]
    public EstadoRonda estadoRonda = EstadoRonda.Jugando;
    public TurnoActual turnoActual = TurnoActual.Jugador;

    [Header("Audio")]
    [SerializeField] private List<AudioClip> drops = new List<AudioClip>();
    [SerializeField] private List<AudioClip> plays = new List<AudioClip>();

    //Hidden
    [HideInInspector] public int puntosEnJuego = 1;
     public int puntosOponente = 0;
     public int puntosJugador = 0;
    [HideInInspector] public int TrucoState = 0;
    [HideInInspector] public bool SeJugoCartaDesdeUltimoCanto = true;
    [HideInInspector] public bool UltimoCantoFueDelJugador = false;
    [HideInInspector] public bool isPaused = false;
    [HideInInspector] public bool EnvidoFueDelJugador = false;
    [HideInInspector] public bool ganoJugador = false;
    [HideInInspector] public bool PuedeResponderTruco = true;
    [HideInInspector] public TipoEnvido TipoDeEnvidoActual;
    [HideInInspector] public List<TipoEnvido> EnvidoCantos = new List<TipoEnvido>();
    [HideInInspector] public int gameSubPointState = 0;
    [HideInInspector] public bool EnvidoCantado = false;
    [HideInInspector] public bool EnvidoRespondido = false;

    //Privates
    private List<CardSelector> allCards = new List<CardSelector>();
    private bool ultimaManoFueEmpate = false;
    private List<CartaSO> mazo = new List<CartaSO>();
    private List<CardSelector> cartasEnMano = new List<CardSelector>();
    private List<Carta> cartasOponenteJugadas = new List<Carta>();
    private List<Carta> cartasJugadorJugadas = new List<Carta>();
    private float zOffsetCentroMesa = 0f;
    private bool rondaSeDefiniraEnProxima = false;
    private bool turnoJugadorEmpieza = true;
    private int pointsToEnd;
    private int manosGanadasJugador = 0;
    private int manosGanadasOponente = 0;
    private Dictionary<TipoEnvido, bool> cantosPorJugador = new();

    //Propiedades
    public int CantidadCartasJugadorJugadas => cartasJugadorJugadas.Count;
    public int CantidadCartasOponenteJugadas => cartasOponenteJugadas.Count;
    public List<CardSelector> AllCards => allCards;
    public bool UltimaManoFueEmpate => ultimaManoFueEmpate;
    public int PointsToEnd => pointsToEnd;
    public int ManosGanadasJugador => manosGanadasJugador;
    public int ManosGanadasOponente => manosGanadasOponente;

    public enum TipoEnvido
    {
        Envido,
        RealEnvido,
        FaltaEnvido
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        cartas = Utils.ParametrosDePartida.cartasSeleccionadas != null && Utils.ParametrosDePartida.cartasSeleccionadas.Count == 40
            ? new List<CartaSO>(Utils.ParametrosDePartida.cartasSeleccionadas)
            : CartaSaveManager.CargarCartas(cartasBackup); 

        pointsToEnd = Utils.ParametrosDePartida.puntosParaGanar;
        iaOponente.estilo = Utils.ParametrosDePartida.estiloSeleccionado;

        Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.PartidasJugadas);
    }

    private void Start()
    {
        mazo = new List<CartaSO>(cartas);
        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        Invoke("SpawnCards", .5f);
        Invoke("DelayedGlow", .5f);
    }

    public void DelayedGlow()
    { 
        uiManager.FadePlayerTurn(turnoActual);
    }

    public void SpawnCards()
    {
        uiManager.ResetSubRondas();
        uiManager.ActualizarBotonesSegunEstado();
        estadoRonda = EstadoRonda.Repartiendo;
        StartCoroutine(SpawnCardsSequence());
        uiManager.MostrarMano(turnoActual);

        bool isPlayer = (turnoActual == TurnoActual.Jugador);
        ChatManager.Instance.AgregarMensaje($"{NombreJugador(isPlayer)} es mano", TipoMensaje.Sistema);
    }

    private IEnumerator SpawnCardsSequence()
    {
        mazo = new List<CartaSO>(cartas);

        cartasEnMano.Clear();
        int playerIndex = 0;
        int opponentIndex = 0;

        for (int i = 0; i < 6; i++) // 3 para cada uno, intercalado
        {
            bool isOpponentDraw = (i % 2 == 0); // arranca el oponente

            var instantiatedCard = Instantiate(carta, spawnPosition.position, Quaternion.Euler(0f, 180f, 0f));
            var cardSelector = instantiatedCard.GetComponent<CardSelector>();

            // Elegir carta random y asignar valores
            var randomCard = mazo[Random.Range(0, mazo.Count)];
            mazo.Remove(randomCard);

            var newCard = instantiatedCard.GetComponent<Carta>();
            newCard.palo = randomCard.palo;
            newCard.valor = randomCard.valor;
            newCard.jerarquiaTruco = randomCard.jerarquiaTruco;
            newCard.imagen.sprite = randomCard.imagen;

            if (isOpponentDraw)
            {
                cardSelector.isOpponent = true;
            }

            AudioManager.Instance.PlaySFX(GetRandomDrop(drops));
            Sequence s = DOTween.Sequence();

            if (isOpponentDraw)
            {
                s.Append(instantiatedCard.transform.DOMove(handOponentPosition[opponentIndex].position, setTime).SetEase(Ease.OutCubic));
                s.Join(instantiatedCard.transform.DORotate(new Vector3(-10f, 180f, 0f), setTime).SetEase(Ease.OutCubic));

                instantiatedCard.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 180f, 0f);
                instantiatedCard.transform.GetChild(0).localPosition = new Vector3(0f, 0f, -0.1f);

                opponentIndex++;
            }
            else
            {
                s.Append(instantiatedCard.transform.DOMove(handPosition[playerIndex].position, setTime).SetEase(Ease.OutCubic));
                s.Join(instantiatedCard.transform.DORotate(new Vector3(-10f, 360f, 0f), setTime).SetEase(Ease.OutCubic));

                playerIndex++;
            }

            cartasEnMano.Add(cardSelector);
            allCards.Add(cardSelector);

            yield return s.WaitForCompletion(); // Esperar a que termine antes de repartir la siguiente
        }

        estadoRonda = EstadoRonda.Jugando;

        // Dejamos preparado el turno, pero lo activamos después de un delay pequeño
        StartCoroutine(ActivarTurnoInicial());
    }

    private IEnumerator ActivarTurnoInicial()
    {
        yield return new WaitForSeconds(0.5f); // aseguramos que la UI ya está lista

        turnoActual = turnoJugadorEmpieza ? TurnoActual.Jugador : TurnoActual.Oponente;

        if (turnoActual == TurnoActual.Oponente)
        {
            iaOponente.JugarCarta();
        }
        else
        {
            uiManager.ActualizarBotonesSegunEstado();
        }
    }

    public void CartaJugada(CardSelector carta)
    {
        if (carta == null || carta.gameObject == null) return;

        var cartaData = carta.GetComponent<Carta>();
        SeJugoCartaDesdeUltimoCanto = true;

        if (carta.isOpponent)
        {
            cartasOponenteJugadas.Add(cartaData);
        }
        else
        {
            cartasJugadorJugadas.Add(cartaData);
        }

        AudioManager.Instance.PlaySFX(GetRandomDrop(plays));

        // Si ya jugaron ambos → evaluar mano
        if (cartasJugadorJugadas.Count > 0 && cartasJugadorJugadas.Count == cartasOponenteJugadas.Count)
        {
            EvaluarMano(cartasJugadorJugadas[^1], cartasOponenteJugadas[^1]);
        }
        else
        {
            if (TrucoState < 3)
            {
                PuedeResponderTruco = true;
            }

            if (turnoActual == TurnoActual.Oponente)
            {
                turnoActual = TurnoActual.Jugador;
            }

            else
            {
                turnoActual = TurnoActual.Oponente;
                iaOponente.JugarCarta();
            }

            uiManager.FadePlayerTurn(turnoActual);
            uiManager.ActualizarBotonesSegunEstado();
        }
    }

    private void EvaluarMano(Carta jug, Carta opo)
    {
        if (jug.jerarquiaTruco > opo.jerarquiaTruco)
        {
            manosGanadasJugador++;
            ultimaManoFueEmpate = false;
            turnoActual = TurnoActual.Jugador;
        }
        else if (opo.jerarquiaTruco > jug.jerarquiaTruco)
        {
            manosGanadasOponente++;
            ultimaManoFueEmpate = false;
            turnoActual = TurnoActual.Oponente;
        }
        else
        {
            ChatManager.Instance.AgregarMensaje($"Empate en la mano", TipoMensaje.Sistema);
            ultimaManoFueEmpate = true;

            // Si es la PRIMERA mano, la ronda se definirá en la siguiente
            if (cartasJugadorJugadas.Count == 1)
                rondaSeDefiniraEnProxima = true;

            // En caso de empate: no cambiar turnoActual
        }

        int subrondaActual = cartasJugadorJugadas.Count;

        if (jug.jerarquiaTruco > opo.jerarquiaTruco)
        {
            uiManager.MostrarSubPuntos(true, subrondaActual);
        }

        else if (opo.jerarquiaTruco > jug.jerarquiaTruco)
        {
            uiManager.MostrarSubPuntos(false, subrondaActual);
        }

        uiManager.FadePlayerTurn(turnoActual);
        VerificarFinDeRonda();
    }

    public void VerificarFinDeRonda()
    {
        // Caso especial: la ronda se definía en la subronda 2 (por empate en subronda 1)
        if (rondaSeDefiniraEnProxima && cartasJugadorJugadas.Count == 2 && cartasOponenteJugadas.Count == 2)
        {
            if (manosGanadasJugador > manosGanadasOponente)
                SumarPuntos(true, true);
            else if (manosGanadasOponente > manosGanadasJugador)
                SumarPuntos(false, true);
            else
            {
                bool ganaJugador = turnoJugadorEmpieza;
                ChatManager.Instance.AgregarMensaje($"Empate incluso en mano definitoria", TipoMensaje.Sistema);
                ChatManager.Instance.AgregarMensaje($"Gana {(ganaJugador ? SteamUserManager.Instance.PlayerName : oponentName)} por ser mano", TipoMensaje.Sistema);
                SumarPuntos(ganaJugador, true);
            }

            return;
        }

        if (manosGanadasJugador == 2)
        {
            SumarPuntos(true, true);
            return;
        }

        if (manosGanadasOponente == 2)
        {
            SumarPuntos(false, true);
            return;
        }

        if (cartasJugadorJugadas.Count == 3 && cartasOponenteJugadas.Count == 3)
        {
            if (manosGanadasJugador > manosGanadasOponente)
                SumarPuntos(true, true);
            else if (manosGanadasOponente > manosGanadasJugador)
                SumarPuntos(false, true);
            else
            {
                bool ganaJugador = turnoJugadorEmpieza;
                ChatManager.Instance.AgregarMensaje($"Empate triple", TipoMensaje.Sistema);
                ChatManager.Instance.AgregarMensaje($"Gana {(ganaJugador ? SteamUserManager.Instance.PlayerName : oponentName)} por ser mano", TipoMensaje.Sistema);
                SumarPuntos(ganaJugador, true);
            }
        }

        if (estadoRonda == EstadoRonda.Jugando)
        {
            if (turnoActual == TurnoActual.Oponente)
            {
                iaOponente.JugarCarta();
            }

            uiManager.ActualizarBotonesSegunEstado();
        }
    }

    private void FinalCheck()
    {
        if (puntosJugador >= pointsToEnd || puntosOponente >= pointsToEnd)
        {
            estadoRonda = EstadoRonda.Finalizado;
            StopAllCoroutines();
            DevolverCartas();
            bool ganoJugador = puntosJugador >= pointsToEnd;
            int ganancia = 0;

            if (ganoJugador)
            {
                ganancia = Utils.ParametrosDePartida.gananciaCalculada;
                SaveSystem.Datos.monedas += ganancia;
                SaveSystem.GuardarDatos();
                ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} gana {ganancia} créditos", TipoMensaje.Sistema);
            }

            uiManager.MostrarResultadoFinal(ganoJugador, ganancia);
        }
    }

    public void FinalizarRonda()
    {
        cartasJugadorJugadas.Clear();
        cartasOponenteJugadas.Clear();
        EnvidoCantos.Clear();
        manosGanadasJugador = 0;
        manosGanadasOponente = 0;
        ultimaManoFueEmpate = false;
        rondaSeDefiniraEnProxima = false;
        EnvidoCantado = false;
        PuedeResponderTruco = true;
        EnvidoRespondido = false;
        EnvidoFueDelJugador = false;
        uiManager.BlockMeVoy(true);

        ResetZOffset();
        uiManager.ResetTruco();
        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);

        DevolverCartas();

        puntosEnJuego = 1;
        TrucoState = 0;

        //Resetear lógica de canto
        SeJugoCartaDesdeUltimoCanto = true;
        UltimoCantoFueDelJugador = false;

        //Alternar quién empieza
        turnoJugadorEmpieza = !turnoJugadorEmpieza;
        turnoActual = turnoJugadorEmpieza ? TurnoActual.Jugador : TurnoActual.Oponente;

        estadoRonda = EstadoRonda.Repartiendo;
    }

    private void DevolverCartas()
    {
        uiManager.ActualizarBotonesSegunEstado();
        StartCoroutine(DevolverCartasSequence());
    }

    private IEnumerator DevolverCartasSequence()
    {
        yield return new WaitForSeconds(devolverCartasWaitTime);

        foreach (var c in allCards)
        {
            Transform cartaTransform = c.transform;
            Sequence s = DOTween.Sequence();
            AudioManager.Instance.PlaySFX(GetRandomDrop(drops));

            if (!c.isOpponent)
            {
                cartaTransform.GetChild(0).localRotation = Quaternion.Euler(0f, 180f, 0f);
                cartaTransform.GetChild(0).localPosition = new Vector3(0f, 0f, -0.1f);
            }

            Vector3 targetRot = new Vector3(-5.732f, 180f, 0f);

            s.Append(cartaTransform.DOMove(spawnPosition.position, resetTime).SetEase(Ease.InOutCubic));
            s.Join(cartaTransform.DORotate(targetRot, resetTime).SetEase(Ease.InOutCubic));

            yield return s.WaitForCompletion();
        }

        foreach (var c in allCards)
        {
            Destroy(c.gameObject);
        }

        cartasEnMano.Clear();
        allCards.Clear();

        yield return new WaitForSeconds(0.5f);

        if (estadoRonda != EstadoRonda.Finalizado)
        {
            SpawnCards(); 
        }
    }

    public float GetZOffset()
    {
        float offset = zOffsetCentroMesa;
        zOffsetCentroMesa -= 0.05f;
        return offset;
    }

    public void ResetZOffset()
    {
        zOffsetCentroMesa = 0f;
    }

    public void ChangeTruco()
    {
        uiManager.ChangeTrucoState(TrucoState);
    }

    public void CantarTruco()
    {
        string nombreCanto = TrucoState switch
        {
            1 => "Truco",
            2 => "Retruco",
            3 => "Vale Cuatro",
            _ => "Truco"
        };

        if (!SeJugoCartaDesdeUltimoCanto && UltimoCantoFueDelJugador)
        {
            Debug.Log("No podés volver a cantar sin que se juegue una carta.");
            return;
        }

        if (TrucoState == 0)
        { 
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Truco);
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.TrucosCantados);
            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} canta {nombreCanto}", TipoMensaje.Sistema);
        }

        else if (TrucoState == 1)
        {   
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Retruco);
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RetrucosCantados);
            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} canta {nombreCanto}", TipoMensaje.Sistema);
        }

        else if (TrucoState == 2)
        { 
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.ValeCuatro);
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.ValeCuatroCantados);
            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} canta {nombreCanto}", TipoMensaje.Sistema);
        }

        TrucoState++;
        estadoRonda = EstadoRonda.EsperandoRespuesta;
        uiManager.OcultarOpcionesTruco();
        iaOponente.ResponderTruco();

        SeJugoCartaDesdeUltimoCanto = false;
        UltimoCantoFueDelJugador = true;
    }

    public void RespuestaJugadorTruco(bool quiero)
    {
        uiManager.OcultarOpcionesTruco();

        if (quiero)
        {
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Quiero);

            string nombreCanto = TrucoState switch
            {
                1 => "Truco",
                2 => "Retruco",
                3 => "Vale Cuatro",
                _ => "Truco"
            };

            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} acepto el {nombreCanto}", TipoMensaje.Sistema);

            if (nombreCanto == "Truco")
            {
                Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.TrucosAceptados);
            }

            else if (nombreCanto == "Retruco")
            {
                Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RetrucosAceptados);
            }

            else
            {
                Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.ValeCuatroAceptados);
            }

            puntosEnJuego = TrucoState + 1;
            puntosEnJuego++;
            estadoRonda = EstadoRonda.Jugando;
            PuedeResponderTruco = false;
            UltimoCantoFueDelJugador = true;
            ChangeTruco();
            uiManager.ActualizarBotonesSegunEstado();
        }

        else
        {
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.NoQuiero);

            string nombreCanto = TrucoState switch
            {
                1 => "Truco",
                2 => "Retruco",
                3 => "Vale Cuatro",
                _ => "Truco"
            };

            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} no acepto el {nombreCanto}", TipoMensaje.Sistema); 

            puntosEnJuego = TrucoState + 1;
            SumarPuntos(false, true);
        }

        if (turnoActual == TurnoActual.Oponente)
        {
            iaOponente.JugarCarta();
        }
    }

    public void SumarPuntos(bool isPlayer, bool finish)
    {
        if (isPlayer) puntosJugador += puntosEnJuego;
        else puntosOponente += puntosEnJuego;

        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);

        if (finish)
        {
            if (TrucoState > 0)
            {
                if (isPlayer)
                    ContarGanadoTruco();
                else
                    ContarPerdidoTruco();
            }

            FinalizarRonda();
            FinalCheck();
        }
    }

    public void SumarPuntos(bool isPlayer, int amount, bool finish)
    {
        if (isPlayer) puntosJugador += amount;
        else puntosOponente += amount;

        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);

        FinalCheck();

        if (finish)
        {
            FinalizarRonda();
        }
    }

    public void MeVoy(bool esJugador)
    {
        if (estadoRonda != EstadoRonda.Jugando)
            return;

        if (esJugador)
        {
            SumarPuntos(true, true);
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.VecesQueTeFuiste);
            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} se fue al mazo", TipoMensaje.Sistema);
        }

        else 
        {
            SumarPuntos(false, true);
            ChatManager.Instance.AgregarMensaje($"{NombreJugador(false)} se fue al mazo", TipoMensaje.Sistema);
        }

        uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.MeVoy);
        uiManager.BlockMeVoy(false);
    }

    public void CantarEnvido(TipoEnvido tipo, bool esJugador)
    {
        if (EnvidoRespondido || TrucoState > 0 || estadoRonda != EstadoRonda.Jugando)
            return;

        // Bloqueo global: ese tipo ya fue cantado por alguien
        if (EnvidoCantos.Contains(tipo))
        {
            // Pero si lo cantó el oponente, y ahora lo canto yo, está permitido
            if (YaCantoEsteJugador(tipo, esJugador))
            {
                Debug.LogWarning($" {NombreJugador(esJugador)} ya cantó {tipo}, no puede repetir.");
                return;
            }
        }

        if (!EnvidoCantado)
        {
            EnvidoCantado = true;
            EnvidoCantos.Clear();
            cantosPorJugador.Clear(); // <-- importante: limpiar al inicio

            EnvidoCantos.Add(tipo);
            TipoDeEnvidoActual = tipo;
            EnvidoFueDelJugador = esJugador;

            // Registrar solo si aún no fue cantado ese tipo
            if (!cantosPorJugador.ContainsKey(tipo))
                cantosPorJugador[tipo] = esJugador;
        }
        else
        {
            EnvidoCantos.Add(tipo);
            TipoDeEnvidoActual = tipo;
            EnvidoFueDelJugador = esJugador;

            // Registrar solo si aún no fue cantado ese tipo
            if (!cantosPorJugador.ContainsKey(tipo))
                cantosPorJugador[tipo] = esJugador;
        }

        string nombreEnvido = TipoDeEnvidoActual switch
        {
            TipoEnvido.Envido => "Envido",
            TipoEnvido.RealEnvido => "Real Envido",
            TipoEnvido.FaltaEnvido => "Falta Envido",
            _ => "Envido"
        };

        ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} canto {nombreEnvido}", TipoMensaje.Sistema);

        // Ocultar opciones si lo cantaste vos
        if (esJugador)
            uiManager.OcultarOpcionesEnvido();

        // Mostrar mensaje en pantalla
        uiManager.MostrarTrucoMensaje(EnvidoFueDelJugador, tipo switch
        {
            TipoEnvido.Envido => UIManager.TrucoMensajeTipo.Envido,
            TipoEnvido.RealEnvido => UIManager.TrucoMensajeTipo.RealEnvido,
            TipoEnvido.FaltaEnvido => UIManager.TrucoMensajeTipo.FaltaEnvido,
            _ => UIManager.TrucoMensajeTipo.Envido
        });

        // Lógica de turno
        if (EnvidoFueDelJugador)
        {
            StartCoroutine(iaOponente.ResponderEnvidoExtendido());
        }
        else
        {
            uiManager.MostrarOpcionesEnvido();
        }

        uiManager.ActualizarBotonesEnvido();
        uiManager.ActualizarBotonesSegunEstado();
    }

    public bool YaCantoEsteJugador(TipoEnvido tipo, bool esJugador)
    {
        if (!cantosPorJugador.ContainsKey(tipo))
            return false;

        return cantosPorJugador[tipo] == esJugador;
    }

    public void CantarEnvidoNormal(bool jugador)
    {
        CantarEnvido(TipoEnvido.Envido, jugador);
        Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.EnvidosCantados);
    }

    public void CantarRealEnvido(bool jugador)
    {
        CantarEnvido(TipoEnvido.RealEnvido, jugador);
        Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RealEnvidosCantados);
    }

    public void CantarFaltaEnvido(bool jugador)
    {
        CantarEnvido(TipoEnvido.FaltaEnvido, jugador);
        Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.FaltaEnvidosCantados);
    }

    public int CalcularPuntosEnvido(bool esJugador)
    {
        var cartas = cartasEnMano
            .Where(c => c != null && c.GetComponent<Carta>() != null && c.isOpponent == !esJugador)
            .Select(c => c.GetComponent<Carta>())
            .ToList();

        if (cartas.Count == 0)
        {
            Debug.LogWarning($"No hay cartas en mano del {(esJugador ? "jugador" : "oponente")} para calcular Envido.");
            return 0;
        }

        // Valor de Envido: 10, 11, 12 valen 0
        int ValorEnvido(Carta c)
        {
            return (c.valor >= 10 && c.valor <= 12) ? 0 : c.valor;
        }

        // Agrupar por palo
        var porPalo = cartas.GroupBy(c => c.palo).Where(g => g.Count() >= 2).ToList();

        if (porPalo.Count > 0)
        {
            int mejorPuntaje = 0;

            foreach (var grupo in porPalo)
            {
                var cartasDelPalo = grupo.ToList();
                if (cartasDelPalo.Count < 2) continue;

                var top2 = cartasDelPalo.OrderByDescending(c => ValorEnvido(c)).Take(2).ToList();
                int suma = ValorEnvido(top2[0]) + ValorEnvido(top2[1]) + 20;

                mejorPuntaje = Mathf.Max(mejorPuntaje, suma);
            }

            return mejorPuntaje;
        }

        // Si no hay cartas del mismo palo, usar la de mayor valor
        int maxSinPalo = cartas.Max(c => ValorEnvido(c));
        return maxSinPalo;
    }

    public void ShowEnvidoResults(int valor1, int valor2)
    {
        uiManager.ShowEnvidoPoints(valor1, valor2);
    }

    public void ResponderEnvido(bool quiero)
    {
        EnvidoRespondido = true;
        uiManager.OcultarOpcionesEnvido();

        int envidoJugador = CalcularPuntosEnvido(true);
        int envidoOponente = CalcularPuntosEnvido(false);

        if (quiero)
        {
            bool ganoJugador = envidoJugador > envidoOponente ||
                               (envidoJugador == envidoOponente && EnvidoFueDelJugador);

            this.ganoJugador = ganoJugador;

            ShowEnvidoResults(envidoJugador, envidoOponente);

            string nombreEnvido = TipoDeEnvidoActual switch
            {
                TipoEnvido.Envido => "Envido",
                TipoEnvido.RealEnvido => "Real Envido",
                TipoEnvido.FaltaEnvido => "Falta Envido",
                _ => "Envido"
            };

            ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} aceptó el {nombreEnvido}", TipoMensaje.Sistema);

            // Siempre sumamos los aceptados
            if (nombreEnvido == "Envido")
            {
                Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.EnvidosAceptados);
            }
            else if (nombreEnvido == "Real Envido")
            {
                Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RealEnvidosAceptados);
            }
            else if (nombreEnvido == "Falta Envido")
            {
                Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.FaltaEnvidosAceptados);
            }

            // Ahora también contamos si ganamos o perdimos el envido
            if (ganoJugador)
            {
                if (nombreEnvido == "Envido")
                {
                    Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.EnvidosGanados);
                }
                else if (nombreEnvido == "Real Envido")
                {
                    Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RealEnvidosGanados);
                }
                else if (nombreEnvido == "Falta Envido")
                {
                    Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.FaltaEnvidosGanados);
                }
            }
            else
            {
                if (nombreEnvido == "Envido")
                {
                    Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.EnvidosPerdidos);
                }
                else if (nombreEnvido == "Real Envido")
                {
                    Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RealEnvidosPerdidos);
                }
                else if (nombreEnvido == "Falta Envido")
                {
                    Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.FaltaEnvidosPerdidos);
                }
            }

            int puntosAGanar = 0;

            if (EnvidoCantos.Contains(TipoEnvido.FaltaEnvido))
            {
                puntosAGanar = PointsToEnd - Mathf.Max(puntosJugador, puntosOponente);
            }
            else
            {
                if (EnvidoCantos.Count == 1)
                {
                    if (EnvidoCantos[0] == TipoEnvido.Envido)
                        puntosAGanar = 2;
                    else if (EnvidoCantos[0] == TipoEnvido.RealEnvido)
                        puntosAGanar = 3;
                }
                else if (EnvidoCantos.Count == 2)
                {
                    if (EnvidoCantos.Contains(TipoEnvido.Envido) && EnvidoCantos.Contains(TipoEnvido.RealEnvido))
                        puntosAGanar = 5; // Envido + Real Envido
                    else if (EnvidoCantos[0] == TipoEnvido.Envido && EnvidoCantos[1] == TipoEnvido.Envido)
                        puntosAGanar = 4; // Envido + Envido
                }
                else if (EnvidoCantos.Count == 3)
                {
                    if (EnvidoCantos[0] == TipoEnvido.Envido &&
                        EnvidoCantos[1] == TipoEnvido.Envido &&
                        EnvidoCantos[2] == TipoEnvido.RealEnvido)
                    {
                        puntosAGanar = 7; // Envido + Envido + Real Envido
                    }
                }
            }

            if (ganoJugador)
            {
                SumarPuntos(true, puntosAGanar, false);
                ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} ganó el {nombreEnvido} (+{puntosAGanar})", TipoMensaje.Sistema);
            }
            else
            {
                SumarPuntos(false, puntosAGanar, false);
                ChatManager.Instance.AgregarMensaje($"{NombreJugador(false)} ganó el {nombreEnvido} (+{puntosAGanar})", TipoMensaje.Sistema);
            }

            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Quiero);
            uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        }


        else
        {
            int puntosPorNoQuerer = 1;

            string nombreEnvido = TipoDeEnvidoActual switch
            {
                TipoEnvido.Envido => "Envido",
                TipoEnvido.RealEnvido => "Real Envido",
                TipoEnvido.FaltaEnvido => "Falta Envido",
                _ => "Envido"
            };

            if (EnvidoCantos.Count == 2)
            {
                if (EnvidoCantos.Contains(TipoEnvido.Envido) && EnvidoCantos.Contains(TipoEnvido.RealEnvido))
                    puntosPorNoQuerer = 2;
                else if (EnvidoCantos[0] == TipoEnvido.Envido && EnvidoCantos[1] == TipoEnvido.Envido)
                    puntosPorNoQuerer = 2;
            }
            else if (EnvidoCantos.Count == 3)
            {
                if (EnvidoCantos[0] == TipoEnvido.Envido &&
                    EnvidoCantos[1] == TipoEnvido.Envido &&
                    EnvidoCantos[2] == TipoEnvido.RealEnvido)
                {
                    puntosPorNoQuerer = 3;
                }
            }

            if (EnvidoFueDelJugador)
            {
                SumarPuntos(true, puntosPorNoQuerer, false);
                ChatManager.Instance.AgregarMensaje($"{NombreJugador(true)} no quiso {nombreEnvido}", TipoMensaje.Sistema);
            }

            else
            {
                SumarPuntos(false, puntosPorNoQuerer, false);
                ChatManager.Instance.AgregarMensaje($"{NombreJugador(false)} no quiso {nombreEnvido}", TipoMensaje.Sistema);
            }

            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.NoQuiero);
            uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        }

        EnvidoCantado = false;

        if (estadoRonda != EstadoRonda.Finalizado)
        {
            estadoRonda = EstadoRonda.Jugando;
            uiManager.ActualizarBotonesSegunEstado();

            if (turnoActual == TurnoActual.Oponente && CantidadCartasOponenteJugadas < 3)
            {
                iaOponente.JugarCarta();
            }
        }
    }

    private AudioClip GetRandomDrop(List<AudioClip> list)
    {
        AudioClip randomClip = list[Random.Range(0, list.Count)];
        return randomClip;
    }

    public void PlaySFXCopy(AudioClip clip)
    {
        AudioManager.Instance.sfxSource.PlayOneShot(clip);
    }

    public string NombreJugador(bool esJugador)
    {
        return esJugador ? SteamUserManager.Instance.PlayerName : oponentName;
    }

    public bool JugadorYaCantoEsteTipo(TipoEnvido tipo)
    {
        return cantosPorJugador.TryGetValue(tipo, out bool fueDelJugador) && fueDelJugador == true;
    }

    private void ContarGanadoTruco()
    {
        if (TrucoState == 1)
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.TrucosGanados);
        else if (TrucoState == 2)
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RetrucosGanados);
        else if (TrucoState == 3)
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.ValeCuatroGanados);
    }

    private void ContarPerdidoTruco()
    {
        if (TrucoState == 1)
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.TrucosPerdidos);
        else if (TrucoState == 2)
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.RetrucosPerdidos);
        else if (TrucoState == 3)
            Utils.Estadisticas.Sumar(Utils.Estadisticas.Keys.ValeCuatroPerdidos);
    }
}