using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TurnIndicator : MonoBehaviour
{
    [SerializeField] List<Sprite> cardSprites;
    [SerializeField] List<GameObject> monkeys;
    [SerializeField] float y_offset = 1;
    [SerializeField] float rps = 1;
    [SerializeField] float cardShowTime = 1f;

    [HideInInspector] public static TurnIndicator instance;

    bool cardShowing = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Duplicate turn indicator " + gameObject.name + " destroying");
            Destroy(gameObject);
        }
        instance = this;
    }

    void Start()
    {
        Assert.IsNotNull(GameManager.instance);
        Assert.IsNotNull(monkeys);
        Assert.AreEqual(cardSprites.Count, 13);
    }

    void Update()
    {
        if (GameManager.instance.currentPlayerIndex == 0) GetComponent<MeshRenderer>().enabled = false;
        else
        {
            if(!cardShowing) GetComponent<MeshRenderer>().enabled = true;
            // the list of monkeys is 1 less than the number of players
            Vector3 new_pos = monkeys[GameManager.instance.currentPlayerIndex - 1].transform.position;
            new_pos.y += y_offset;
            transform.position = new_pos;
        }
        // rotate
        transform.Rotate(Vector3.up * rps * Time.deltaTime * 360);
    }

    IEnumerator SpawnCard(int monkeyIndex)
    {
        cardShowing = true;
        List<Card> hand = BSInterface.instance.GetMonkeyHand(monkeyIndex);

        if (hand.Count == 0)
        {
            cardShowing = false;
            yield break;
        }

        int randIndex = Random.Range(0, hand.Count);
        Sprite cardSprite = cardSprites[(int)hand[randIndex].rank - 1];

        GameObject cardInstance = new GameObject("CardSprite");
        Vector3 new_pos = monkeys[monkeyIndex].transform.position;
        new_pos.y += y_offset;
        cardInstance.transform.position = new_pos;
        SpriteRenderer sr = cardInstance.AddComponent<SpriteRenderer>();
        sr.sprite = cardSprite;
        // Optionally, set sorting layer/order if needed
        // sr.sortingOrder = 10; 

        GetComponent<MeshRenderer>().enabled = false;

        yield return new WaitForSeconds(cardShowTime);

        Destroy(cardInstance);
        cardShowing = false;
    }
    
    public void RevealCard(int monkeyIndex)
    {
        StartCoroutine(SpawnCard(monkeyIndex));
    }
}
