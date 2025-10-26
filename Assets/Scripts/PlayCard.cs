using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayCard : MonoBehaviour
{
    private List<Card> cards;
    public void Awake()
    {
        cards = new List<Card>();
    }

    public void AddCard(Card c)
    {
        cards.Add(c);
        GetComponentInChildren<TMP_Text>().text = "X" + cards.Count;
    }

    public void OnCardClicked()
    {
        if (BSGameLogic.instance != null && BSGameLogic.instance.IsHumanTurn())
        {
            // good practice to ensure cards list is valid
            if (cards != null && cards.Count > 0)
            {
                BSGameLogic.instance.PlayCards(cards);
            }
        }
    }
}
