using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BSAIController : MonoBehaviour
{
    [Header("AI Call Settings")]
    [SerializeField] float aiCallDelayMin = 0.5f;
    [SerializeField] float aiCallDelayMax = 2.0f;
    
    [Header("Cheat Cooldown Settings")]
    [SerializeField] int minTurnsBetweenCheats = 2; 
    [SerializeField] int globalCheatCooldown = 1;
    
    private List<Dictionary<CardRank, int>> aiKnowledge;
    private int numPlayers;
    private CardRank lastExpectedRank;
    private int lastCardsPlayed;
    private List<List<Card>> currentHands;

    public static BSAIController instance;
    
    private Dictionary<int, int> lastCheatTurn = new Dictionary<int, int>();
    private int lastGlobalCheatTurn = -999; 
    private int currentTurnNumber = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public void InitializeAI(int playerCount, List<List<Card>> hands)
    {
        numPlayers = playerCount;
        aiKnowledge = new List<Dictionary<CardRank, int>>();
        lastCheatTurn.Clear();
        lastGlobalCheatTurn = -999;
        currentTurnNumber = 0;
        
        for (int i = 0; i < numPlayers; i++)
        {
            aiKnowledge.Add(new Dictionary<CardRank, int>());
            lastCheatTurn[i] = -999;
            
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
        currentHands = hands;
        currentTurnNumber++;
    }

    public void OnMonkeyCheatUsed(int monkeyIndex)
    {
        lastCheatTurn[monkeyIndex] = currentTurnNumber;
        lastGlobalCheatTurn = currentTurnNumber;
        Debug.Log($"Monkey {monkeyIndex} cheated on turn {currentTurnNumber}");
    }

    public bool CanMonkeyCheat(int monkeyIndex)
    {
        if (lastCheatTurn.ContainsKey(monkeyIndex))
        {
            int turnsSinceLastCheat = currentTurnNumber - lastCheatTurn[monkeyIndex];
            if (turnsSinceLastCheat < minTurnsBetweenCheats)
            {
                Debug.Log($"Monkey {monkeyIndex} on individual cooldown ({turnsSinceLastCheat}/{minTurnsBetweenCheats} turns)");
                return false;
            }
        }
        
        int turnsSinceAnyCheat = currentTurnNumber - lastGlobalCheatTurn;
        if (turnsSinceAnyCheat < globalCheatCooldown)
        {
            Debug.Log($"Global cheat cooldown active ({turnsSinceAnyCheat}/{globalCheatCooldown} turns)");
            return false;
        }
        
        return true;
    }

    public System.Collections.IEnumerator CheckForAICalls()
    {
        int currentPlayer = BSGameLogic.instance.GetPlayer();
        GameState state = BSGameLogic.instance.GetState();
        
        if (state != GameState.TruthTold && state != GameState.LieTold)
        {
            yield break;
        }
        
        if (currentHands == null)
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
        return GetCallProbability(monkeyIndex, BSGameLogic.humanPlayerIndex, expectedRank, cardsPlayed);
    }

    bool ShouldAICallSimple(int aiPlayer, int targetPlayer)
    {
        return ShouldAICall(aiPlayer, targetPlayer, lastExpectedRank, lastCardsPlayed, currentHands);
    }

    bool ShouldAICall(int aiPlayer, int targetPlayer, CardRank expectedRank, int cardsPlayed, List<List<Card>> hands)
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

        int targetHandSize;
        if (currentHands != null && targetPlayer < currentHands.Count)
        {
            targetHandSize = currentHands[targetPlayer].Count;
        }
        else
        {
            targetHandSize = BSGameLogic.instance.GetHand(targetPlayer).Count;
        }

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
        if (!isRealData)
        {
            Debug.Log($"Monkey {monkeyIndex} was given false info (deflected), ignoring peek data");
            return;
        }
        
        foreach (CardRank r in peekedCards.Select(str => CardUtils.StringToRank(str)))
        {
            if (!aiKnowledge[monkeyIndex].ContainsKey(r))
            {
                aiKnowledge[monkeyIndex][r] = 1;
                continue;
            }

            int previousRankKnowledge = aiKnowledge[monkeyIndex][r];
            int cardsHeldOfRank = BSGameLogic.instance.GetMonkeyHand(monkeyIndex).Where(c => c.rank == r).Count();
            int remainingCardsPlayerCanHold = 4 - cardsHeldOfRank - previousRankKnowledge;

            if (remainingCardsPlayerCanHold <= 0) continue;
            if (Random.value < (1 / (previousRankKnowledge + 1f))) 
                aiKnowledge[monkeyIndex][r] = Mathf.Min(4, aiKnowledge[monkeyIndex][r] + 1);
        }
    }
}