using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BSAICheat : MonoBehaviour
{
    [Header("Deflect UI")]
    public GameObject deflectUI;
    public TextMeshProUGUI alertText;
    public Image fillCircle;
    public Button clickButton; 
    public TextMeshProUGUI resultText;
    
    [Header("Deflect Settings")]
    public float deflectTimeLimit = 3f; 
    public int clicksNeeded = 15;
    public float minPeekInterval = 10f;
    public float maxPeekInterval = 15f;
    public float minResultDisplayTime = 2f;
    public bool autoStart = true;
    
    [Header("Scoring")]
    public int deflectSuccessPoints = 10;
    private int currentClicks;
    private float deflectTimer;
    private bool isDeflecting;
    private int peekingMonkeyIndex;
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
        if (deflectUI != null)
            deflectUI.SetActive(false);
        
        if (clickButton != null)
            clickButton.onClick.AddListener(OnDeflectClick);
        
        gameActive = false;
        
        if (autoStart)
        {
            Debug.Log("BSAICheat: Auto-starting peek system");
            StartPeekSystem();
        }
    }

    void Update()
    {
        if (isDeflecting)
        {
            deflectTimer -= Time.deltaTime;
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnDeflectClick();
            }
            
            if (deflectTimer <= 0)
            {
                DeflectFailed();
            }
        }
    }

    public void StartPeekSystem()
    {
        if (gameActive)
        {
            Debug.LogWarning("BSAICheat: Peek system already active!");
            return;
        }
        
        gameActive = true;
        Debug.Log("BSAICheat: Starting peek system");
        StartCoroutine(MonkeyPeekAttemptRoutine());
    }

    public void StopPeekSystem()
    {
        Debug.Log("BSAICheat: Stopping peek system");
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
            float secondsToWait = Random.Range(minPeekInterval, maxPeekInterval);
            Debug.Log($"BSAICheat: Next monkey peek attempt in {secondsToWait:F1} seconds");
            yield return new WaitForSeconds(secondsToWait);
        
            if (!isDeflecting && gameActive) 
            {
                int randomMonkey = Random.Range(1, 5);
                
                if (BSAIController.instance != null && BSAIController.instance.CanMonkeyCheat(randomMonkey))
                {
                    Debug.Log($"BSAICheat: Monkey {randomMonkey} attempting to peek!");
                    StartDeflectMinigame(randomMonkey);
                    
                    while (isDeflecting)
                    {
                        yield return null;
                    }
                }
                else
                {
                    Debug.Log($"BSAICheat: Monkey {randomMonkey} is on cooldown, skipping peek attempt");
                }
            }
        }
        
        Debug.Log("BSAICheat: Peek routine ended");
    }

    void StartDeflectMinigame(int monkeyIndex)
    {
        isDeflecting = true;
        peekingMonkeyIndex = monkeyIndex;
        currentClicks = 0;
        deflectTimer = deflectTimeLimit;
        
        if (deflectUI != null)
            deflectUI.SetActive(true);
        
        if (alertText != null)
        {
            alertText.text = $"Monkey {monkeyIndex} is trying to peek!\nPress SPACE {clicksNeeded} times!";
        }
        
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
        
        if (resultText != null)
            resultText.text = "";
        
        Debug.Log($"BSAICheat: Monkey {monkeyIndex} is trying to peek! Press SPACE {clicksNeeded} times in {deflectTimeLimit} seconds!");
    }

    void OnDeflectClick()
    {
        if (!isDeflecting)
            return;
        
        currentClicks++;
        
        if (fillCircle != null)
            fillCircle.fillAmount = (float)currentClicks / clicksNeeded;
        
        Debug.Log($"BSAICheat: Click {currentClicks}/{clicksNeeded}");
        
        if (currentClicks >= clicksNeeded)
        {
            DeflectSuccess();
        }
    }

    void DeflectSuccess()
    {
        Debug.Log("BSAICheat: Deflect SUCCESS!");
        isDeflecting = false;
        
        if (resultText != null)
            resultText.text = "DEFLECTED!";
        
        string[] allRanks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
        string[] fakeCards = new string[3];
        for (int i = 0; i < 3; i++)
        {
            fakeCards[i] = allRanks[Random.Range(0, allRanks.Length)];
        }
        
        Debug.Log($"BSAICheat: DEFLECTED! Monkey {peekingMonkeyIndex} now thinks you have: {string.Join(", ", fakeCards)}");
        
        if (BSAIController.instance != null)
        {
            BSAIController.instance.MonkeyPeekedAtHuman(peekingMonkeyIndex, fakeCards, false);
        }
        
        Debug.Log($"BSAICheat: Deflect bonus! +{deflectSuccessPoints} points");

        StartCoroutine(HideDeflectUI());
    }

    void DeflectFailed()
    {
        Debug.Log("BSAICheat: Deflect FAILED!");
        isDeflecting = false;
        
        if (resultText != null)
            resultText.text = "THEY SAW YOUR CARDS!";
        
        if (BSGameLogic.instance == null)
        {
            Debug.LogError("BSAICheat: BSGameLogic.instance is null!");
            StartCoroutine(HideDeflectUI());
            return;
        }
        
        List<Card> humanHand = BSGameLogic.instance.GetHand(BSGameLogic.humanPlayerIndex);
        
        string[] realCards = new string[humanHand.Count];
        for (int i = 0; i < humanHand.Count; i++)
        {
            realCards[i] = GetRankString(humanHand[i].rank);
        }
        
        Debug.Log($"BSAICheat: FAILED TO DEFLECT! Monkey {peekingMonkeyIndex} saw your real cards: {string.Join(", ", realCards)}");
        
        if (BSAIController.instance != null)
        {
            BSAIController.instance.MonkeyPeekedAtHuman(peekingMonkeyIndex, realCards, true);
            BSAIController.instance.OnMonkeyCheatUsed(peekingMonkeyIndex);
        }

        StartCoroutine(HideDeflectUI());
    }

    IEnumerator HideDeflectUI()
    {
        yield return new WaitForSeconds(minResultDisplayTime);

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