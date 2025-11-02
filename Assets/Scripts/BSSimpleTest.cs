using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple test script for the BS game that doesn't require UI components
/// This provides keyboard controls and console output to test the game functionality
/// </summary>
public class BSSimpleTest : MonoBehaviour
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
        
        Debug.Log("=== BS Game Simple Test ===");
        Debug.Log("Controls:");
        Debug.Log("SPACE - Play your card (when it's your turn) OR continue without calling BS (after AI turn)");
        Debug.Log("B - Call BS on the previous player (only right after their turn)");
        Debug.Log("N - New Game");
        Debug.Log("H - Show Hand");
        Debug.Log("S - Show Status");
        Debug.Log("");
        Debug.Log("Game Rules:");
        Debug.Log("- Players must play cards in sequence: Ace -> 2 -> 3 -> ... -> King -> Ace");
        Debug.Log("- You MUST play a card every turn (no passing allowed)");
        Debug.Log("- After each AI turn, you can call BS or continue");
        Debug.Log("- If you call BS and are wrong, you pick up the pile");
        Debug.Log("- If you call BS and are right, they pick up the pile");
        Debug.Log("- Pile clears after BS call and game continues");
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (bsInterface.IsPlayerTurn())
            {
                // It's your turn to play a card
                PlayRequiredCard();
            }
            else
            {
                // It's not your turn, but you can continue without calling BS
                ContinueGame();
            }
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            // Call BS on the previous player
            CallBS();
        }
        else if (Input.GetKeyDown(KeyCode.N))
            NewGame();
        else if (Input.GetKeyDown(KeyCode.H))
            ShowHand();
        else if (Input.GetKeyDown(KeyCode.S))
            ShowStatus();
    }

    void PlayRequiredCard()
    {
        if (!bsInterface.IsPlayerTurn())
        {
            Debug.Log("Not your turn!");
            return;
        }
        
        List<Card> hand = bsInterface.GetPlayerHand();
        if (hand.Count == 0)
        {
            Debug.Log("No cards in hand!");
            return;
        }
        
        // Get the current required rank from the game
        string currentClaimString = bsInterface.GetCurrentClaim();
        if (System.Enum.TryParse(currentClaimString, out CardRank requiredRank))
        {
            // Check if we have the required rank (honest play)
            List<Card> cardsOfRank = bsInterface.GetCardsOfRank(requiredRank);
            if (cardsOfRank.Count > 0)
            {
                // Play honestly - we have the required cards
                bsInterface.PlayCards(cardsOfRank, requiredRank);
                Debug.Log($"Played {cardsOfRank.Count} {requiredRank}(s) honestly");
            }
            else
            {
                // Bluff - play a random card but claim it's the required rank
                Card randomCard = hand[Random.Range(0, hand.Count)];
                List<Card> bluffCards = new List<Card> { randomCard };
                
                bsInterface.PlayCards(bluffCards, requiredRank);
                Debug.Log($"Bluffed! Played {randomCard.ToString()} claiming it's a {requiredRank}");
            }
        }
        else
        {
            Debug.Log($"Could not parse required rank: {currentClaimString}");
        }
    }

    void ContinueGame()
    {
        // Continue without calling BS (this should work when it's not your turn)
        bsInterface.ContinueWithoutBS();
        Debug.Log("Continued without calling BS");
    }

    void CallBS()
    {
        // Can call BS anytime during the game
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
