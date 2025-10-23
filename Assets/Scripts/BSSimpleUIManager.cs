using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI manager for BS game that updates text displays
/// Connects to your existing UI text elements
/// </summary>
public class BSSimpleUIManager : MonoBehaviour
{
    [Header("Game References")]
    public BSInterface bsInterface;
    
    [Header("UI Text Elements")]
    public TextMeshProUGUI player0Text; // p0
    public TextMeshProUGUI player1Text; // p1  
    public TextMeshProUGUI player2Text; // p2
    public TextMeshProUGUI player3Text; // p3
    public TextMeshProUGUI gameInfoText; // Game Info (center)
    
    [Header("Update Settings")]
    public float updateInterval = 0.3f;
    
    private float lastUpdateTime;

    void Start()
    {
        if (bsInterface == null)
            bsInterface = FindFirstObjectByType<BSInterface>();
        
        if (bsInterface != null)
        {
            bsInterface.OnMessageReceived += OnGameMessage;
            bsInterface.OnHandUpdated += OnHandUpdated;
            bsInterface.OnTurnChanged += OnTurnChanged;
            bsInterface.OnGameEnded += OnGameEnded;
        }
        
        // Initial update
        UpdateAllText();
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
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateAllText();
            lastUpdateTime = Time.time;
        }
    }

    public void UpdateAllText()
    {
        if (bsInterface == null) return;
        
        UpdatePlayerTexts();
        UpdateGameInfoText();
    }

    void UpdatePlayerTexts()
    {
        if (bsInterface.gameManager == null || bsInterface.gameManager.players == null) return;
        
        // Update each player text
        for (int i = 0; i < bsInterface.gameManager.players.Count && i < 4; i++)
        {
            Player player = bsInterface.gameManager.players[i];
            string playerText = GetPlayerDisplayText(player, i);
            
            switch (i)
            {
                case 0:
                    if (player0Text != null) player0Text.text = playerText;
                    break;
                case 1:
                    if (player1Text != null) player1Text.text = playerText;
                    break;
                case 2:
                    if (player2Text != null) player2Text.text = playerText;
                    break;
                case 3:
                    if (player3Text != null) player3Text.text = playerText;
                    break;
            }
        }
    }

    string GetPlayerDisplayText(Player player, int playerIndex)
    {
        string playerName = player.name;
        int cardCount = player.hand.Count;
        
        if (cardCount == 0)
        {
            return $"{playerName}\nNo cards";
        }
        
        // Sort cards from Ace to King
        List<Card> sortedHand = new List<Card>(player.hand);
        sortedHand.Sort((a, b) => a.rank.CompareTo(b.rank));
        
        // Show specific cards for all players
        string cardList = "";
        int maxCardsToShow = 8; // Increased to show more cards
        
        for (int i = 0; i < sortedHand.Count && i < maxCardsToShow; i++)
        {
            cardList += sortedHand[i].ToString() + "\n";
        }
        
        if (sortedHand.Count > maxCardsToShow)
        {
            cardList += $"... +{sortedHand.Count - maxCardsToShow} more";
        }
        
        return $"{playerName}\n{cardCount} cards\n{cardList}";
    }

    void UpdateGameInfoText()
    {
        if (gameInfoText == null) return;
        
        string lastPlayedInfo = bsInterface.GetLastPlayedInfo();
        string currentClaim = bsInterface.GetCurrentClaim();
        bool isPlayerTurn = bsInterface.IsPlayerTurn();
        bool gameInProgress = bsInterface.IsGameInProgress();
        
        string info = "";
        
        if (!gameInProgress)
        {
            info = "Game Over!\nPress N for new game";
        }
        else if (isPlayerTurn)
        {
            // Player's turn - show what they need to play
            info = $"Play: {currentClaim}\n";
            info += $"Your Turn: YES\n";
            info += $"Press SPACE to play\n";
            info += $"Press B to call BS";
        }
        else
        {
            // AI's turn - show what was just played
            info = $"Last Play: {lastPlayedInfo}\n";
            info += $"Next Rank: {currentClaim}\n";
            info += $"Your Turn: NO\n";
            info += $"Press SPACE to continue\n";
            info += $"Press B to call BS";
        }
        
        gameInfoText.text = info;
    }

    // Event handlers
    void OnGameMessage(string message)
    {
        // Game messages are handled by the game manager
    }

    void OnHandUpdated(List<Card> hand)
    {
        // Hand updates are handled by the regular update cycle
    }

    void OnTurnChanged(string playerName)
    {
        // Turn changes are handled by the regular update cycle
    }

    void OnGameEnded()
    {
        // Game end is handled by the regular update cycle
    }
}
