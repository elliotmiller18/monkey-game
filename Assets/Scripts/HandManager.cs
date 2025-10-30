using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    Dictionary<(CardRank rank, Suit suit), GameObject> cardToUIElt;
    [SerializeField] GameObject CardUIElement;
    float cardWidth;

    public static HandManager instance;

    void Awake()
    {
        cardWidth = CardUIElement.GetComponent<RectTransform>().rect.width;
        if (instance != null && instance != this)
        {
            Debug.LogError("Error: duplicate handmanager, destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
        cardToUIElt = new Dictionary<(CardRank, Suit), GameObject>();
    }

    public void RenderCards(List<Card> cards)
    {
        foreach (GameObject elt in cardToUIElt.Values)
        {
            Destroy(elt);
        }
        cardToUIElt.Clear();
        
        HashSet<(CardRank rank, Suit suit)> distinctCards = new HashSet<(CardRank, Suit)>();
        foreach (Card c in cards)
        {
            distinctCards.Add((c.rank, c.suit));
        }

        float startingXPos = cardWidth;
        startingXPos -= cardWidth * distinctCards.Count / 2;
        
        foreach(Card c in cards)
        {
            if (!cardToUIElt.ContainsKey((c.rank, c.suit)))
            {
                // instantiate card as a child and then update its x pos
                GameObject card = Instantiate(CardUIElement, transform);
                Vector3 newPos = card.transform.localPosition;
                newPos.x = startingXPos;
                startingXPos += cardWidth;
                card.transform.localPosition = newPos;

                //TODO: put this cardSprites list in one central location
                card.GetComponent<Image>().sprite = TurnIndicator.instance.cardSprites[CardUtils.SuitedCardToIndex(c)];
                cardToUIElt[(c.rank, c.suit)] = card;
            }
            // it's on the hover zone
            cardToUIElt[(c.rank, c.suit)].GetComponentInChildren<PlayCard>().AddCard(c);
        }
    }
}
