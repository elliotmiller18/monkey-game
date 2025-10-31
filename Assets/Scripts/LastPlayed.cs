using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class LastPlayed : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] GameObject BSButton;

    public static LastPlayed instance;

    void Awake()
    {
        Assert.IsNotNull(text, "text shouldn't be null!");
        if (instance != null && instance != this)
        {
            Debug.LogError("duplicate lastplayed, destroying");
            Destroy(gameObject);
            return;
        }
        instance = this;
        HideTextAndButton();
    }

    public void UpdateText(int playerIndex, int cardsPlayed, CardRank rank)
    {
        Assert.IsFalse(cardsPlayed < 1, "can't update with less than 1 card played");
        string end = cardsPlayed + " " + CardUtils.RankToString(rank) + (cardsPlayed > 1 ? "s" : "");
        string start;

        if (playerIndex == 0)
        {
            BSButton.SetActive(false);
            start = "You claimed";
        }
        else
        {
            BSButton.SetActive(true);
            start = "Monkey " + playerIndex + " claimed";
        }

        text.text = start + "\n" + end;
    }

    public void HideTextAndButton()
    {
        text.text = "";
        BSButton.SetActive(false);
    }
}
