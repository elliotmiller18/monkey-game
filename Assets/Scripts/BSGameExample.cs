using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Example script showing how to use the BS game system
/// This demonstrates the basic functions available for UI integration
/// </summary>
public class BSGameExample : MonoBehaviour
{
    [Header("Game References")]
    public BSInterface bsInterface;
    
    [Header("Debug Info")]
    public string currentMessage = "";
    public string currentTurn = "";
    public List<Card> playerHand = new List<Card>();
    public bool isPlayerTurn = false;
    public bool gameInProgress = false;

    void Start()
    {
        if (bsInterface == null)
            bsInterface = FindFirstObjectByType<BSInterface>();
        
        if (bsInterface != null)
        {
            // Subscribe to game events
            bsInterface.OnMessageReceived += OnMessageReceived;
            bsInterface.OnHandUpdated += OnHandUpdated;
            bsInterface.OnTurnChanged += OnTurnChanged;
            bsInterface.OnGameEnded += OnGameEnded;
        }
    }

    void OnDestroy()
    {
        if (bsInterface != null)
        {
            // Unsubscribe from events
            bsInterface.OnMessageReceived -= OnMessageReceived;
            bsInterface.OnHandUpdated -= OnHandUpdated;
            bsInterface.OnTurnChanged -= OnTurnChanged;
            bsInterface.OnGameEnded -= OnGameEnded;
        }
    }

    void Update()
    {
        // Update debug info
        if (bsInterface != null)
        {
            isPlayerTurn = bsInterface.IsPlayerTurn();
            gameInProgress = bsInterface.IsGameInProgress();
            playerHand = bsInterface.GetPlayerHand();
        }
    }

    // Example UI functions - these would be called by your UI buttons
    public void ExamplePlayCards()
    {
        if (!isPlayerTurn) return;
        
        // Example: Play all Aces if you have them
        List<Card> aces = bsInterface.GetCardsOfRank(CardRank.Ace);
        if (aces.Count > 0)
        {
            bsInterface.PlayCards(aces, CardRank.Ace);
        }
    }

    public void ExampleCallBS()
    {
        if (!isPlayerTurn) return;
        
        bsInterface.CallBS();
    }


    public void ExampleStartNewGame()
    {
        bsInterface.StartNewGame();
    }

    // Event handlers
    void OnMessageReceived(string message)
    {
        currentMessage = message;
        Debug.Log($"Game Message: {message}");
    }

    void OnHandUpdated(List<Card> hand)
    {
        playerHand = hand;
        Debug.Log($"Hand Updated: {bsInterface.GetHandAsString()}");
    }

    void OnTurnChanged(string playerName)
    {
        currentTurn = playerName;
        Debug.Log($"Turn Changed: {playerName}");
    }

    void OnGameEnded()
    {
        Debug.Log("Game Ended!");
    }

    // Utility methods for UI
    public string GetPlayerHandString()
    {
        return bsInterface?.GetHandAsString() ?? "No hand";
    }

    public string GetCurrentClaim()
    {
        return bsInterface?.GetCurrentClaim() ?? "";
    }

    public int GetCardsToPlay()
    {
        return bsInterface?.GetCardsToPlay() ?? 1;
    }

    // Example of how to check if player can play a specific rank
    public bool CanPlayRank(CardRank rank)
    {
        return bsInterface?.HasCardOfRank(rank) ?? false;
    }

    // Example of getting all available ranks in hand
    public List<CardRank> GetAvailableRanks()
    {
        List<CardRank> availableRanks = new List<CardRank>();
        List<Card> hand = bsInterface?.GetPlayerHand() ?? new List<Card>();
        
        foreach (Card card in hand)
        {
            if (!availableRanks.Contains(card.rank))
            {
                availableRanks.Add(card.rank);
            }
        }
        
        return availableRanks;
    }
}
