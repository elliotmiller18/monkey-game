using TMPro;
using UnityEngine;

public class BSUi : MonoBehaviour
{
    [SerializeField] TMP_Text CallStatusText;
    [SerializeField] TMP_Text ContinueWarning;

    void Start()
    {
        ContinueWarning.text = "";
    }

    public void ContinueClick()
    {
        ContinueWarning.text = "";
        GameState state = BSGameLogic.instance.GetState();
        if (state == GameState.LieTold || state == GameState.TruthTold)
        {
            BSGameLogic.instance.Continue();
            CallStatusText.text = "";
        }
        else if(state == GameState.WaitingForPlay && BSGameLogic.instance.GetPlayer() == 0)
        {
            ContinueWarning.text = "You need to play\na card !";
        }
    }

    public void BSClick()
    {
        ContinueWarning.text = "";
        GameState state = BSGameLogic.instance.GetState();
        if ((state == GameState.LieTold || state == GameState.TruthTold) && BSGameLogic.instance.GetPlayer() != 0)
        {
            BSGameLogic.instance.Call(BSGameLogic.humanPlayerIndex);
            CallStatusText.text = state == GameState.LieTold ? "Call successful!" : "Call failed..";
        }
        else if((state == GameState.LieTold || state == GameState.TruthTold) && BSGameLogic.instance.GetPlayer() == 0)
        {
            CallStatusText.text = "You can't call BS on yourself.";
        }
    }
}
