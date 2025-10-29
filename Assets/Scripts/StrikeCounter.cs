using TMPro;
using UnityEngine;

public class StrikeCounter : MonoBehaviour
{
    TMP_Text txt;
    int maxStrikes;
    void Start()
    {
        txt = GetComponent<TMP_Text>();
        maxStrikes = MonkeyBSGame.instance.GetMaxStrike();
    }

    void Update()
    {
        int strikes = MonkeyBSGame.instance.GetStrikes();
        txt.text = "Times caught peeking: " + strikes + "/" + maxStrikes;
        if(maxStrikes - 1 == strikes)
        {
            txt.text = txt.text + "\nIf you get caught one more time, you're out!";
        }
    }
}
