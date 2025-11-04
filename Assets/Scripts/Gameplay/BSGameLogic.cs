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
    [SerializeField] GameObject ResetGameButton;
    public const int humanPlayerIndex = 0;
    int currentPlayer;
    int lastPlayedCount;
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

    public void EndGame()
    {
        Debug.Log("Ending game called");
        state = GameState.Inactive;
        ResetGameButton.SetActive(true);
        LastPlayed.instance.HideTextAndButton();
        Debug.Log("ending game");
        Pile.instance.ClearPile();
        if (BSAICheat.instance != null)
        {
            BSAICheat.instance.StopPeekSystem();
        }
    }

    void Start()
    {
        // + 1 for player
        int numPlayers = MonkeyObjects.NumMonkeys() + 1;
        StartGame(numPlayers);
    }

    public CardRank GetExpectedRank()
    {
        return expectedRank;
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
        return hands[id];
    }

    public List<Card> GetMonkeyHand(int monkeyId)
    {
        // id 0 is the player
        return hands[monkeyId + 1];
    }

    void InstanceAssertions()
    {
        Assert.IsNotNull(LastPlayed.instance, "LastPlayed instance is null");
        Assert.IsNotNull(Pile.instance, "Pile instance is null");
        Assert.IsNotNull(BSAICheat.instance, "BSAICheat instance is null");
        Assert.IsNotNull(BSAIController.instance, "BSAIController instance is null");
        Assert.IsNotNull(HandManager.instance, "HandManager instance is null");
        Assert.IsNotNull(TurnTimer.instance, "TurnTimer instance is null");
        Assert.IsNotNull(MonkeyBSGame.instance, "MonkeyBSGame instance is null");
        Assert.IsNotNull(CallArrow.instance, "CallArrow instance is null");
        Assert.IsNotNull(CallAudio.instance, "CallAudio instance is null");
    }

    // start game, so divide the deck among each player
    public void StartGame(int numPlayers)
    {
        Assert.AreEqual(state, GameState.Inactive, "Trying to start a game while one is active");
        // assert that all of the instances we use aren't null 
        InstanceAssertions();

        ResetGameButton.SetActive(false);

        currentPlayer = humanPlayerIndex;

        List<Card> deck = CardUtils.CreateDeck();
        CardUtils.ShuffleDeck(deck);

        hands = new List<List<Card>>();
        pile = new List<Card>();
        expectedRank = CardRank.Ace;
        
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
        
        BSAIController.instance.InitializeAI(numPlayers, hands);
        BSAICheat.instance.StartPeekSystem();
        
        HandManager.instance.RenderCards(hands[humanPlayerIndex]);
        MonkeyBSGame.instance.RestartGame();
    }

    // play card and set lie flag depending on how many 
    public void PlayCards(List<Card> played)
    {
        Assert.AreEqual(state, GameState.WaitingForPlay, "Trying to play a card while either waiting for call or continue or game is inactive");
        Assert.AreNotEqual(played.Count, 0, "Can't play zero cards");

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);

        // NEW: Play card animation for the current player
        if (MonkeyBSGame.instance != null && currentPlayer != humanPlayerIndex)
        {
            // currentPlayer is indexed from 0, but monkey list might need adjustment
            // Subtract 1 because player 0 is human, monkeys start at index 0 for player 1
            MonkeyBSGame.instance.PlayCardAnimation(currentPlayer - 1);
        }

        lastPlayedCount = played.Count; // NEW: Track count
        state = GameState.TruthTold;
        
        foreach (Card c in played)
        {
            if (expectedRank != c.rank) state = GameState.LieTold;
            hands[currentPlayer].Remove(c);
            pile.Add(c);
            
            BSAIController.instance.OnCardPlayed(currentPlayer, c);
        }

        if (currentPlayer == humanPlayerIndex) HandManager.instance.RenderCards(hands[humanPlayerIndex]);

        LastPlayed.instance.UpdateText(currentPlayer, played.Count, expectedRank);
        Pile.instance.AddCards(played.Count);

        BSAIController.instance.TrackPlay(expectedRank, lastPlayedCount, hands);

        TurnTimer.instance.StartTimer();
    }

    // call
    public void Call(int caller)
    {
        // can't call yourself
        Assert.AreNotEqual(caller, currentPlayer, "A player can't call BS on themselves");
        Assert.IsTrue(state == GameState.LieTold || state == GameState.TruthTold, "Trying to call while waiting for turn or game is inactive");

        // Stop the timer when BS is called
        TurnTimer.instance.StopTimer();

        bool successful = state == GameState.LieTold;

        CallArrow.instance.DrawArrow(caller, currentPlayer, successful);
        CallAudio.instance.PlayCallClip(successful);
        
        int victim = successful ? currentPlayer : caller;
        PickUpPile(victim);

        // After BS is called, advance directly without AI check
        AdvanceToNextTurn();
    }

    // continue without a call
    public void Continue()
    {
        Assert.IsTrue(state == GameState.LieTold || state == GameState.TruthTold, "Trying to continue while waiting for a turn or game is inactive");

        // Stop the timer when continuing
        TurnTimer.instance.StopTimer();

        StartCoroutine(ContinueWithAICheck());
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
    
        BSAIController.instance.OnPilePickedUp(victim, hands[victim]);
        
        pile.Clear();
        Pile.instance.ClearPile();
    }

    bool CheckForWin()
    {
        foreach (List<Card> hand in hands)
        {
            if (hand.Count == 0) return true;
        }
        return false;
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
        
        PlayCards(cardsToPlay);
    }
}