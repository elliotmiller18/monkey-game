using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    Dictionary<CardRank, GameObject> rankToUIElt;
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
        rankToUIElt = new Dictionary<CardRank, GameObject>();
    }

    public void RenderCards(List<Card> cards)
    {
        foreach (GameObject elt in rankToUIElt.Values)
        {
            Destroy(elt);
        }
        rankToUIElt.Clear();
        
        HashSet<CardRank> distinctRanks = new HashSet<CardRank>();
        foreach (Card c in cards)
        {
            distinctRanks.Add(c.rank);
        }

        float startingXPos = cardWidth;
        startingXPos -= cardWidth * distinctRanks.Count / 2;
        
        foreach(Card c in cards)
        {
            if (!rankToUIElt.ContainsKey(c.rank))
            {
                // instantiate card as a child and then update its x pos
                GameObject card = Instantiate(CardUIElement, transform);
                Vector3 newPos = card.transform.localPosition;
                newPos.x = startingXPos;
                startingXPos += cardWidth;
                card.transform.localPosition = newPos;

                //TODO: put this cardSprites list in one central location
                card.GetComponent<Image>().sprite = TurnIndicator.instance.cardSprites[CardUtils.CardToIndex(c)];
                rankToUIElt[c.rank] = card;
            }
            // it's on the hover zone
            rankToUIElt[c.rank].GetComponentInChildren<PlayCard>().AddCard(c);
        }
    }
}
