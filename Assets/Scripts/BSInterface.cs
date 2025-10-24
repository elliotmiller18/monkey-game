using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BSInterface : MonoBehaviour
{
    // moved the assignment to start
    [HideInInspector] public GameManager gameManager;
    
    [Header("UI Events")]
    public System.Action<string> OnMessageReceived;
    public System.Action<List<Card>> OnHandUpdated;
    public System.Action<string> OnTurnChanged;
    public System.Action OnGameEnded;

    public static BSInterface instance;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
    }

    void Start()
    {
        gameManager = GameManager.instance;
        Assert.IsNotNull(gameManager, "game manager not set in bsinterface");
        
        gameManager.OnGameMessage += HandleGameMessage;
        gameManager.OnPlayerHandUpdated += HandleHandUpdated;
        gameManager.OnPlayerTurnChanged += HandleTurnChanged;
        gameManager.OnGameEnded += HandleGameEnded;
    
    }

    void OnDestroy()
    {
            // Unsubscribe from events
            gameManager.OnGameMessage -= HandleGameMessage;
            gameManager.OnPlayerHandUpdated -= HandleHandUpdated;
            gameManager.OnPlayerTurnChanged -= HandleTurnChanged;
            gameManager.OnGameEnded -= HandleGameEnded;
    }

    // Public interface methods for UI to call
    public void PlayCards(List<Card> cards, CardRank claimedRank)
    {
            gameManager.PlayCards(cards, claimedRank);
    }

    public void CallBS()
    {
            gameManager.CallBS();
    }

    public void ContinueWithoutBS()
    {
            gameManager.ContinueWithoutBS();
    }



    public void StartNewGame()
    {
            gameManager.InitializeGame();
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

    public int GetCurrentPlayerID()
    {
        // player is index 0
        return gameManager.currentPlayerIndex;
    }

    public int GetNumPlayers()
    {
        return gameManager.players.Count;
    }

    public List<Card> GetMonkeyHand(int monkeyId)
    {
        // id 0 is the player
        int playerId = monkeyId + 1;
        if(playerId >= gameManager.players.Count || playerId <= 0)
        {
            Debug.Log("trying to get hand of invalid monkey id or monkey that is not the game");
            return new List<Card>();
        } else
        {
            return gameManager.players[playerId].hand;
        }
    }
}
