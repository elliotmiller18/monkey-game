using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CenterCard : MonoBehaviour
{
    Image img;
    TMP_Text counterText;

    void Awake()
    {
        img = GetComponent<Image>();
        img.enabled = false;
        counterText = GetComponentInChildren<TMP_Text>();
        if (counterText != null) counterText.text = "";
    }

    public void SwitchImage(CardRank r, int count)
    {
        img.enabled = true;
        if (counterText != null) counterText.text = "X" + count;
        img.sprite = TurnIndicator.instance.cardSprites[CardUtils.SuitedCardToIndex(r)];
    }
    
    public void Reset()
    {
        img.enabled = false;
        if (counterText != null) counterText.text = "";
    }
}
