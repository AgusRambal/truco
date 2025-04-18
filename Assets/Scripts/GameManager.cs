using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EstadoRonda
{
    Repartiendo,
    Jugando,
    EsperandoRespuesta,
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

    [HideInInspector] public List<CardSelector> allCards = new List<CardSelector>();
    [HideInInspector] public int trucoState = 0;
     public int puntosEnJuego = 1;
    [HideInInspector] public bool seJugoCartaDesdeUltimoCanto = true;
    [HideInInspector] public bool ultimoCantoFueDelJugador = false;
    [HideInInspector] public bool ultimaManoFueEmpate = false;

    private List<CartaSO> mazo = new List<CartaSO>();
    private List<CardSelector> cartasEnMano = new List<CardSelector>();
    private List<Carta> cartasJugadorJugadas = new List<Carta>();
    private List<Carta> cartasOponenteJugadas = new List<Carta>();
    private float zOffsetCentroMesa = 0f;
    private bool rondaSeDefiniraEnProxima = false;

    public int manosGanadasJugador = 0;
    public int manosGanadasOponente = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        mazo = new List<CartaSO>(cartas);
        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        SpawnCards();
    }

    public void SpawnCards()
    {
        uiManager.SetBotonesInteractables(false);
        estadoRonda = EstadoRonda.Repartiendo;
        StartCoroutine(SpawnCardsSequence());
    }

    private IEnumerator SpawnCardsSequence()
    {
        mazo = new List<CartaSO>(cartas);
        cartasEnMano.Clear();
        int playerIndex = 0;
        int opponentIndex = 0;

        for (int i = 0; i < 6; i++)
        {
            bool isOpponentDraw = (i % 2 == 0);

            var instantiatedCard = Instantiate(carta, spawnPosition.position, Quaternion.Euler(0f, 180f, 0f));
            var cardSelector = instantiatedCard.GetComponent<CardSelector>();
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

                cartasEnMano.Add(cardSelector);
                playerIndex++;
            }

            allCards.Add(cardSelector);
            yield return s.WaitForCompletion();
        }

        estadoRonda = EstadoRonda.Jugando;
        turnoActual = TurnoActual.Jugador;
        uiManager.SetBotonesInteractables(true);
    }

    public void CartaJugada(CardSelector carta)
    {
        var cartaData = carta.GetComponent<Carta>();
        seJugoCartaDesdeUltimoCanto = true;

        if (carta.isOpponent)
        {
            cartasOponenteJugadas.Add(cartaData);
            turnoActual = TurnoActual.Jugador;
            uiManager.SetBotonesInteractables(true);
        }

        else
        {
            cartasJugadorJugadas.Add(cartaData);
            turnoActual = TurnoActual.Oponente;
            uiManager.SetBotonesInteractables(false);
            iaOponente.JugarCarta();
        }

        if (cartasJugadorJugadas.Count > 0 && cartasJugadorJugadas.Count == cartasOponenteJugadas.Count)
        {
            EvaluarMano(cartasJugadorJugadas[^1], cartasOponenteJugadas[^1]);
        }
    }

    private void EvaluarMano(Carta jug, Carta opo)
    {
        if (jug.jerarquiaTruco > opo.jerarquiaTruco)
        {
            manosGanadasJugador++;
            ultimaManoFueEmpate = false;
        }
        else if (opo.jerarquiaTruco > jug.jerarquiaTruco)
        {
            manosGanadasOponente++;
            ultimaManoFueEmpate = false;
        }
        else
        {
            Debug.Log("Empate en la mano");
            ultimaManoFueEmpate = true;

            // Si es la PRIMERA mano, la ronda se definirá en la siguiente
            if (cartasJugadorJugadas.Count == 1)
                rondaSeDefiniraEnProxima = true;
        }

        VerificarFinDeRonda();
    }

    public void VerificarFinDeRonda()
    {
        // Caso especial: la ronda se definía en la subronda 2 (por empate en subronda 1)
        if (rondaSeDefiniraEnProxima && cartasJugadorJugadas.Count == 2 && cartasOponenteJugadas.Count == 2)
        {
            if (manosGanadasJugador > manosGanadasOponente)
                puntosJugador += puntosEnJuego;
            else if (manosGanadasOponente > manosGanadasJugador)
                puntosOponente += puntosEnJuego;
            else
            {
                Debug.Log("Empate incluso en mano definitoria — gana el jugador por ser mano");
                puntosJugador += puntosEnJuego;
            }

            FinalizarRonda();
            return;
        }

        if (manosGanadasJugador == 2)
        {
            puntosJugador += puntosEnJuego;
            FinalizarRonda();
            return;
        }

        if (manosGanadasOponente == 2)
        {
            puntosOponente += puntosEnJuego;
            FinalizarRonda();
            return;
        }

        if (cartasJugadorJugadas.Count == 3 && cartasOponenteJugadas.Count == 3)
        {
            if (manosGanadasJugador > manosGanadasOponente)
                puntosJugador += puntosEnJuego;
            else if (manosGanadasOponente > manosGanadasJugador)
                puntosOponente += puntosEnJuego;
            else
            {
                Debug.Log("Empate triple — gana el jugador por ser mano");
                puntosJugador += puntosEnJuego;
            }

            FinalizarRonda();
        }
    }


    public void FinalizarRonda()
    {
        cartasJugadorJugadas.Clear();
        cartasOponenteJugadas.Clear();
        uiManager.ResetTruco();
        manosGanadasJugador = 0;
        manosGanadasOponente = 0;
        seJugoCartaDesdeUltimoCanto = true;
        ultimoCantoFueDelJugador = false;
        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
        ResetZOffset();
        DevolverCartas();
        puntosEnJuego = 1;
        trucoState = 0;
        estadoRonda = EstadoRonda.Repartiendo;
    }

    private void DevolverCartas()
    {
        uiManager.SetBotonesInteractables(false);
        StartCoroutine(DevolverCartasSequence());
    }

    private IEnumerator DevolverCartasSequence()
    {
        yield return new WaitForSeconds(devolverCartasWaitTime);

        foreach (var c in allCards)
        {
            Transform cartaTransform = c.transform;
            Sequence s = DOTween.Sequence();

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
        SpawnCards();
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
        uiManager.ChangeTrucoState(trucoState);
    }

    public void CantarTruco()
    {
        if (!seJugoCartaDesdeUltimoCanto && ultimoCantoFueDelJugador)
        {
            Debug.Log("No podés volver a cantar sin que se juegue una carta.");
            return;
        }

        trucoState++;
        estadoRonda = EstadoRonda.EsperandoRespuesta;
        Debug.Log("Se cantó Truco. Esperando respuesta...");
        uiManager.OcultarOpcionesTruco();
        iaOponente.ResponderTruco();

        seJugoCartaDesdeUltimoCanto = false;
        ultimoCantoFueDelJugador = true;
    }

    public void RespuestaJugadorTruco(bool quiero)
    {
        uiManager.OcultarOpcionesTruco();

        if (quiero)
        {
            trucoState++;
            puntosEnJuego++;
            estadoRonda = EstadoRonda.Jugando;
            ChangeTruco();
        }
        else
        {
            puntosOponente += puntosEnJuego;
            FinalizarRonda();
        }

        if (turnoActual == TurnoActual.Oponente)
        {
            iaOponente.JugarCarta();
        }
    }

    public void SumarPuntosJugador()
    {
        puntosJugador += puntosEnJuego;
        uiManager.SetPointsInScreen(puntosJugador, puntosOponente);
    }

    public void MeVoy(bool esJugador)
    {
        if (esJugador)
            puntosOponente += puntosEnJuego;
        else
            puntosJugador += puntosEnJuego;

        FinalizarRonda();
    }

    public void CantarEnvido()
    {
        Debug.Log("Envido no implementado todavía");
    }
}