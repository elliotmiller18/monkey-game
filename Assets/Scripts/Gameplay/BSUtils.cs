using System.Collections.Generic;
using UnityEngine;

public enum Suit
{
    Clubs,
    Spades,
    Hearts,
    Diamonds
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
    public const int NUM_RANKS = 13;
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

    public static int CardToIndex(Card c)
    {
        return (int)c.rank - 1;
    }

    public static int CardToIndex(CardRank r)
    {
        return (int)r - 1;
    }

    public static CardRank NextRank(CardRank r)
    {
        return r == CardRank.King ? CardRank.Ace : ++r;
    }

    public static int SuitedCardToIndex(Card c)
    {
        return (int)c.suit * 13 + (int)c.rank - 1;
    }

    public static string RankToString(CardRank r)
    {
        switch (r)
        {
            case CardRank.Ace: return "A";
            case CardRank.Two: return "2";
            case CardRank.Three: return "3";
            case CardRank.Four: return "4";
            case CardRank.Five: return "5";
            case CardRank.Six: return "6";
            case CardRank.Seven: return "7";
            case CardRank.Eight: return "8";
            case CardRank.Nine: return "9";
            case CardRank.Ten: return "10";
            case CardRank.Jack: return "J";
            case CardRank.Queen: return "Q";
            case CardRank.King: return "K";
            default: return r.ToString();
        }
    }
    
    public static CardRank StringToRank(string cardStr)
    {
        switch(cardStr)
        {
            case "A": return CardRank.Ace;
            case "2": return CardRank.Two;
            case "3": return CardRank.Three;
            case "4": return CardRank.Four;
            case "5": return CardRank.Five;
            case "6": return CardRank.Six;
            case "7": return CardRank.Seven;
            case "8": return CardRank.Eight;
            case "9": return CardRank.Nine;
            case "10": return CardRank.Ten;
            case "J": return CardRank.Jack;
            case "Q": return CardRank.Queen;
            case "K": return CardRank.King;
            default: return CardRank.Ace;
        }
    }
}