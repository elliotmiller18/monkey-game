using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class MonkeyHands : MonoBehaviour
{
    [SerializeField] List<TMP_Text> monkeyCardCounts;

    [SerializeField] List<Transform> MonkeyPositions;
    [SerializeField] Transform table;
    [SerializeField] GameObject cardBackSpritePrefab;

    Dictionary<int, GameObject> renderedHands = new Dictionary<int, GameObject>();

    public static MonkeyHands instance;

    void Awake()
    {
        Assert.IsFalse(monkeyCardCounts.Count == 0, "You forgot to assign the monkeyCardCounts in the BSUI gameobject");
        if (instance != null && instance != this)
        {
            Debug.LogError("duplicate MonkeyHands, destroying gameobject");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Update()
    {
        for (int i = 0; i < monkeyCardCounts.Count; i++)
        {
            monkeyCardCounts[i].text = BSGameLogic.instance.GetHand(i + 1).Count.ToString();
        }
    }

    GameObject GenerateHandModel(int numCards)
    {
        GameObject parent = new GameObject("monkeyhand");
        float width = cardBackSpritePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;

        float xOffset = numCards * width;
        xOffset = -xOffset / 2;
        for (int i = 0; i < numCards; i++)
        {
            GameObject card = Instantiate(cardBackSpritePrefab, parent.transform);
            card.transform.localPosition = new Vector3(xOffset, 0, 0);
            if (i < numCards - 1)
            {
                GameObject c2 = Instantiate(cardBackSpritePrefab, parent.transform);
                c2.transform.localPosition = new Vector3(xOffset, cardBackSpritePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.y, 0);
                i++;
            }
            xOffset += width;
        }
        return parent;
    }
    
    // 
    public void RenderMonkeyHand(int monkeyIndex)
    {
        if (renderedHands.ContainsKey(monkeyIndex))
        {
            Destroy(renderedHands[monkeyIndex]);
        }

        Transform monkeyTransform = MonkeyPositions[monkeyIndex];
        GameObject cardModel = GenerateHandModel(BSGameLogic.instance.GetMonkeyHand(monkeyIndex).Count);
        SphereCollider sphere = monkeyTransform.GetComponent<SphereCollider>();

        // Make the cards face the table
        cardModel.transform.LookAt(table.position);
        cardModel.transform.Rotate(90, 0, 0); // fix 2D sprite orientation

        // Move them outward from the monkey
        float radius = sphere.radius * Mathf.Max(monkeyTransform.lossyScale.x, monkeyTransform.lossyScale.y, monkeyTransform.lossyScale.z);
        cardModel.transform.position = monkeyTransform.position - cardModel.transform.forward * radius;

        renderedHands[monkeyIndex] = cardModel;
    }
}
