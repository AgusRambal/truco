using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public List<Sprite> allCards = new List<Sprite>();
    public List<Image> playerHand = new List<Image>();

    //Button
    public void Set()
    {
        foreach (var card in playerHand)
        {
            card.sprite = allCards[Random.Range(0, allCards.Count)];
        }
    }
}
