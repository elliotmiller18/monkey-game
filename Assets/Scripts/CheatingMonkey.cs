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

    [Header("UI - Optional")]
    public GameObject cardDisplayUI;

    private Monkey currentlyPeekingAt;
    private bool isPeeking;
    private int score;
    private int strikes;
    private int maxStrikes = 3;
    private bool gameOver;

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

        if (cardDisplayUI != null)
            cardDisplayUI.SetActive(false);

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

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        GUI.Label(new Rect(20, 20, 300, 50), $"Score: {score}", style);

        GUIStyle strikeStyle = new GUIStyle(style);
        strikeStyle.normal.textColor = strikes >= maxStrikes ? Color.red : (strikes >= 2 ? Color.yellow : Color.white);
        GUI.Label(new Rect(20, 60, 300, 50), $"Strikes: {strikes}/{maxStrikes}", strikeStyle);

        if (gameOver)
        {
            GUIStyle gameOverStyle = new GUIStyle();
            gameOverStyle.fontSize = 60;
            gameOverStyle.fontStyle = FontStyle.Bold;
            gameOverStyle.normal.textColor = Color.red;
            gameOverStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 100), "GAME OVER!", gameOverStyle);
            
            GUIStyle finalScoreStyle = new GUIStyle();
            finalScoreStyle.fontSize = 40;
            finalScoreStyle.fontStyle = FontStyle.Bold;
            finalScoreStyle.normal.textColor = Color.white;
            finalScoreStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width / 2 - 300, Screen.height / 2, 600, 50), $"Final Score: {score}", finalScoreStyle);
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

        if (cardDisplayUI != null)
        {
            cardDisplayUI.SetActive(true);
        }

        TurnIndicator.instance.RevealCard(monkeyIndex);
        Debug.Log($"Peeking at monkey with cards: {string.Join(", ", BSInterface.instance.GetMonkeyHand(monkeyIndex))}");
    }

    void StopPeeking()
    {
        if (currentlyPeekingAt != null && currentlyPeekingAt.isLookingAway)
        {
            score += 5;
            Debug.Log($"Successfully peeked! +5 points. Total Score: {score} | Strikes: {strikes}/{maxStrikes}");
            // it's hacky to put stuff in here but i'm just gonna thug it out yk

        }

        isPeeking = false;
        currentlyPeekingAt = null;

        if (cardDisplayUI != null)
            cardDisplayUI.SetActive(false);
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

        if (cardDisplayUI != null)
            cardDisplayUI.SetActive(false);

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
}