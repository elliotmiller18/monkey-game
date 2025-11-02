using System.Collections.Generic;
using UnityEngine;

public class BSAIController : MonoBehaviour
{
    [Header("AI Call Settings")]
    [SerializeField] float aiCallDelayMin = 0.5f;
    [SerializeField] float aiCallDelayMax = 2.0f;
    private List<Dictionary<CardRank, int>> aiKnowledge;
    private int numPlayers;
    private CardRank lastExpectedRank;
    private int lastCardsPlayed;
    private List<List<Card>> currentHands;

    public static BSAIController instance;

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
        currentHands = hands;
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
}