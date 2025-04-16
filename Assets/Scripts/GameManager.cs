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
    public Transform target;

    //Hidden
    [HideInInspector] public int cardIdSelected = -1;

    //Privates
    private List<CartaSO> mazo = new List<CartaSO>();


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
        mazo = new List<CartaSO>(cartas);
    }

    public void SpawnCards()
    { 
        for (int i = 0; i < 3; i++) 
        {
            var InstantiatedCard = Instantiate(carta, target);

            var randomCard = mazo[Random.Range(0, mazo.Count)];
            mazo.Remove(randomCard);

            var newCard = InstantiatedCard.GetComponent<Carta>();
            newCard.palo = randomCard.palo;
            newCard.valor = randomCard.valor;
            newCard.jerarquiaTruco = randomCard.jerarquiaTruco;
            newCard.imagen = randomCard.imagen;
        }
    }
}
