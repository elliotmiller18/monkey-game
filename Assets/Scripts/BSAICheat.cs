using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BSAICheat : MonoBehaviour
{
    [Header("Deflect UI")]
    public GameObject deflectUI; // Parent panel for the deflect minigame
    public TextMeshProUGUI alertText; // Text showing "[Monkey] is trying to peek!"
    public Image fillCircle; // The circle fill meter (use Image with Fill Type: Radial 360)
    public Button clickButton; // The clickable circle button
    public TextMeshProUGUI resultText; // Shows "DEFLECTED!" or "THEY SAW YOUR CARDS!"
    
    [Header("Deflect Settings")]
    public float deflectTimeLimit = 3f; // Seconds player has to fill the circle
    public int clicksNeeded = 15; // Number of clicks needed to deflect
    public float minPeekInterval = 45f; // Minimum time between peek attempts
    public float maxPeekInterval = 120f; // Maximum time between peek attempts
    
    [Header("Scoring")]
    public int deflectSuccessPoints = 10;
    private int currentClicks;
    private float deflectTimer;
    private bool isDeflecting;
    private int peekingMonkeyIndex; // Which monkey is peeking (1-3)
    private bool gameActive;

    public static BSAICheat instance;

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
        // Setup deflect UI
        if (deflectUI != null)
            deflectUI.SetActive(false);
        
        if (clickButton != null)
            clickButton.onClick.AddListener(OnDeflectClick);
        
        gameActive = false;
    }

    void Update()
    {
        // Handle deflect minigame timer
        if (isDeflecting)
        {
            deflectTimer -= Time.deltaTime;
            
            if (deflectTimer <= 0)
            {
                DeflectFailed();
            }
        }
    }

    public void StartPeekSystem()
    {
        gameActive = true;
        StartCoroutine(MonkeyPeekAttemptRoutine());
    }

    public void StopPeekSystem()
    {
        gameActive = false;
        StopAllCoroutines();
        
        if (deflectUI != null)
            deflectUI.SetActive(false);
        
        isDeflecting = false;
    }

    IEnumerator MonkeyPeekAttemptRoutine()
    {
        while (gameActive)
        {
            // Wait random time before next peek attempt
            int secondsToWait = Random.Range((int)minPeekInterval, (int)maxPeekInterval);
            Debug.Log($"Next monkey peek attempt in {secondsToWait} seconds");
            yield return new WaitForSeconds(secondsToWait);
        
            
            // Don't interrupt if already deflecting
            if (!isDeflecting)
            {
                // Pick random monkey (1, 2, 4, or 3)
                int randomMonkey = Random.Range(1, 5);
                StartDeflectMinigame(randomMonkey);
            }
        }
    }

    void StartDeflectMinigame(int monkeyIndex)
    {
        isDeflecting = true;
        peekingMonkeyIndex = monkeyIndex;
        currentClicks = 0;
        deflectTimer = deflectTimeLimit;
        
        // Show UI
        if (deflectUI != null)
            deflectUI.SetActive(true);
        
        if (alertText != null)
        {
            alertText.text = $"Monkey {monkeyIndex} is trying to peek!";
        }
        
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
        
        if (resultText != null)
            resultText.text = "";
        
        Debug.Log($"Monkey {monkeyIndex} is trying to peek! Click the circle {clicksNeeded} times in {deflectTimeLimit} seconds!");
    }

    void OnDeflectClick()
    {
        if (!isDeflecting)
            return;
        
        currentClicks++;
        
        // Update fill circle
        if (fillCircle != null)
            fillCircle.fillAmount = (float)currentClicks / clicksNeeded;
        
        // Check if deflected successfully
        if (currentClicks >= clicksNeeded)
        {
            DeflectSuccess();
        }
    }

    void DeflectSuccess()
    {
        isDeflecting = false;
        
        if (resultText != null)
            resultText.text = "DEFLECTED!";
        
        // Generate 3 random fake cards
        string[] allRanks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        string[] fakeCards = new string[3];
        for (int i = 0; i < 3; i++)
        {
            fakeCards[i] = allRanks[Random.Range(0, allRanks.Length)];
        }
        
        Debug.Log($"DEFLECTED! Monkey {peekingMonkeyIndex} now thinks you have: {string.Join(", ", fakeCards)}");
        
        // Send fake cards to AI
        BSAIController.instance.MonkeyPeekedAtHuman(peekingMonkeyIndex, fakeCards, false);
        
        // Add points (you might want to integrate this with your scoring system)
        Debug.Log($"Deflect bonus! +{deflectSuccessPoints} points");
        
        StartCoroutine(HideDeflectUI());
    }

    void DeflectFailed()
    {
        isDeflecting = false;
        
        if (resultText != null)
            resultText.text = "THEY SAW YOUR CARDS!";
        
        // Get player's real cards from BSGameLogic
        List<Card> humanHand = BSGameLogic.instance.GetHand(BSGameLogic.humanPlayerIndex);
        
        // Convert to string array of ranks
        string[] realCards = new string[humanHand.Count];
        for (int i = 0; i < humanHand.Count; i++)
        {
            realCards[i] = GetRankString(humanHand[i].rank);
        }
        
        Debug.Log($"FAILED TO DEFLECT! Monkey {peekingMonkeyIndex} saw your real cards: {string.Join(", ", realCards)}");
        
        // Send real cards to AI
        BSAIController.instance.MonkeyPeekedAtHuman(peekingMonkeyIndex, realCards, true);
        
        StartCoroutine(HideDeflectUI());
    }

    IEnumerator HideDeflectUI()
    {
        yield return new WaitForSeconds(1.5f);
        
        if (deflectUI != null)
            deflectUI.SetActive(false);
    }

    string GetRankString(CardRank rank)
    {
        switch (rank)
        {
            case CardRank.Ace: return "A";
            case CardRank.Two: return "2";
            case CardRank.Three: return "3";
            case CardRank.Four: return "4";
            case CardRank.Five: return "5";
            case CardRank.Six: return "6";
            case CardRank.Seven: return "7";
            case CardRank.Eight: return "8";
            case CardRank.Nine: return "9";
            case CardRank.Ten: return "10";
            case CardRank.Jack: return "J";
            case CardRank.Queen: return "Q";
            case CardRank.King: return "K";
            default: return "?";
        }
    }
}