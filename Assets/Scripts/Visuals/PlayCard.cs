using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayCard : MonoBehaviour
{
    private List<Card> cards;
    bool selected;

    public Card GetCard()
    {
        return cards[0];
    }

    public void Awake()
    {
        cards = new List<Card>();
        selected = false;
    }

    public void AddCard(Card c)
    {
        cards.Add(c);
    }

    public void OnCardClicked()
    {
        selected = !selected;

        if (!selected)
        {
            Deselect();
            return;
        }

        GetComponent<CardHoverEffect>().ToggleFrozen();
        CardSelect.instance.AddCard(this);
    }
    
    public void Deselect()
    {
        selected = false;
        CardSelect.instance.RemoveCard(this);
        GetComponent<CardHoverEffect>().ToggleFrozen();
    }
}
