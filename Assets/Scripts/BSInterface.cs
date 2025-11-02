using System.Collections.Generic;
using UnityEngine;

public class BSInterface : MonoBehaviour
{
    [Header("Game Manager Reference")]
    public GameManager gameManager;
    
    [Header("UI Events")]
    public System.Action<string> OnMessageReceived;
    public System.Action<List<Card>> OnHandUpdated;
    public System.Action<string> OnTurnChanged;
    public System.Action OnGameEnded;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        if (gameManager != null)
        {
            // Subscribe to game events
            gameManager.OnGameMessage += HandleGameMessage;
            gameManager.OnPlayerHandUpdated += HandleHandUpdated;
            gameManager.OnPlayerTurnChanged += HandleTurnChanged;
            gameManager.OnGameEnded += HandleGameEnded;
        }
    }

    void OnDestroy()
    {
        if (gameManager != null)
        {
            // Unsubscribe from events
            gameManager.OnGameMessage -= HandleGameMessage;
            gameManager.OnPlayerHandUpdated -= HandleHandUpdated;
            gameManager.OnPlayerTurnChanged -= HandleTurnChanged;
            gameManager.OnGameEnded -= HandleGameEnded;
        }
    }

    // Public interface methods for UI to call
    public void PlayCards(List<Card> cards, CardRank claimedRank)
    {
        if (gameManager != null)
        {
            gameManager.PlayCards(cards, claimedRank);
        }
    }

    public void CallBS()
    {
        if (gameManager != null)
        {
            gameManager.CallBS();
        }
    }

    public void ContinueWithoutBS()
    {
        if (gameManager != null)
        {
            gameManager.ContinueWithoutBS();
        }
    }



    public void StartNewGame()
    {
        if (gameManager != null)
        {
            gameManager.InitializeGame();
        }
    }

    // Getter methods for UI
    public List<Card> GetPlayerHand()
    {
        return gameManager?.GetPlayerHand() ?? new List<Card>();
    }

    public string GetCurrentClaim()
    {
        return gameManager?.GetCurrentClaim() ?? "";
    }

    public string GetLastPlayedInfo()
    {
        return gameManager?.GetLastPlayedInfo() ?? "";
    }

    public int GetCardsToPlay()
    {
        return gameManager?.GetCardsToPlay() ?? 1;
    }

    public bool IsPlayerTurn()
    {
        return gameManager?.IsPlayerTurn() ?? false;
    }

    public bool IsGameInProgress()
    {
        return gameManager?.gameInProgress ?? false;
    }

    // Event handlers
    void HandleGameMessage(string message)
    {
        OnMessageReceived?.Invoke(message);
    }

    void HandleHandUpdated(List<Card> hand)
    {
        OnHandUpdated?.Invoke(hand);
    }

    void HandleTurnChanged(int playerIndex)
    {
        if (gameManager != null && gameManager.players != null && playerIndex < gameManager.players.Count)
        {
            OnTurnChanged?.Invoke(gameManager.players[playerIndex].name);
        }
    }

    void HandleGameEnded()
    {
        OnGameEnded?.Invoke();
    }

    // Utility methods for UI
    public List<Card> GetCardsOfRank(CardRank rank)
    {
        List<Card> hand = GetPlayerHand();
        List<Card> cardsOfRank = new List<Card>();
        
        foreach (Card card in hand)
        {
            if (card.rank == rank)
                cardsOfRank.Add(card);
        }
        
        return cardsOfRank;
    }

    public bool HasCardOfRank(CardRank rank)
    {
        return GetCardsOfRank(rank).Count > 0;
    }

    public string GetHandAsString()
    {
        List<Card> hand = GetPlayerHand();
        if (hand.Count == 0)
            return "No cards";
        
        string handString = "";
        for (int i = 0; i < hand.Count; i++)
        {
            handString += hand[i].ToString();
            if (i < hand.Count - 1)
                handString += ", ";
        }
        return handString;
    }
}
