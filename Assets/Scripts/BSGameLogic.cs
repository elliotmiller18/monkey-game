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
    CardRank expectedRank = CardRank.Ace;

    GameState state = GameState.Inactive;

    List<List<Card>> hands;
    List<Card> pile;

    public static BSGameLogic instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate BSGameLogic, destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void EndGame()
    {
        state = GameState.Inactive;
        ResetGameButton.SetActive(true);
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
        NextUp.SwitchImage(new Card(Suit.Spades, CardRank.Ace), 0);
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
        HandManager.instance.RenderCards(hands[humanPlayerIndex]);
        MonkeyBSGame.instance.RestartGame();
    }

    // play card and set lie flag depending on how many 
    public void PlayCards(List<Card> played)
    {
        Assert.AreEqual(state, GameState.WaitingForPlay, "Trying to play a card while either waiting for call or continue or game is inactive");
        Assert.AreNotEqual(played.Count, 0, "Can't play zero cards");

        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);

        state = GameState.TruthTold;
        foreach (Card c in played)
        {
            if (expectedRank != c.rank) state = GameState.LieTold;
            hands[currentPlayer].Remove(c);
            pile.Add(c);
        }

        if (currentPlayer == humanPlayerIndex) HandManager.instance.RenderCards(hands[humanPlayerIndex]);
        LastPlayed.SwitchImage(played[0], played.Count);
        NextUp.SwitchImage(new Card(Suit.Spades, CardUtils.NextRank(expectedRank)), 0);
    }

    // call
    public void Call(int caller)
    {
        // can't call yourself
        Assert.AreNotEqual(caller, currentPlayer, "A player can't call BS on themselves");
        Assert.IsTrue(state == GameState.LieTold || state == GameState.TruthTold, "Trying to call while waiting for turn or game is inactive");

        int victim = state == GameState.LieTold ? currentPlayer : caller;
        PickUpPile(victim);

        Continue();
    }

    // continue without a call
    public void Continue()
    {
        Assert.IsTrue(state == GameState.LieTold || state == GameState.TruthTold, "Trying to continue while waiting for a turn or game is inactive");

        currentPlayer = (currentPlayer + 1) % hands.Count;
        expectedRank = CardUtils.NextRank(expectedRank);
        state = CheckForWin() ? GameState.Inactive : GameState.WaitingForPlay;
        if(CheckForWin())
        {
            EndGame();
        } else
        {
            state = GameState.WaitingForPlay;
            if (currentPlayer != humanPlayerIndex) PlayAITurn();
        }

    }

    // self explanatory
    void PickUpPile(int victim)
    {
        // no assertion here cause it's just a helper function, there aren't any transitions
        hands[victim].AddRange(pile);
        if (victim == humanPlayerIndex)
        {
            HandManager.instance.RenderCards(hands[humanPlayerIndex]);
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
    
    void PlayAITurn()
    {
        Assert.AreEqual(state, GameState.WaitingForPlay, "Trying to do ai turn while either waiting for call or continue or game is inactive");
        Assert.AreNotEqual(currentPlayer, humanPlayerIndex, "Trying to do ai turn while human player is expected to play");
        // very simple for now, either play all cards such that AI isn't lying or if impossible just play the first card and lie
        List<Card> cardsToPlay = new List<Card>();
        foreach (Card c in hands[currentPlayer])
        {
            if (c.rank == expectedRank) cardsToPlay.Add(c);
        }

        // we either play all fitting cards or just the first card
        if (cardsToPlay.Count == 0) cardsToPlay.Add(hands[currentPlayer][0]);
        PlayCards(cardsToPlay);
    }
}
