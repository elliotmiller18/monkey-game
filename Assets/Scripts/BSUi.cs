using TMPro;
using UnityEngine;

public class BSUi : MonoBehaviour
{
    [SerializeField] TMP_Text CallStatusText;
    [SerializeField] TMP_Text ContinueWarning;
    [SerializeField] GameObject ContinueButton;

    void Start()
    {
        ContinueWarning.text = "";
    }

    void Update()
    {
        GameState state = BSGameLogic.instance.GetState();
        ContinueButton.SetActive(state == GameState.TruthTold || state == GameState.LieTold);
        if(state == GameState.TruthTold || state == GameState.LieTold)
        {
            ContinueButton.SetActive(true);
            ContinueWarning.text = "";
        } else
        {
            ContinueButton.SetActive(false);
            // could also be inactive
            if(state == GameState.WaitingForPlay) ContinueWarning.text = "You need to play\na " + CardUtils.RankToString(BSGameLogic.instance.GetExpectedRank());
        }
    }

    public void ContinueClick()
    {
        GameState state = BSGameLogic.instance.GetState();
        if (state == GameState.LieTold || state == GameState.TruthTold)
        {
            BSGameLogic.instance.Continue();
            CallStatusText.text = "";
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
