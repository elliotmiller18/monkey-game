using System.Collections.Generic;
using UnityEngine;

public enum Suit
{
    Hearts,
    Diamonds,
    Spades,
    Clubs
}

public enum CardRank
{
    Ace = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13
}

[System.Serializable]
public class Card
{
    public Suit suit;
    public CardRank rank;

    public Card(Suit s, CardRank r)
    {
        suit = s;
        rank = r;
    }

    public override string ToString()
    {
        return $"{rank} of {suit}";
    }

    public override bool Equals(object obj)
    {
        if (obj is Card other)
        {
            return suit == other.suit && rank == other.rank;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (int)suit * 13 + (int)rank;
    }
}

public static class CardUtils
{
    public static List<Card> CreateDeck()
    {
        List<Card> deck = new List<Card>();
        foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
        {
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                deck.Add(new Card(suit, rank));
            }
        }
        return deck;
    }

    public static void ShuffleDeck(List<Card> deck)
    {
        for (int i = 0; i < deck.Count; i++)
        {
            Card temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }
}