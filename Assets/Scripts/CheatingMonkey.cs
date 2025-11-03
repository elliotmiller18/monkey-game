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
        public Quaternion originalRotation;
    }

    [Header("Monkey Setup")]
    public List<Monkey> monkeys = new List<Monkey>();

    [Header("Rotation Settings")]
    public float rotationDegrees = 90f;
    public float rotationSpeed = 5f;

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
            Destroy(gameObject);
            return;
        }
        instance = this;
    
        if (monkeys != null)
        {
            foreach (var monkey in monkeys)
            {
                if (monkey != null && monkey.monkeyObject != null)
                {
                    monkey.originalRotation = monkey.monkeyObject.transform.rotation;
                }
            }
        }
    }

    void Start()
    {
        // Check if monkeys list is empty
        if (monkeys == null || monkeys.Count == 0)
        {
            Debug.LogError("No monkeys assigned! Add monkeys to the list in the Inspector.");
            return;
        }

        // Remove any null entries and validate
        monkeys.RemoveAll(m => m == null || m.monkeyObject == null);

        if (monkeys.Count == 0)
        {
            Debug.LogError("All monkey entries are null! Assign GameObjects in the Inspector.");
            return;
        }

        for (int i = 0; i < monkeys.Count; i++)
        {
            var monkey = monkeys[i];
            
            monkey.isLookingAway = false;
            
            if (monkey.cards == null || monkey.cards.Length == 0)
            {
                monkey.cards = new string[] { "A♠", "K♥", "3♦" };
            }
            
            Debug.Log($"Monkey {i} ({monkey.monkeyObject.name}) initialized successfully");
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

            List<Monkey> availableMonkeys = monkeys.FindAll(m => m != null && m.monkeyObject != null && !m.isLookingAway);
            
            if (availableMonkeys.Count > 0)
            {
                Monkey randomMonkey = availableMonkeys[Random.Range(0, availableMonkeys.Count)];
                StartCoroutine(MonkeyLookAway(randomMonkey));
            }
        }
    }

    IEnumerator MonkeyLookAway(Monkey monkey)
    {
        if (monkey == null || monkey.monkeyObject == null)
        {
            Debug.LogWarning("Attempted to rotate a null monkey");
            yield break;
        }

        monkey.isLookingAway = true;
        
        // Randomly choose left or right rotation
        float direction = Random.value > 0.5f ? 1f : -1f;
        Quaternion targetRotation = monkey.originalRotation * Quaternion.Euler(0, rotationDegrees * direction, 0);
        
        // Smoothly rotate to look away
        float elapsed = 0;
        Quaternion startRotation = monkey.monkeyObject.transform.rotation;
        while (elapsed < 1f / rotationSpeed)
        {
            if (monkey.monkeyObject == null) yield break; // Safety check
            
            elapsed += Time.deltaTime;
            monkey.monkeyObject.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed * rotationSpeed);
            yield return null;
        }
        
        if (monkey.monkeyObject != null)
        {
            monkey.monkeyObject.transform.rotation = targetRotation;
        }

        float duration = Random.Range(minLookAwayDuration, maxLookAwayDuration);
        yield return new WaitForSeconds(duration);

        if (monkey.monkeyObject == null) yield break; // Safety check

        // Smoothly rotate back to original position
        elapsed = 0;
        startRotation = monkey.monkeyObject.transform.rotation;
        while (elapsed < 1f / rotationSpeed)
        {
            if (monkey.monkeyObject == null) yield break; // Safety check
            
            elapsed += Time.deltaTime;
            monkey.monkeyObject.transform.rotation = Quaternion.Slerp(startRotation, monkey.originalRotation, elapsed * rotationSpeed);
            yield return null;
        }
        
        if (monkey.monkeyObject != null)
        {
            monkey.monkeyObject.transform.rotation = monkey.originalRotation;
        }
        
        monkey.isLookingAway = false;

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

            // Raycast against everything EXCEPT the table layer
            int ignoreTableLayer = ~(1 << LayerMask.NameToLayer("Table")); // Ignores "Table" layer
        
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ignoreTableLayer))
            {
                int clickedMonkeyIndex = monkeys.FindIndex(m => m != null && m.monkeyObject == hit.collider.gameObject);
            
                if (clickedMonkeyIndex >= 0 && clickedMonkeyIndex < monkeys.Count)
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

        // Check if TurnIndicator exists and isn't destroyed
        if (TurnIndicator.instance != null && TurnIndicator.instance)
        {
            TurnIndicator.instance.RevealCard(monkeyIndex);
        }

        if (BSGameLogic.instance != null && BSGameLogic.instance)
        {
            Debug.Log($"Peeking at monkey with cards: {string.Join(", ", BSGameLogic.instance.GetHand(monkeyIndex + 1))}");
        }
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
    }

    void GameOver()
    {
        if (BSGameLogic.instance != null && BSGameLogic.instance)
        {
            BSGameLogic.instance.EndGame();
        }
        
        gameOver = true;
        Debug.Log($"=== GAME OVER === Final Score: {score} | You got {maxStrikes} strikes!");
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
            if (monkey != null && monkey.monkeyObject != null)
            {
                monkey.isLookingAway = false;
                // Reset rotation
                monkey.monkeyObject.transform.rotation = monkey.originalRotation;
            }
        }

        // 5. Start the main coroutine again
        StartCoroutine(RandomLookAwayRoutine());
        
        Debug.Log($"--- GAME RESTARTED --- Score: {score} | Strikes: {strikes}/{maxStrikes}");
    }
    
    public void PlayCardAnimation(int monkeyIndex)
    {
        if (monkeyIndex >= 0 && monkeyIndex < monkeys.Count)
        {
            GameObject monkeyObj = monkeys[monkeyIndex].monkeyObject;
            if (monkeyObj != null)
            {
                Animation anim = monkeyObj.GetComponent<Animation>();
                if (anim != null)
                {
                    anim.Play();
                }
            }
        }
    }
}