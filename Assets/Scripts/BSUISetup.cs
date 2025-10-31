using UnityEngine;
using TMPro;

/// <summary>
/// Helper script to set up BS game UI
/// Attach this to a GameObject and use it to configure the UI manager
/// </summary>
public class BSUISetup : MonoBehaviour
{
    [Header("UI Text References")]
    public TextMeshProUGUI player0Text;
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;
    public TextMeshProUGUI player3Text;
    public TextMeshProUGUI gameInfoText;
    
    [Header("Game References")]
    public BSInterface bsInterface;
    public BSSimpleUIManager uiManager;
    
    [ContextMenu("Setup UI Manager")]
    public void SetupUIManager()
    {
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<BSSimpleUIManager>();
            if (uiManager == null)
            {
                Debug.LogError("BSSimpleUIManager not found! Create one first.");
                return;
            }
        }
        
        // Assign the text references
        uiManager.player0Text = player0Text;
        uiManager.player1Text = player1Text;
        uiManager.player2Text = player2Text;
        uiManager.player3Text = player3Text;
        uiManager.gameInfoText = gameInfoText;
        uiManager.bsInterface = bsInterface;
        
        Debug.Log("UI Manager configured successfully!");
    }
    
    [ContextMenu("Test UI Update")]
    public void TestUIUpdate()
    {
        if (uiManager != null)
        {
            uiManager.UpdateAllText();
            Debug.Log("UI updated!");
        }
        else
        {
            Debug.LogError("UI Manager not found!");
        }
    }
}
