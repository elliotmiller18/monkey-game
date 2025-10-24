using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player
{
    public string name;
    public List<Card> hand;
    public bool isHuman;
    public bool isActive;

    public Player(string playerName, bool human = false)
    {
        name = playerName;
        hand = new List<Card>();
        isHuman = human;
        isActive = true;
    }

    public void AddCard(Card card)
    {
        hand.Add(card);
    }

    public void RemoveCards(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            hand.Remove(card);
        }
    }

    public bool HasCard(CardRank rank)
    {
        foreach (Card card in hand)
        {
            if (card.rank == rank)
                return true;
        }
        return false;
    }

    public List<Card> GetCardsOfRank(CardRank rank)
    {
        List<Card> cards = new List<Card>();
        foreach (Card card in hand)
        {
            if (card.rank == rank)
                cards.Add(card);
        }
        return cards;
    }

    public bool IsOut()
    {
        return hand.Count == 0;
    }
}

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int numberOfAIPlayers = 3;
    
    [Header("Game State")]
    public List<Player> players;
    public List<Card> deck;
    public List<Card> discardPile;
    public int currentPlayerIndex;
    public CardRank currentClaim;
    public int cardsToPlay;
    public bool gameInProgress;
    public bool waitingForPlayerAction;
    
    // Track the last played cards to check for BS
    public List<Card> lastPlayedCards;
    public CardRank lastClaimedRank;
    public int lastPlayerIndex; // Track who actually played the last cards

    // Events for UI
    public System.Action<string> OnGameMessage;
    public System.Action<List<Card>> OnPlayerHandUpdated;
    public System.Action<int> OnPlayerTurnChanged;
    public System.Action OnGameEnded;

    public static GameManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("extra gamemanger in " + gameObject.name + ", destroying it");
            Destroy(gameObject);
        }
        instance = this;
    }

    void Start()
    {
        currentPlayerIndex = 0;
        InitializeGame();
        Assertions();
    }

    public void InitializeGame()
    {
        players = new List<Player>();
        deck = CardUtils.CreateDeck();
        CardUtils.ShuffleDeck(deck);
        discardPile = new List<Card>();

        // Create human player
        players.Add(new Player("You", true));

        // Create AI players
        for (int i = 1; i <= numberOfAIPlayers; i++)
        {
            players.Add(new Player($"AI Player {i}", false));
        }

        // Deal cards
        DealCards();

        currentPlayerIndex = 0;
        currentClaim = CardRank.Ace;
        cardsToPlay = 1;
        gameInProgress = true;
        waitingForPlayerAction = false;

        OnGameMessage?.Invoke($"Game started! {players.Count} players. It's {players[currentPlayerIndex].name}'s turn.");
        OnPlayerHandUpdated?.Invoke(players[0].hand); // Update human player's hand
    }
    
    void Assertions()
    {
        Assert.AreEqual(numberOfAIPlayers + 1, players.Count, "ai players + 1 and size of players count variable do not match");
    }

    void DealCards()
    {
        int cardsPerPlayer = deck.Count / players.Count;
        int extraCards = deck.Count % players.Count;
        
        int cardIndex = 0;
        for (int i = 0; i < players.Count; i++)
        {
            int cardsToDeal = cardsPerPlayer + (i < extraCards ? 1 : 0);
            for (int j = 0; j < cardsToDeal; j++)
            {
                players[i].AddCard(deck[cardIndex]);
                cardIndex++;
            }
        }
    }

    void Update()
    {
        if (gameInProgress && !waitingForPlayerAction)
        {
            if (players[currentPlayerIndex].isHuman)
            {
                waitingForPlayerAction = true;
                OnPlayerTurnChanged?.Invoke(currentPlayerIndex);
            }
            // AI turns are now handled manually after player actions
        }
    }

    // Human player actions
    public void PlayCards(List<Card> cards, CardRank claimedRank)
    {
        if (!gameInProgress || !waitingForPlayerAction || currentPlayerIndex != 0)
            return;

        // Validate that the player is claiming the correct rank for this turn
        if (claimedRank != currentClaim)
        {
            OnGameMessage?.Invoke($"You must play {currentClaim}(s) this turn, not {claimedRank}s!");
            return;
        }

        if (cards.Count < 1)
        {
            OnGameMessage?.Invoke($"You must play at least 1 card.");
            return;
        }

        // Track what was actually played
        lastPlayedCards = new List<Card>(cards);
        lastClaimedRank = claimedRank;
        lastPlayerIndex = currentPlayerIndex; // Track who played these cards
        
        // Remove cards from player's hand
        players[0].RemoveCards(cards);
        
        // Add to discard pile
        discardPile.AddRange(cards);
        
        OnGameMessage?.Invoke($"You played {cards.Count} card(s) claiming {claimedRank}.");
        
        // Advance to next rank after player plays cards
        AdvanceToNextRank();
        
        // After player plays, immediately trigger next AI player
        NextTurn();
        if (gameInProgress && !players[currentPlayerIndex].isHuman)
        {
            PlayAITurn();
        }
    }

    public void CallBS()
    {
        if (!gameInProgress)
            return;

        // Get the player who actually just played (not calculated from current turn)
        Player previousPlayer = players[lastPlayerIndex];
        
        // Check if the previous player was lying by comparing actual played cards to claimed rank
        bool wasLying = false;
        if (lastPlayedCards != null && lastPlayedCards.Count > 0)
        {
            // Check if any of the actually played cards match the claimed rank
            bool hasClaimedRank = false;
            foreach (Card card in lastPlayedCards)
            {
                if (card.rank == lastClaimedRank)
                {
                    hasClaimedRank = true;
                    break;
                }
            }
            wasLying = !hasClaimedRank;
        }
        
        if (wasLying)
        {
            // Previous player was lying - they pick up the discard pile
            previousPlayer.hand.AddRange(discardPile);
            discardPile.Clear();
            OnGameMessage?.Invoke($"BS called! {previousPlayer.name} was lying and picks up the pile!");
        }
        else
        {
            // Previous player was telling the truth - caller (you) picks up the pile
            players[0].hand.AddRange(discardPile);
            discardPile.Clear();
            OnGameMessage?.Invoke($"BS called! You were wrong and pick up the pile!");
        }
        
        OnPlayerHandUpdated?.Invoke(players[0].hand);
        
        // Clear the pile and continue to next player in normal turn order
        waitingForPlayerAction = false;
        NextTurn();
        if (gameInProgress && !players[currentPlayerIndex].isHuman)
        {
            PlayAITurn();
        }
    }

    public void ContinueWithoutBS()
    {
        if (!gameInProgress || !waitingForPlayerAction)
            return;

        OnGameMessage?.Invoke("You chose not to call BS. Game continues.");
        
        waitingForPlayerAction = false;
        NextTurn();
        if (gameInProgress && !players[currentPlayerIndex].isHuman)
        {
            PlayAITurn();
        }
    }



    void PlayAITurn()
    {
        Player aiPlayer = players[currentPlayerIndex];
        
        // AI logic - must play the current required rank
        if (aiPlayer.HasCard(currentClaim))
        {
            // AI has the required card - play it
            List<Card> cardsToPlay = aiPlayer.GetCardsOfRank(currentClaim);
            // AI can play 1 or more cards of the required rank
            int cardsToPlayCount = Random.Range(1, Mathf.Min(cardsToPlay.Count + 1, 4)); // Play 1-3 cards
            List<Card> actualCards = cardsToPlay.GetRange(0, Mathf.Min(cardsToPlayCount, cardsToPlay.Count));
            
            // Track what was actually played
            lastPlayedCards = new List<Card>(actualCards);
            lastClaimedRank = currentClaim;
            lastPlayerIndex = currentPlayerIndex; // Track who played these cards
            
            aiPlayer.RemoveCards(actualCards);
            discardPile.AddRange(actualCards);
            
            OnGameMessage?.Invoke($"{aiPlayer.name} played {actualCards.Count} {currentClaim}(s).");
        }
        else
        {
            // AI doesn't have the required card - bluff
            List<Card> bluffCards = new List<Card>();
            int cardsToBluff = Random.Range(1, Mathf.Min(aiPlayer.hand.Count + 1, 3)); // Bluff 1-2 cards
            
            for (int i = 0; i < cardsToBluff && aiPlayer.hand.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, aiPlayer.hand.Count);
                bluffCards.Add(aiPlayer.hand[randomIndex]);
                aiPlayer.hand.RemoveAt(randomIndex);
            }
            
            // Track what was actually played (bluff)
            lastPlayedCards = new List<Card>(bluffCards);
            lastClaimedRank = currentClaim;
            lastPlayerIndex = currentPlayerIndex; // Track who played these cards
            
            discardPile.AddRange(bluffCards);
            OnGameMessage?.Invoke($"{aiPlayer.name} played {bluffCards.Count} card(s) claiming {currentClaim} (bluffing).");
        }
        
        // Advance to next rank after AI plays cards
        AdvanceToNextRank();
        
        // After AI plays, give player choice to call BS or continue
        waitingForPlayerAction = true;
        OnGameMessage?.Invoke($"Do you think {aiPlayer.name} was lying? Press B to call BS, or SPACE to continue.");
    }

    void NextTurn()
    {
        // Check for game end
        foreach (Player player in players)
        {
            if (player.IsOut())
            {
                EndGame(player);
                return;
            }
        }
        
        // Move to next player (don't advance rank here - only when cards are played)
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        waitingForPlayerAction = players[currentPlayerIndex].isHuman;
        
        if (waitingForPlayerAction)
        {
            OnPlayerTurnChanged?.Invoke(currentPlayerIndex);
        }
    }

    void AdvanceToNextRank()
    {
        // Advance through the sequence: Ace -> 2 -> 3 -> 4 -> 5 -> 6 -> 7 -> 8 -> 9 -> 10 -> Jack -> Queen -> King -> Ace
        switch (currentClaim)
        {
            case CardRank.Ace:
                currentClaim = CardRank.Two;
                break;
            case CardRank.Two:
                currentClaim = CardRank.Three;
                break;
            case CardRank.Three:
                currentClaim = CardRank.Four;
                break;
            case CardRank.Four:
                currentClaim = CardRank.Five;
                break;
            case CardRank.Five:
                currentClaim = CardRank.Six;
                break;
            case CardRank.Six:
                currentClaim = CardRank.Seven;
                break;
            case CardRank.Seven:
                currentClaim = CardRank.Eight;
                break;
            case CardRank.Eight:
                currentClaim = CardRank.Nine;
                break;
            case CardRank.Nine:
                currentClaim = CardRank.Ten;
                break;
            case CardRank.Ten:
                currentClaim = CardRank.Jack;
                break;
            case CardRank.Jack:
                currentClaim = CardRank.Queen;
                break;
            case CardRank.Queen:
                currentClaim = CardRank.King;
                break;
            case CardRank.King:
                currentClaim = CardRank.Ace;
                break;
        }
        
        OnGameMessage?.Invoke($"Next player must play {currentClaim}(s)");
    }

    void EndGame(Player winner)
    {
        gameInProgress = false;
        OnGameMessage?.Invoke($"Game Over! {winner.name} wins!");
        OnGameEnded?.Invoke();
    }

    // Utility methods for UI
    public List<Card> GetPlayerHand()
    {
        return players[0] == null ? null : players[0].hand;
    }

    public string GetCurrentClaim()
    {
        return currentClaim.ToString();
    }

    public string GetLastPlayedInfo()
    {
        if (lastPlayedCards == null || lastPlayedCards.Count == 0 || lastPlayerIndex < 0 || lastPlayerIndex >= players.Count)
        {
            return "No cards played yet";
        }
        
        Player lastPlayer = players[lastPlayerIndex];
        return $"{lastPlayer.name} played {lastPlayedCards.Count} {lastClaimedRank}(s)";
    }

    public int GetCardsToPlay()
    {
        return cardsToPlay;
    }

    public bool IsPlayerTurn()
    {
        return currentPlayerIndex == 0 && waitingForPlayerAction;
    }
}
