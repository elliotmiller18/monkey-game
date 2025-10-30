using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum GameState
{
    TruthTold,
    LieTold,
    WaitingForPlay,
    Inactive
}

public class BSGameLogic : MonoBehaviour
{
    [SerializeField] AudioClip clip;
    [SerializeField] CenterCard LastPlayed;
    [SerializeField] CenterCard NextUp;
    [SerializeField] GameObject ResetGameButton;
    public const int humanPlayerIndex = 0;
    int currentPlayer;
    int lastPlayedCount; // NEW: Track how many cards were just played
    CardRank expectedRank = CardRank.Ace;

    GameState state = GameState.Inactive;

    List<List<Card>> hands;
    List<Card> pile;

    public static BSGameLogic instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        //TODO: make this dynamic for game regeneration with varying # of monkeys, it'll just be 5 for now
        int numPlayers = 5;
        StartGame(numPlayers);
    }

    public bool IsHumanTurn()
    {
        return currentPlayer == humanPlayerIndex && state == GameState.WaitingForPlay;
    }
 
    public int GetPlayer()
    {
        return currentPlayer;
    }

    public GameState GetState()
    {
        return state;
    }

    public List<Card> GetHand(int id)
    {
        // id 0 is the player
        return hands[id];
    }

    // start game, so divide the deck among each player
    public void StartGame(int numPlayers)
    {
        Assert.AreEqual(state, GameState.Inactive, "Trying to start a game while one is active");

        ResetGameButton.SetActive(false);
        NextUp.SwitchImage(CardRank.Ace, 0);
        LastPlayed.Reset();

        currentPlayer = humanPlayerIndex;

        List<Card> deck = CardUtils.CreateDeck();
        CardUtils.ShuffleDeck(deck);

        hands = new List<List<Card>>();
        pile = new List<Card>();
        
        for (int i = 0; i < numPlayers; i++)
        {
            hands.Add(new List<Card>());
        }

        int c = 0;
        while (c < deck.Count)
        {
            for (int i = 0; i < numPlayers; i++)
            {
                if (c >= deck.Count) break;
                hands[i].Add(deck[c]);
                c++;
            }
        }

        state = GameState.WaitingForPlay;
        
        // NEW: Initialize AI (with null check)
        if (BSAIController.instance != null)
        {
            BSAIController.instance.InitializeAI(numPlayers, hands);
        }
        
        HandManager.instance.RenderCards(hands[humanPlayerIndex]);
    }

    // play card and set lie flag depending on how many 
    public void PlayCards(List<Card> played)
    {
        Assert.AreEqual(state, GameState.WaitingForPlay, "Trying to play a card while either waiting for call or continue or game is inactive");
        Assert.AreNotEqual(played.Count, 0, "Can't play zero cards");

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);

        lastPlayedCount = played.Count; // NEW: Track count
        state = GameState.TruthTold;
        
        foreach (Card c in played)
        {
            if (expectedRank != c.rank) state = GameState.LieTold;
            hands[currentPlayer].Remove(c);
            pile.Add(c);
            
            // NEW: Update AI knowledge
            if (BSAIController.instance != null)
            {
                BSAIController.instance.OnCardPlayed(currentPlayer, c);
            }
        }

        if (currentPlayer == humanPlayerIndex) HandManager.instance.RenderCards(hands[humanPlayerIndex]);
        LastPlayed.SwitchImage(expectedRank, played.Count);
        NextUp.SwitchImage(CardUtils.NextRank(expectedRank), 0);
        
        // Track the play for AI
        if (BSAIController.instance != null)
        {
            BSAIController.instance.TrackPlay(expectedRank, lastPlayedCount, hands);
        }
        
        // Start the timer for player to continue or AI to call
        if (TurnTimer.instance != null)
        {
            TurnTimer.instance.StartTimer();
        }
    }

    // call
    public void Call(int caller)
    {
        // can't call yourself
        Assert.AreNotEqual(caller, currentPlayer, "A player can't call BS on themselves");
        Assert.IsTrue(state == GameState.LieTold || state == GameState.TruthTold, "Trying to call while waiting for turn or game is inactive");

        // Stop the timer when BS is called
        if (TurnTimer.instance != null)
        {
            TurnTimer.instance.StopTimer();
        }

        int victim = state == GameState.LieTold ? currentPlayer : caller;
        PickUpPile(victim);

        // After BS is called, advance directly without AI check
        AdvanceToNextTurn();
    }

    // continue without a call
    public void Continue()
    {
        Assert.IsTrue(state == GameState.LieTold || state == GameState.TruthTold, "Trying to continue while waiting for a turn or game is inactive");

        // Stop the timer when continuing
        if (TurnTimer.instance != null)
        {
            TurnTimer.instance.StopTimer();
        }

        // Give AI a chance to call BS before continuing
        if (BSAIController.instance != null)
        {
            StartCoroutine(ContinueWithAICheck());
        }
        else
        {
            // No AI, just continue immediately
            AdvanceToNextTurn();
        }
    }

    System.Collections.IEnumerator ContinueWithAICheck()
    {
        int playerBeforeAICheck = currentPlayer;
        yield return StartCoroutine(BSAIController.instance.CheckForAICalls());
        if (currentPlayer != playerBeforeAICheck)
        {
            yield break;
        }
        AdvanceToNextTurn();
    }

    void AdvanceToNextTurn()
    {
        currentPlayer = (currentPlayer + 1) % hands.Count;
        expectedRank = CardUtils.NextRank(expectedRank);
        
        state = CheckForWin() ? GameState.Inactive : GameState.WaitingForPlay;
        if(CheckForWin())
        {
            state = GameState.Inactive;
            ResetGameButton.SetActive(true);
        } else
        {
            state = GameState.WaitingForPlay;
            if (currentPlayer != humanPlayerIndex)
            {
                PlayAITurn();
            }
        }
    }

    void PickUpPile(int victim)
    {
        hands[victim].AddRange(pile);
        if (victim == humanPlayerIndex)
        {
            HandManager.instance.RenderCards(hands[humanPlayerIndex]);
        }
        
        if (BSAIController.instance != null)
        {
            BSAIController.instance.OnPilePickedUp(victim, hands[victim]);
        }
        
        pile.Clear();
    }

    bool CheckForWin()
    {
        foreach (List<Card> hand in hands)
        {
            if (hand.Count == 0) return true;
        }
        return false;
    }
    
    public void EndGame()
    {
        state = GameState.Inactive;
        ResetGameButton.SetActive(true);
    }
    
    void PlayAITurn()
    {
        Assert.AreEqual(state, GameState.WaitingForPlay, "Trying to do ai turn while either waiting for call or continue or game is inactive");
        Assert.AreNotEqual(currentPlayer, humanPlayerIndex, "Trying to do ai turn while human player is expected to play");
        List<Card> cardsToPlay = new List<Card>();
        foreach (Card c in hands[currentPlayer])
        {
            if (c.rank == expectedRank) cardsToPlay.Add(c);
        }

        if (cardsToPlay.Count == 0) cardsToPlay.Add(hands[currentPlayer][0]);
        
        // Debug.Log($"AI Player {currentPlayer} playing {cardsToPlay.Count} card(s)");
        PlayCards(cardsToPlay);
    }
}