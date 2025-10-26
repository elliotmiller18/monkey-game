using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CenterCard : MonoBehaviour
{
    Image img;
    TMP_Text counterText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        counterText = GetComponentInChildren<TMP_Text>();
        counterText.enabled = false;
        img = GetComponent<Image>();
        img.enabled = false;
    }

    public void SwitchImage(CardRank r, int count)
    {
        counterText.enabled = true;
        if (count < 1) counterText.text = "";
        else counterText.text = "X" + count;
        img.enabled = true;
        img.sprite = TurnIndicator.instance.cardSprites[CardUtils.CardToIndex(r)];
    }
}
