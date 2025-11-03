using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MonkeyObjects : MonoBehaviour
{
    public static List<GameObject> monkeys { get; private set; }

    public static GameObject GetMonkey(int i)
    {
        return monkeys[i];
    }

    public static int NumMonkeys()
    {
        return monkeys.Count;
    }

    void Awake()
    {
        Assert.IsNull(monkeys, "global monkeys array already set, probably means duplicate MonkeyObjects script (did you duplicate the Monkeys parent?)");
        
        monkeys = new List<GameObject>();
        foreach (Transform child in gameObject.transform)
        {
            monkeys.Add(child.gameObject);
        }
    }
}
