enum Suit
{
    Hearts,
    Diamonds,
    Spades,
    Clubs
}

public struct Card
{
    Suit suit;
    int number;

    Card(Suit s, int n)
    {
        suit = s;
        number = n;
    }
}