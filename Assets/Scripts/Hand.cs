using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour
{
    
    void Update()
    {
        string hand = "";
        if (GameManager.instance.GetPlayerHand() == null) hand = "waiting on game ...";
        else
        {
            foreach (Card card in GameManager.instance.GetPlayerHand())
            {
                hand += card.ToString() + " ";
            }
        }
        
        hand.Replace("\n", "");
        GetComponent<TextMeshProUGUI>().text = hand;
    }
}
