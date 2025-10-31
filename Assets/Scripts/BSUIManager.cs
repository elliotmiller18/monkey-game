using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages UI updates for the BS game
/// Updates player hands, game info, and current requirements
/// </summary>
public class BSUIManager : MonoBehaviour
{
    [Header("Game References")]
    public BSInterface bsInterface;
    
    [Header("Player Hand Displays")]
    public TextMeshProUGUI player0Hand;
    public TextMeshProUGUI player1Hand;
    public TextMeshProUGUI player2Hand;
    public TextMeshProUGUI player3Hand;
    
    [Header("Game Info Display")]
    public TextMeshProUGUI gameInfoText;
    
    [Header("Update Settings")]
    public float updateInterval = 0.5f; // Update every 0.5 seconds
    
    private float lastUpdateTime;
    private string lastGameMessage = "";
    private string lastCurrentClaim = "";
    private bool lastIsPlayerTurn = false;

    void Start()
    {
        if (bsInterface == null)
            bsInterface = FindFirstObjectByType<BSInterface>();
        
        if (bsInterface != null)
        {
            // Subscribe to game events
            bsInterface.OnMessageReceived += OnGameMessage;
            bsInterface.OnHandUpdated += OnHandUpdated;
            bsInterface.OnTurnChanged += OnTurnChanged;
            bsInterface.OnGameEnded += OnGameEnded;
        }
        
        // Initial update
        UpdateAllDisplays();
    }

    void OnDestroy()
    {
        if (bsInterface != null)
        {
            bsInterface.OnMessageReceived -= OnGameMessage;
            bsInterface.OnHandUpdated -= OnHandUpdated;
            bsInterface.OnTurnChanged -= OnTurnChanged;
            bsInterface.OnGameEnded -= OnGameEnded;
        }
    }

    void Update()
    {
        // Update UI at regular intervals
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateAllDisplays();
            lastUpdateTime = Time.time;
        }
    }

    void UpdateAllDisplays()
    {
        if (bsInterface == null) return;
        
        UpdatePlayerHands();
        UpdateGameInfo();
    }

    void UpdatePlayerHands()
    {
        if (bsInterface.gameManager == null || bsInterface.gameManager.players == null) return;
        
        // Update each player's hand display
        for (int i = 0; i < bsInterface.gameManager.players.Count && i < 4; i++)
        {
            Player player = bsInterface.gameManager.players[i];
            string handText = GetHandDisplayText(player, i);
            
            switch (i)
            {
                case 0:
                    if (player0Hand != null) player0Hand.text = handText;
                    break;
                case 1:
                    if (player1Hand != null) player1Hand.text = handText;
                    break;
                case 2:
                    if (player2Hand != null) player2Hand.text = handText;
                    break;
                case 3:
                    if (player3Hand != null) player3Hand.text = handText;
                    break;
            }
        }
    }

    string GetHandDisplayText(Player player, int playerIndex)
    {
        string playerName = player.name;
        string handCount = $"Cards: {player.hand.Count}";
        
        if (playerIndex == 0) // Human player - show actual cards
        {
            if (player.hand.Count == 0)
                return $"{playerName}\n{handCount}\nNo cards";
            
            string cardList = "";
            for (int i = 0; i < player.hand.Count && i < 8; i++) // Show up to 8 cards
            {
                cardList += player.hand[i].ToString() + "\n";
            }
            if (player.hand.Count > 8)
                cardList += "...";
            
            return $"{playerName}\n{handCount}\n{cardList}";
        }
        else // AI players - show card count only
        {
            return $"{playerName}\n{handCount}\n(Hidden)";
        }
    }

    void UpdateGameInfo()
    {
        if (gameInfoText == null) return;
        
        string currentClaim = bsInterface.GetCurrentClaim();
        bool isPlayerTurn = bsInterface.IsPlayerTurn();
        bool gameInProgress = bsInterface.IsGameInProgress();
        
        string gameInfo = "";
        
        if (!gameInProgress)
        {
            gameInfo = "Game Over!\nPress N for new game";
        }
        else
        {
            gameInfo = $"Current Turn: {currentClaim}\n";
            gameInfo += $"Your Turn: {(isPlayerTurn ? "YES" : "NO")}\n";
            gameInfo += $"Game Status: In Progress";
        }
        
        gameInfoText.text = gameInfo;
    }

    // Event handlers
    void OnGameMessage(string message)
    {
        if (message != lastGameMessage)
        {
            Debug.Log($"BS Game: {message}");
            lastGameMessage = message;
        }
    }

    void OnHandUpdated(List<Card> hand)
    {
        // Hand update will be handled by the regular update cycle
    }

    void OnTurnChanged(string playerName)
    {
        Debug.Log($"Turn changed to: {playerName}");
    }

    void OnGameEnded()
    {
        Debug.Log("Game ended!");
        UpdateGameInfo(); // Update to show game over state
    }
}
