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

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scriptable obejcts")]
    public List<CartaSO> cartas = new List<CartaSO>();

    [Header("References")]
    public GameObject carta;

    [Header("Transforms")]
    public Transform[] handPosition;
    public Transform[] handOponentPosition;
    public Transform target;
    public Transform spawnPosition;

    [Header("Parameters")]
    [SerializeField] private float setTime = 0.25f;
    [SerializeField] private float resetTime = 0.25f;

    [Header("Game stats")]
    public EstadoRonda estadoRonda = EstadoRonda.Jugando;
    public int round = 0;
    public int puntosOponente = 0;
    public int puntosJugador = 0;


    //Privates
    public List<CartaSO> mazo = new List<CartaSO>();
    public List<CardSelector> cartasEnMano = new List<CardSelector>();
    public List<CardSelector> allCards = new List<CardSelector>();
    private int puntosEnJuego = 1;

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
        round++;
        mazo = new List<CartaSO>(cartas);
        SpawnCards();
    }

    public void SpawnCards()
    {
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
                instantiatedCard.GetComponent<CardSelector>().isOpponent = true;
            }

            Sequence s = DOTween.Sequence();

            if (isOpponentDraw)
            {
                // Cartas del oponente → no rotan
                s.Append(instantiatedCard.transform.DOMove(handOponentPosition[opponentIndex].position, setTime).SetEase(Ease.OutCubic));
                s.Join(instantiatedCard.transform.DORotate(new Vector3(-10f, 180f, 0f), setTime).SetEase(Ease.OutCubic));

                instantiatedCard.transform.GetChild(0).localRotation = Quaternion.Euler(0f, 180f, 0f);
                instantiatedCard.transform.GetChild(0).localPosition = new Vector3(0f, 0f, -0.1f);

                opponentIndex++;
            }
            else
            {
                // Cartas del jugador → rotan a 360
                s.Append(instantiatedCard.transform.DOMove(handPosition[playerIndex].position, setTime).SetEase(Ease.OutCubic));
                s.Join(instantiatedCard.transform.DORotate(new Vector3(-10f, 360f, 0f), setTime).SetEase(Ease.OutCubic));

                cartasEnMano.Add(cardSelector);
                playerIndex++;
            }

            allCards.Add(cardSelector);

            yield return s.WaitForCompletion(); // Esperar a que termine antes de repartir la siguiente
        }

        estadoRonda = EstadoRonda.Jugando;
    }

    public void CantarTruco()
    {
        puntosEnJuego += 1;
        estadoRonda = EstadoRonda.EsperandoRespuesta;

        Debug.Log("Se cantó Truco. Esperando respuesta...");
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

    private void FinalizarRonda()
    {
        puntosOponente += puntosEnJuego;
        puntosEnJuego = 1;
        estadoRonda = EstadoRonda.Repartiendo;

        DevolverCartas();
    }

    private void DevolverCartas()
    {
        StartCoroutine(DevolverCartasSequence());
    }

    private IEnumerator DevolverCartasSequence()
    {
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

        // Destruir todas las cartas después del loop
        foreach (var c in allCards)
        {
            Destroy(c.gameObject);
        }

        cartasEnMano.Clear();
        allCards.Clear();

        // Arrancar siguiente ronda
        yield return new WaitForSeconds(0.5f);
        SpawnCards();
    }
}
