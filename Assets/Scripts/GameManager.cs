using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform target;

    public List<Sprite> allCards = new List<Sprite>();
    public List<Image> playerHand = new List<Image>();

    public int cardIdSelected = -1;

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
        Set();
    }

    //Button
    public void Set()
    {
        foreach (var card in playerHand)
        {
            card.sprite = allCards[Random.Range(0, allCards.Count)];
        }
    }
}
