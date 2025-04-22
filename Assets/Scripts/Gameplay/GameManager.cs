using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Utils;

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

    [Header("References")]
    public UIManager uiManager;
    public GameObject carta;
    [SerializeField] private IAOponente iaOponente;

    [Header("Transforms")]
    public Transform[] handPosition;
    public Transform[] handOponentPosition;
    public Transform target;
    public Transform spawnPosition;

    [Header("Parameters")]
    [SerializeField] private float setTime = 0.25f;
    [SerializeField] private float resetTime = 0.25f;
    [SerializeField] private float devolverCartasWaitTime = 0.1f;

    [Header("Game stats")]
    public EstadoRonda estadoRonda = EstadoRonda.Jugando;
    public TurnoActual turnoActual = TurnoActual.Jugador;
    public int puntosOponente = 0;
    public int puntosJugador = 0;
    public int puntosEnJuego = 1;

    [Header("Audio")]
    [SerializeField] private List<AudioClip> drops = new List<AudioClip>();
    [SerializeField] private List<AudioClip> plays = new List<AudioClip>();

    //Hidden
    [HideInInspector] public int TrucoState = 0;
    [HideInInspector] public bool SeJugoCartaDesdeUltimoCanto = true;
    [HideInInspector] public bool UltimoCantoFueDelJugador = false;
    [HideInInspector] public bool isPaused = false;
    [HideInInspector] public bool EnvidoCantado = false;
    [HideInInspector] public bool EnvidoFueDelJugador = false;
    [HideInInspector] public bool EnvidoRespondido = false;
    [HideInInspector] public bool ganoJugador = false;
    [HideInInspector] public TipoEnvido TipoDeEnvidoActual;

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

    //Propiedades
    public int CantidadCartasJugadorJugadas => cartasJugadorJugadas.Count;
    public int CantidadCartasOponenteJugadas => cartasOponenteJugadas.Count;
    public List<CardSelector> AllCards => allCards;
    public bool UltimaManoFueEmpate => ultimaManoFueEmpate;

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
        pointsToEnd = ParametrosDePartida.puntosParaGanar;
    }

    private void Start()
    {
        mazo = new List<CartaSO>(cartas);
        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        Invoke("SpawnCards", .5f);
    }

    public void SpawnCards()
    {
        uiManager.ActualizarBotonesSegunEstado();
        estadoRonda = EstadoRonda.Repartiendo;
        StartCoroutine(SpawnCardsSequence());
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
            // Todavía no se completó la subronda → activar próximo turno
            if (turnoActual == TurnoActual.Oponente)
            {
                turnoActual = TurnoActual.Jugador;
                uiManager.ActualizarBotonesSegunEstado();
            }
            else
            {
                turnoActual = TurnoActual.Oponente;
                uiManager.ActualizarBotonesSegunEstado();
                iaOponente.JugarCarta();
            }
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
            Debug.Log("Empate en la mano");
            ultimaManoFueEmpate = true;

            // Si es la PRIMERA mano, la ronda se definirá en la siguiente
            if (cartasJugadorJugadas.Count == 1)
                rondaSeDefiniraEnProxima = true;

            // En caso de empate: no cambiar turnoActual
        }

        VerificarFinDeRonda();
    }

    public void VerificarFinDeRonda()
    {
        // Caso especial: la ronda se definía en la subronda 2 (por empate en subronda 1)
        if (rondaSeDefiniraEnProxima && cartasJugadorJugadas.Count == 2 && cartasOponenteJugadas.Count == 2)
        {
            if (manosGanadasJugador > manosGanadasOponente)
                SumarPuntos(true);
            else if (manosGanadasOponente > manosGanadasJugador)
                SumarPuntos(false);
            else
            {
                Debug.Log("Empate incluso en mano definitoria — gana el jugador por ser mano");
                SumarPuntos(true);
            }

            return;
        }

        if (manosGanadasJugador == 2)
        {
            SumarPuntos(true);
            return;
        }

        if (manosGanadasOponente == 2)
        {
            SumarPuntos(false);
            return;
        }

        if (cartasJugadorJugadas.Count == 3 && cartasOponenteJugadas.Count == 3)
        {
            if (manosGanadasJugador > manosGanadasOponente)
                SumarPuntos(true);
            else if (manosGanadasOponente > manosGanadasJugador)
                SumarPuntos(false);
            else
            {
                Debug.Log("Empate triple — gana el jugador por ser mano");
                SumarPuntos(true);
            }
        }

        if (estadoRonda == EstadoRonda.Jugando)
        {
            if (turnoActual == TurnoActual.Oponente)
            {
                uiManager.ActualizarBotonesSegunEstado();
                iaOponente.JugarCarta();
            }
            else
            {
                uiManager.ActualizarBotonesSegunEstado();
            }
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
                ganancia = Random.Range(1, 31); //Por ahora valor random
                int creditosActuales = PlayerPrefs.GetInt("Creditos", 0);
                int total = creditosActuales + ganancia;

                PlayerPrefs.SetInt("Creditos", total);
                PlayerPrefs.Save();

                Debug.Log($"Ganaste {ganancia} créditos. Total acumulado: {total}");
            }

            uiManager.MostrarResultadoFinal(ganoJugador, ganancia); 
            
            return;
        }
    }

    public void FinalizarRonda()
    {
        cartasJugadorJugadas.Clear();
        cartasOponenteJugadas.Clear();
        manosGanadasJugador = 0;
        manosGanadasOponente = 0;
        ultimaManoFueEmpate = false;
        rondaSeDefiniraEnProxima = false;
        EnvidoCantado = false;

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
        if (!SeJugoCartaDesdeUltimoCanto && UltimoCantoFueDelJugador)
        {
            Debug.Log("No podés volver a cantar sin que se juegue una carta.");
            return;
        }

        if (TrucoState == 0)
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Truco);
        else if (TrucoState == 1)
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Retruco);
        else if (TrucoState == 2)
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.ValeCuatro);

        TrucoState++;
        estadoRonda = EstadoRonda.EsperandoRespuesta;
        Debug.Log("Se cantó Truco. Esperando respuesta...");
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
            TrucoState++;
            puntosEnJuego++;
            estadoRonda = EstadoRonda.Jugando;
            ChangeTruco();
        }
        else
        {
            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.NoQuiero);
            SumarPuntos(false);
        }

        if (turnoActual == TurnoActual.Oponente)
        {
            iaOponente.JugarCarta();
        }
    }

    public void SumarPuntos(bool isPlayer)
    {
        if (isPlayer) puntosJugador += puntosEnJuego;
        else puntosOponente += puntosEnJuego;

        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        FinalizarRonda();
        FinalCheck();
    }

    public void MeVoy(bool esJugador)
    {
        if (esJugador)
            SumarPuntos(false);
        else
            SumarPuntos(true);

        uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.MeVoy);
    }

    public void CantarEnvido(TipoEnvido tipo)
    {
        if (EnvidoCantado || estadoRonda != EstadoRonda.Jugando)
            return;

        if (tipo == TipoEnvido.FaltaEnvido && turnoActual != TurnoActual.Jugador)
            return; // Solo el jugador puede cantar Falta Envido

        EnvidoCantado = true;
        TipoDeEnvidoActual = tipo;

        if (turnoActual == TurnoActual.Jugador && cartasJugadorJugadas.Count == 0)
        {
            EnvidoFueDelJugador = true;

            uiManager.MostrarTrucoMensaje(true, tipo switch
            {
                TipoEnvido.Envido => UIManager.TrucoMensajeTipo.Envido,
                TipoEnvido.RealEnvido => UIManager.TrucoMensajeTipo.RealEnvido,
                TipoEnvido.FaltaEnvido => UIManager.TrucoMensajeTipo.FaltaEnvido,
                _ => UIManager.TrucoMensajeTipo.Envido
            });

            StartCoroutine(iaOponente.ResponderEnvidoCoroutine());
        }
        else if (turnoActual == TurnoActual.Oponente && cartasOponenteJugadas.Count == 0)
        {
            EnvidoFueDelJugador = false;

            uiManager.MostrarTrucoMensaje(false, tipo switch
            {
                TipoEnvido.Envido => UIManager.TrucoMensajeTipo.Envido,
                TipoEnvido.RealEnvido => UIManager.TrucoMensajeTipo.RealEnvido,
                _ => UIManager.TrucoMensajeTipo.Envido
            });

            uiManager.MostrarOpcionesEnvido();
        }
    }

    public void CantarEnvidoNormal()
    {
        CantarEnvido(TipoEnvido.Envido);
    }

    public void CantarRealEnvido()
    {
        CantarEnvido(TipoEnvido.RealEnvido);
    }

    public void CantarFaltaEnvido()
    {
        CantarEnvido(TipoEnvido.FaltaEnvido);
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

        // DEBUG: mostrar cartas y sus valores
        Debug.Log($"=== Cartas del {(esJugador ? "jugador" : "oponente")} ===");
        foreach (var c in cartas)
        {
            Debug.Log($"Carta: {c.valor} de {c.palo} → Envido: {ValorEnvido(c)}"); 
        }

        // Agrupar por palo
        var porPalo = cartas.GroupBy(c => c.palo)
                            .Where(g => g.Count() >= 2)
                            .ToList();

        if (porPalo.Count > 0)
        {
            int mejorPuntaje = 0;

            foreach (var grupo in porPalo)
            {
                var cartasDelPalo = grupo.ToList();
                if (cartasDelPalo.Count < 2) continue;

                var top2 = cartasDelPalo.OrderByDescending(c => ValorEnvido(c)).Take(2).ToList();
                int suma = ValorEnvido(top2[0]) + ValorEnvido(top2[1]) + 20;

                Debug.Log($"→ Palo {grupo.Key}: {ValorEnvido(top2[0])} + {ValorEnvido(top2[1])} + 20 = {suma}");
                mejorPuntaje = Mathf.Max(mejorPuntaje, suma);
            }

            Debug.Log($"Resultado final de Envido: {mejorPuntaje}");
            return mejorPuntaje;
        }

        // Si no hay cartas del mismo palo, usar la de mayor valor
        int maxSinPalo = cartas.Max(c => ValorEnvido(c));
        Debug.Log($"No hay palo repetido. Mayor carta: {maxSinPalo}");
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
            ShowEnvidoResults(envidoJugador, envidoOponente);

            bool ganoJugador = envidoJugador > envidoOponente ||
                               (envidoJugador == envidoOponente && EnvidoFueDelJugador);

            this.ganoJugador = ganoJugador;

            switch (TipoDeEnvidoActual)
            {
                case TipoEnvido.Envido:
                    if (ganoJugador)
                    {
                        puntosJugador += 2;
                        Debug.Log("Resultado Envido: Gana el jugador (+2 puntos)");
                    }
                    else
                    {
                        puntosOponente += 2;
                        Debug.Log("Resultado Envido: Gana el oponente (+2 puntos)");
                    }
                    break;

                case TipoEnvido.RealEnvido:
                    if (ganoJugador)
                    {
                        puntosJugador += 3;
                        Debug.Log("Resultado Real Envido: Gana el jugador (+3 puntos)");
                    }
                    else
                    {
                        puntosOponente += 3;
                        Debug.Log("Resultado Real Envido: Gana el oponente (+3 puntos)");
                    }
                    break;
            }

            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.Quiero);
        }
        else
        {
            if (EnvidoFueDelJugador)
            {
                puntosJugador += 1;
                Debug.Log("Resultado: No quiso. +1 punto para el jugador");
            }
            else
            {
                puntosOponente += 1;
                Debug.Log("Resultado: No quiso. +1 punto para el oponente");
            }

            uiManager.MostrarTrucoMensaje(true, UIManager.TrucoMensajeTipo.NoQuiero);
            uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        }

        if (turnoActual == TurnoActual.Oponente)
        {
            estadoRonda = EstadoRonda.Jugando;
            uiManager.ActualizarBotonesSegunEstado();
            iaOponente.JugarCarta();
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
}