using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple test UI for the BS game
/// This provides keyboard controls and console output to test the game functionality
/// </summary>
public class BSTestUI : MonoBehaviour
{
    [Header("Game References")]
    public BSInterface bsInterface;
    
    private List<string> messageHistory = new List<string>();
    private const int MAX_MESSAGES = 10;

    void Start()
    {
        // Find the BS interface if not assigned
        if (bsInterface == null)
            bsInterface = FindFirstObjectByType<BSInterface>();
        
        if (bsInterface == null)
        {
            Debug.LogError("BSInterface not found! Make sure you have a GameManager in the scene.");
            return;
        }
        
        // Subscribe to game events
        bsInterface.OnMessageReceived += OnMessageReceived;
        bsInterface.OnHandUpdated += OnHandUpdated;
        bsInterface.OnTurnChanged += OnTurnChanged;
        bsInterface.OnGameEnded += OnGameEnded;
        
        Debug.Log("=== BS Game Test UI ===");
        Debug.Log("Controls:");
        Debug.Log("1 - Play Aces");
        Debug.Log("2 - Play Kings");
        Debug.Log("3 - Call BS");
        Debug.Log("4 - Pass");
        Debug.Log("5 - New Game");
        Debug.Log("H - Show Hand");
        Debug.Log("S - Show Status");
    }

    void OnDestroy()
    {
        if (bsInterface != null)
        {
            bsInterface.OnMessageReceived -= OnMessageReceived;
            bsInterface.OnHandUpdated -= OnHandUpdated;
            bsInterface.OnTurnChanged -= OnTurnChanged;
            bsInterface.OnGameEnded -= OnGameEnded;
        }
    }

    void Update()
    {
        // Handle keyboard input
        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlayCardsOfRank(CardRank.Ace);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            PlayCardsOfRank(CardRank.King);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            CallBS();
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            NewGame();
        else if (Input.GetKeyDown(KeyCode.H))
            ShowHand();
        else if (Input.GetKeyDown(KeyCode.S))
            ShowStatus();
    }

    void PlayCardsOfRank(CardRank rank)
    {
        if (!bsInterface.IsPlayerTurn())
        {
            Debug.Log("Not your turn!");
            return;
        }
        
        List<Card> cardsToPlay = bsInterface.GetCardsOfRank(rank);
        if (cardsToPlay.Count > 0)
        {
            bsInterface.PlayCards(cardsToPlay, rank);
            Debug.Log($"Played {cardsToPlay.Count} {rank}(s)");
        }
        else
        {
            Debug.Log($"You don't have any {rank}s");
        }
    }

    void CallBS()
    {
        if (!bsInterface.IsPlayerTurn())
        {
            Debug.Log("Not your turn!");
            return;
        }
        
        bsInterface.CallBS();
        Debug.Log("Called BS!");
    }


    void NewGame()
    {
        bsInterface.StartNewGame();
        Debug.Log("Started new game");
    }

    void ShowHand()
    {
        List<Card> hand = bsInterface.GetPlayerHand();
        if (hand.Count == 0)
        {
            Debug.Log("No cards in hand");
            return;
        }
        
        Debug.Log($"Hand ({hand.Count} cards):");
        foreach (Card card in hand)
        {
            Debug.Log($"  {card.ToString()}");
        }
    }

    void ShowStatus()
    {
        Debug.Log($"Game in progress: {bsInterface.IsGameInProgress()}");
        Debug.Log($"Player turn: {bsInterface.IsPlayerTurn()}");
        Debug.Log($"Current claim: {bsInterface.GetCurrentClaim()}");
        Debug.Log($"Cards to play: {bsInterface.GetCardsToPlay()}");
        Debug.Log($"Hand size: {bsInterface.GetPlayerHand().Count}");
    }

    // Event handlers
    void OnMessageReceived(string message)
    {
        messageHistory.Add(message);
        if (messageHistory.Count > MAX_MESSAGES)
        {
            messageHistory.RemoveAt(0);
        }
        
        Debug.Log($"BS Game: {message}");
    }

    void OnHandUpdated(List<Card> hand)
    {
        Debug.Log($"Hand updated: {hand.Count} cards");
    }

    void OnTurnChanged(string playerName)
    {
        Debug.Log($"Turn changed to: {playerName}");
    }

    void OnGameEnded()
    {
        Debug.Log("Game ended!");
    }
}