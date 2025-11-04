using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BSAIController : MonoBehaviour
{
    [Header("AI Call Settings")]
    [SerializeField] float aiCallDelayMin = 0.5f;
    [SerializeField] float aiCallDelayMax = 2.0f;
    
    [Header("Cheat Cooldown Settings")]
    [SerializeField] int minTurnsBetweenCheats = 2; // Minimum turns before same monkey can cheat again
    [SerializeField] int globalCheatCooldown = 1; // Minimum turns before ANY monkey can cheat after a cheat
    
    private List<Dictionary<CardRank, int>> aiKnowledge;
    private int numPlayers;
    private CardRank lastExpectedRank;
    private int lastCardsPlayed;

    public static BSAIController instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            Debug.LogError("duplicate BSAIController, destroying");
            return;
        }
        instance = this;
    }

    public void InitializeAI(int playerCount, List<List<Card>> hands)
    {
        numPlayers = playerCount;
        aiKnowledge = new List<Dictionary<CardRank, int>>();

        for (int i = 0; i < numPlayers; i++)
        {
            aiKnowledge.Add(new Dictionary<CardRank, int>());

            if (i != BSGameLogic.humanPlayerIndex)
            {
                foreach (Card card in hands[i])
                {
                    if (!aiKnowledge[i].ContainsKey(card.rank))
                        aiKnowledge[i][card.rank] = 0;
                    aiKnowledge[i][card.rank]++;
                }
            }
        }
    }

    public void OnCardPlayed(int playerIndex, Card card)
    {
        if (playerIndex == BSGameLogic.humanPlayerIndex) return;
        
        if (aiKnowledge[playerIndex].ContainsKey(card.rank))
        {
            aiKnowledge[playerIndex][card.rank]--;
            if (aiKnowledge[playerIndex][card.rank] <= 0)
                aiKnowledge[playerIndex].Remove(card.rank);
        }
    }

    public void OnPilePickedUp(int playerIndex, List<Card> hand)
    {
        if (playerIndex == BSGameLogic.humanPlayerIndex) return;
        
        aiKnowledge[playerIndex].Clear();
        foreach (Card c in hand)
        {
            if (!aiKnowledge[playerIndex].ContainsKey(c.rank))
                aiKnowledge[playerIndex][c.rank] = 0;
            aiKnowledge[playerIndex][c.rank]++;
        }
    }
    
    public void TrackPlay(CardRank expectedRank, int cardsPlayed, List<List<Card>> hands)
    {
        lastExpectedRank = expectedRank;
        lastCardsPlayed = cardsPlayed;
    }

    public System.Collections.IEnumerator CheckForAICalls()
    {
        int currentPlayer = BSGameLogic.instance.GetPlayer();
        GameState state = BSGameLogic.instance.GetState();

        if (state != GameState.TruthTold && state != GameState.LieTold)
        {
            yield break;
        }

        for (int i = 0; i < numPlayers; i++)
        {
            if (i == currentPlayer || i == BSGameLogic.humanPlayerIndex)
            {
                continue;
            }

            if (ShouldAICallSimple(i, currentPlayer))
            {
                Debug.Log($"*** AI Player {i} CALLS BS on Player {currentPlayer}! ***");
                BSGameLogic.instance.Call(i);
                yield break;
            }
        }
    }
    
    public float GetMonkeySuspicion(int monkeyIndex, CardRank expectedRank, int cardsPlayed)
    {
        return GetCallProbability(monkeyIndex + 1, BSGameLogic.humanPlayerIndex, expectedRank, cardsPlayed);
    }

    bool ShouldAICallSimple(int aiPlayer, int targetPlayer)
    {
        return ShouldAICall(aiPlayer, targetPlayer, lastExpectedRank, lastCardsPlayed);
    }

    bool ShouldAICall(int aiPlayer, int targetPlayer, CardRank expectedRank, int cardsPlayed)
    {
        float randomRoll = Random.value;
        bool willCall = randomRoll < GetCallProbability(aiPlayer, targetPlayer, expectedRank, cardsPlayed);
        return willCall;
    }

    float GetCallProbability(int aiPlayer, int targetPlayer, CardRank expectedRank, int cardsPlayed)
    {
        int cardsOfExpectedRank = 0;
        if (aiKnowledge[aiPlayer].ContainsKey(expectedRank))
        {
            cardsOfExpectedRank = aiKnowledge[aiPlayer][expectedRank];
        }

        float baseProbability = 0f;
        switch (cardsOfExpectedRank)
        {
            case 0: baseProbability = 0.08f; break;
            case 1: baseProbability = 0.15f; break;
            case 2: baseProbability = 0.25f; break;
            case 3: baseProbability = 0.40f; break;
            case 4: baseProbability = 0.60f; break;
        }

        int targetHandSize = BSGameLogic.instance.GetHand(targetPlayer).Count;
        float handSizeMultiplier = 1.0f;

        if (targetHandSize <= 3)
            handSizeMultiplier = 2.0f;
        else if (targetHandSize <= 6)
            handSizeMultiplier = 1.5f;
        else if (targetHandSize <= 10)
            handSizeMultiplier = 1.2f;
        else if (targetHandSize >= 20)
            handSizeMultiplier = 0.7f;

        float finalProbability = Mathf.Clamp01(baseProbability * handSizeMultiplier);

        if (cardsPlayed >= 3)
        {
            finalProbability = Mathf.Clamp01(finalProbability * 1.3f);
        }

        return finalProbability;
    }
    
    public void MonkeyPeekedAtHuman(int monkeyIndex, string[] peekedCards, bool isRealData)
    {
        if(!isRealData)
        {
            Debug.Log($"Monkey {monkeyIndex} was given false info, just returning :D");
            return;
        }
        // Store the peeked information        
        foreach (CardRank r in peekedCards.Select(str => CardUtils.StringToRank(str)))
        {
            if (!aiKnowledge[monkeyIndex].ContainsKey(r))
            {
                aiKnowledge[monkeyIndex][r] = 1;
                continue;
            }

            int previousRankKnowledge = aiKnowledge[monkeyIndex][r];
            // the number of cards of rank r this monkey holds
            int cardsHeldOfRank = BSGameLogic.instance.GetMonkeyHand(monkeyIndex).Where(c => c.rank == r).Count();
            int remainingCardsPlayerCanHold = 4 - cardsHeldOfRank - previousRankKnowledge;

            // the ai already thinks that the player and themselves hold 4 cards, meaning we shouldn't do anything
            // as we are necessarily seeing a card that we already know about
            if (remainingCardsPlayerCanHold <= 0) continue;

            // we randomly decide whether or not to add another card based on the chance that we're seeing a card we already know about
            // the chance being 1 / previousRankKnowledge + 1 so 100% chance if we didn't know that they had this rank, 50% if they already have 1, etc etc
            if (Random.value < (1 / (previousRankKnowledge + 1f))) aiKnowledge[monkeyIndex][r] = Mathf.Min(4, aiKnowledge[monkeyIndex][r] + 1);
        }
        
        Debug.Log($"Monkey {monkeyIndex} peeked at human. Saw: {string.Join(", ", peekedCards)}");
    }
}