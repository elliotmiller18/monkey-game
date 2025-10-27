using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonkeyBSGame : MonoBehaviour
{
    [System.Serializable]
    public class Monkey
    {
        public GameObject monkeyObject;
        public string[] cards;
        public bool isLookingAway;
        public Renderer monkeyRenderer;
    }

    [Header("Monkey Setup")]
    public List<Monkey> monkeys = new List<Monkey>();

    [Header("Colors")]
    public Color lookingColor = new Color(0.65f, 0.4f, 0.2f);
    public Color lookingAwayColor = Color.green;

    [Header("Timing")]
    public float minLookAwayInterval = 2f;
    public float maxLookAwayInterval = 4f;
    public float minLookAwayDuration = 1.5f;
    public float maxLookAwayDuration = 3f;

    private Monkey currentlyPeekingAt;
    private bool isPeeking;
    private int score;
    private int strikes;
    private int maxStrikes = 3;
    private bool gameOver;

    public static MonkeyBSGame instance;

    void Awake()
    {
        if (instance != this && instance != null)
        {
            Debug.LogError("Duplicate MonkeyBSGame, destroying attached gameobject");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        foreach (var monkey in monkeys)
        {
            monkey.monkeyRenderer = monkey.monkeyObject.GetComponent<Renderer>();
            
            if (monkey.monkeyRenderer == null)
            {
                Debug.LogError($"No Renderer found on {monkey.monkeyObject.name}!");
                continue;
            }
            
            monkey.isLookingAway = false;
            monkey.monkeyRenderer.material.color = lookingColor;
            
            if (monkey.cards == null || monkey.cards.Length == 0)
            {
                monkey.cards = new string[] { "A♠", "K♥", "3♦" };
            }
        }

        strikes = 0;
        gameOver = false;

        StartCoroutine(RandomLookAwayRoutine());
        Debug.Log($"Starting game! Score: {score} | Strikes: {strikes}/{maxStrikes}");
    }

    void Update()
    {
        if (!gameOver)
        {
            HandlePeekingInput();
        }
    }

    IEnumerator RandomLookAwayRoutine()
    {
        while (!gameOver)
        {
            float waitTime = Random.Range(minLookAwayInterval, maxLookAwayInterval);
            yield return new WaitForSeconds(waitTime);

            List<Monkey> availableMonkeys = monkeys.FindAll(m => !m.isLookingAway);
            
            if (availableMonkeys.Count > 0)
            {
                Monkey randomMonkey = availableMonkeys[Random.Range(0, availableMonkeys.Count)];
                StartCoroutine(MonkeyLookAway(randomMonkey));
            }
        }
    }

    IEnumerator MonkeyLookAway(Monkey monkey)
    {
        monkey.isLookingAway = true;
        monkey.monkeyRenderer.material.color = lookingAwayColor;

        float duration = Random.Range(minLookAwayDuration, maxLookAwayDuration);
        yield return new WaitForSeconds(duration);

        monkey.isLookingAway = false;
        monkey.monkeyRenderer.material.color = lookingColor;

        if (isPeeking && currentlyPeekingAt == monkey)
        {
            CaughtCheating();
        }
    }

    void HandlePeekingInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                int clickedMonkeyIndex = monkeys.FindIndex(m => m.monkeyObject == hit.collider.gameObject);
                
                if (monkeys[clickedMonkeyIndex] != null)
                {
                    if (monkeys[clickedMonkeyIndex].isLookingAway)
                    {
                        StartPeeking(clickedMonkeyIndex);
                    }
                    else
                    {
                        ClickedWhileLooking();
                    }
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isPeeking)
            {
                StopPeeking();
            }
        }
    }

    void StartPeeking(int monkeyIndex)
    {
        isPeeking = true;
        currentlyPeekingAt = monkeys[monkeyIndex];

        TurnIndicator.instance.RevealCard(monkeyIndex);
        Debug.Log($"Peeking at monkey with cards: {string.Join(", ", BSGameLogic.instance.GetHand(monkeyIndex + 1))}");
    }

    void StopPeeking()
    {
        if (currentlyPeekingAt != null && currentlyPeekingAt.isLookingAway)
        {
            score += 5;
            Debug.Log($"Successfully peeked! +5 points. Total Score: {score} | Strikes: {strikes}/{maxStrikes}");
        }

        isPeeking = false;
        currentlyPeekingAt = null;
    }

    void ClickedWhileLooking()
    {
        strikes++;
        Debug.Log($"STRIKE! Clicked on a monkey that was looking! Strikes: {strikes}/{maxStrikes}");

        if (strikes >= maxStrikes)
        {
            GameOver();
        }
    }

    void CaughtCheating()
    {
        strikes++;
        Debug.Log($"CAUGHT CHEATING! Strike added. Strikes: {strikes}/{maxStrikes}");
        
        isPeeking = false;
        currentlyPeekingAt = null;

        if (strikes >= maxStrikes)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(FlashCaughtWarning());
        }
    }

    void GameOver()
    {
        BSGameLogic.instance.EndGame();
        gameOver = true;
        Debug.Log($"=== GAME OVER === Final Score: {score} | You got {maxStrikes} strikes!");
        
        foreach (var monkey in monkeys)
        {
            monkey.monkeyRenderer.material.color = Color.red;
        }
    }

    IEnumerator FlashCaughtWarning()
    {
        foreach (var monkey in monkeys)
        {
            monkey.monkeyRenderer.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.3f);

        foreach (var monkey in monkeys)
        {
            monkey.monkeyRenderer.material.color = monkey.isLookingAway ? lookingAwayColor : lookingColor;
        }
    }

    public int GetStrikes()
    {
        return strikes;
    }

    public int GetMaxStrike()
    {
        return maxStrikes;
    }

    public void RestartGame()
    {
        // 1. Stop all active coroutines (RandomLookAwayRoutine, MonkeyLookAway, etc.)
        StopAllCoroutines();

        // 2. Reset game state variables
        score = 0;
        strikes = 0;
        gameOver = false;
        isPeeking = false;
        currentlyPeekingAt = null;

        // 3. Reset all monkeys to their starting state
        foreach (var monkey in monkeys)
        {
            if (monkey.monkeyRenderer != null)
            {
                monkey.isLookingAway = false;
                monkey.monkeyRenderer.material.color = lookingColor;
            }
        }

        // 5. Start the main coroutine again
        StartCoroutine(RandomLookAwayRoutine());
        
        Debug.Log($"--- GAME RESTARTED --- Score: {score} | Strikes: {strikes}/{maxStrikes}");
    }
}