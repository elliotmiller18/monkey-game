using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class MonkeyHands : MonoBehaviour
{
    [SerializeField] List<TMP_Text> monkeyCardCounts;

    void Awake()
    {
        Assert.IsFalse(monkeyCardCounts.Count == 0, "You forgot to assign the monkeyCardCounts in the BSUI gameobject");
    }

    void Update()
    {
        for(int i = 0; i < monkeyCardCounts.Count; i++)
        {
            monkeyCardCounts[i].text = BSGameLogic.instance.GetHand(i + 1).Count.ToString();
        }
    }
}
