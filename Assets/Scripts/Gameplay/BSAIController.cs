using System.Collections.Generic;
using UnityEngine;

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
    private List<List<Card>> currentHands;

    public static BSAIController instance;
    private Dictionary<CardRank, int> humanPlayerKnowledge;
    private bool hasValidPeekData = false;
    private float peekDataConfidence = 1.0f;
    
    // Cheat tracking
    private Dictionary<int, int> lastCheatTurn = new Dictionary<int, int>(); // playerIndex -> turn number
    private int lastGlobalCheatTurn = -999; // Last turn ANY monkey cheated
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
            lastCheatTurn[i] = -999; // Initialize with very negative number
            
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
        currentTurnNumber++; // Increment turn counter
    }

    // Call this when a monkey successfully peeks (cheats)
    public void OnMonkeyCheatUsed(int monkeyIndex)
    {
        lastCheatTurn[monkeyIndex] = currentTurnNumber;
        lastGlobalCheatTurn = currentTurnNumber;
        Debug.Log($"Monkey {monkeyIndex} cheated on turn {currentTurnNumber}");
    }

    // Check if a monkey can cheat right now
    public bool CanMonkeyCheat(int monkeyIndex)
    {
        // Check individual cooldown
        if (lastCheatTurn.ContainsKey(monkeyIndex))
        {
            int turnsSinceLastCheat = currentTurnNumber - lastCheatTurn[monkeyIndex];
            if (turnsSinceLastCheat < minTurnsBetweenCheats)
            {
                Debug.Log($"Monkey {monkeyIndex} on individual cooldown ({turnsSinceLastCheat}/{minTurnsBetweenCheats} turns)");
                return false;
            }
        }
        
        // Check global cooldown
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
    
    bool ShouldAICallSimple(int aiPlayer, int targetPlayer)
    {
        return ShouldAICall(aiPlayer, targetPlayer, lastExpectedRank, lastCardsPlayed, currentHands);
    }

    bool ShouldAICall(int aiPlayer, int targetPlayer, CardRank expectedRank, int cardsPlayed, List<List<Card>> hands)
    {
        if (targetPlayer == BSGameLogic.humanPlayerIndex && hasValidPeekData)
        {
            int peekedCardsOfRank = 0;
            if (humanPlayerKnowledge.ContainsKey(expectedRank))
            {
                peekedCardsOfRank = humanPlayerKnowledge[expectedRank];
            }

            // If we peeked and they claimed to have cards they don't have (according to peek)
            if (peekedCardsOfRank == 0 && peekDataConfidence > 0.7f)
            {
                Debug.Log($"AI {aiPlayer}: I peeked and human doesn't have {expectedRank}! Calling BS with high confidence.");
                return Random.value < 0.85f;
            }

            // If peek shows they DO have those cards, less likely to call
            if (peekedCardsOfRank >= cardsPlayed)
            {
                Debug.Log($"AI {aiPlayer}: Peek data shows human likely has {expectedRank}. Not calling.");
                return Random.value < 0.05f;
            }
        }
        
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

        int targetHandSize = hands[targetPlayer].Count;
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

        float randomRoll = Random.value;
        bool willCall = randomRoll < finalProbability;
        return willCall;
    }
    
    public void MonkeyPeekedAtHuman(int monkeyIndex, string[] peekedCards, bool isRealData)
    {
        // Store the peeked information
        humanPlayerKnowledge = new Dictionary<CardRank, int>();
        
        foreach (string cardStr in peekedCards)
        {
            CardRank rank = ParseCardRank(cardStr);
            if (!humanPlayerKnowledge.ContainsKey(rank))
                humanPlayerKnowledge[rank] = 0;
            humanPlayerKnowledge[rank]++;
        }
        
        hasValidPeekData = true;
        peekDataConfidence = isRealData ? 1.0f : 0.6f; // ORIGINAL: Lower confidence for fake data
        
        Debug.Log($"Monkey {monkeyIndex} peeked at human. Saw: {string.Join(", ", peekedCards)} (Real: {isRealData}, Confidence: {peekDataConfidence})");
    }

    CardRank ParseCardRank(string cardStr)
    {
        switch(cardStr)
        {
            case "A": return CardRank.Ace;
            case "2": return CardRank.Two;
            case "3": return CardRank.Three;
            case "4": return CardRank.Four;
            case "5": return CardRank.Five;
            case "6": return CardRank.Six;
            case "7": return CardRank.Seven;
            case "8": return CardRank.Eight;
            case "9": return CardRank.Nine;
            case "10": return CardRank.Ten;
            case "J": return CardRank.Jack;
            case "Q": return CardRank.Queen;
            case "K": return CardRank.King;
            default: return CardRank.Ace;
        }
    }
}