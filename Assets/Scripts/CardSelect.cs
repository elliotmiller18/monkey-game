using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class CardSelect : MonoBehaviour
{

    public static CardSelect instance;
    Queue<PlayCard> cardsToPlay;
    Button button;
    // what if i have a queue of card UI Elements

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("duplicate CardsToPlay, destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
        cardsToPlay = new Queue<PlayCard>();
        button = GetComponent<Button>();
    }

    void Update()
    {
        button.enabled = cardsToPlay.Count > 0;
    }

    public void AddCard(PlayCard cardUIElt)
    {
        Assert.IsFalse(cardsToPlay.Contains(cardUIElt), "Trying to add a card to the stack that's already in it");
        cardsToPlay.Enqueue(cardUIElt);
        if(cardsToPlay.Count > 4)
        {
            // this will remove it from the queue
            cardsToPlay.Peek().Deselect();
        }
    }

    public void RemoveCard(PlayCard cardUIElt)
    {
        Assert.IsTrue(cardsToPlay.Contains(cardUIElt), "Trying to remove a card from the stack that's nnot in it");
        cardsToPlay = new Queue<PlayCard>(cardsToPlay.Where(item => item != cardUIElt));
    }

    public void PlayPile()
    {
        if (!BSGameLogic.instance.IsHumanTurn()) return;
        if (cardsToPlay.Count == 0) return;

        List<Card> cardList = new List<Card>();
        while (cardsToPlay.Count > 0)
        {
            cardList.Add(cardsToPlay.Peek().GetCard());
            cardsToPlay.Dequeue();
        }

        BSGameLogic.instance.PlayCards(cardList);
        cardsToPlay.Clear();
    }
}
