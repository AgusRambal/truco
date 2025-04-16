using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scriptable obejcts")]
    public List<CartaSO> cartas = new List<CartaSO>();

    [Header("References")]
    public GameObject carta;

    [Header("Transforms")]
    public Transform[] handPosition;
    public Transform target;
    public Transform spawnPosition;

    [Header("Stats")]
    public int round = 0;

    //Privates
    private List<CartaSO> mazo = new List<CartaSO>();
    private List<CardSelector> cartasEnMano = new List<CardSelector>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        round++;
        mazo = new List<CartaSO>(cartas);
        SpawnCards();
    }

    public void SpawnCards()
    {
        StartCoroutine(SpawnCardsSequence());
    }

    private IEnumerator SpawnCardsSequence()
    {
        mazo = new List<CartaSO>(cartas);

        for (int i = 0; i < 3; i++)
        {
            // Instanciar en spawnPosition con rotación en Y de 180°
            var instantiatedCard = Instantiate(carta, spawnPosition.position, Quaternion.Euler(0f, 180f, 0f));
            var cardSelector = instantiatedCard.GetComponent<CardSelector>();
            cartasEnMano.Add(cardSelector);

            // Elegir carta al azar
            var randomCard = mazo[Random.Range(0, mazo.Count)];
            mazo.Remove(randomCard);

            // Asignar valores
            var newCard = instantiatedCard.GetComponent<Carta>();
            newCard.palo = randomCard.palo;
            newCard.valor = randomCard.valor;
            newCard.jerarquiaTruco = randomCard.jerarquiaTruco;
            newCard.imagen.sprite = randomCard.imagen;

            // Animación de movimiento y rotación en 0.5s
            Sequence s = DOTween.Sequence();
            s.Append(instantiatedCard.transform.DOMove(handPosition[i].position, 0.5f).SetEase(Ease.OutCubic));
            s.Join(instantiatedCard.transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f).SetEase(Ease.OutCubic));

            yield return s.WaitForCompletion(); // esperar a que termine antes de pasar a la siguiente
        }
    }
}
