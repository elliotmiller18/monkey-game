using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    [SerializeField] private Button button;
    void Update()
    {
        button.gameObject.SetActive(BSGameLogic.instance.IsHumanTurn());
    }
}
