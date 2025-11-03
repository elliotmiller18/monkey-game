using TMPro;
using UnityEngine;

public class Pile : MonoBehaviour
{
    [SerializeField] Sprite cardBack;
    [SerializeField] GameObject pileParent;

    int order = 1;
    float offsetRange = 0.04f;

    public static Pile instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("duplicate pile, destroying");
            return;
        }
        instance = this;
    }

    public void AddCards(int numCards)
    {
        for (int i = 0; i < numCards; i++)
        {
            GameObject card = new GameObject("pileCard");
            SpriteRenderer sr = card.AddComponent<SpriteRenderer>();
            sr.sprite = cardBack;
            sr.sortingOrder = order;
            ++order;

            card.transform.SetParent(pileParent.transform);
            card.transform.localPosition = new Vector3(Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange), Random.Range(-offsetRange, offsetRange));
            // card.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        }

        TMP_Text text = pileParent.GetComponentInChildren<TMP_Text>();
        // order starts at 1 
        text.text = (order - 1).ToString();
        text.GetComponent<MeshRenderer>().sortingOrder = order + 1;
    }
    
    public void ClearPile()
    {
        order = 1;
        foreach (Transform child in pileParent.transform)
        {
            // if it doesn't have TMP_Text destroy it
            if (child.GetComponent<TMP_Text>() == null) Destroy(child.gameObject);
        }
        pileParent.GetComponentInChildren<TMP_Text>().text = 0.ToString();
    }
}
