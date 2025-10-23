using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Debug console for testing BS game
/// Provides console commands to test game functionality
/// </summary>
public class BSDebugConsole : MonoBehaviour
{
    [Header("Game References")]
    public BSInterface bsInterface;
    
    private List<string> commandHistory = new List<string>();
    private const int MAX_HISTORY = 20;

    void Start()
    {
        if (bsInterface == null)
            bsInterface = FindFirstObjectByType<BSInterface>();
        
        if (bsInterface == null)
        {
            Debug.LogError("BSInterface not found!");
            return;
        }
        
        PrintHelp();
    }

    void Update()
    {
        // Check for console input
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessCommand();
        }
    }

    void OnGUI()
    {
        // Simple console display
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("BS Game Debug Console");
        GUILayout.Label("Type commands and press Enter");
        GUILayout.Space(10);
        
        // Display command history
        foreach (string command in commandHistory)
        {
            GUILayout.Label(command);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Commands: help, status, hand, play [rank], callbs, pass, newgame");
        
        GUILayout.EndArea();
    }

    void ProcessCommand()
    {
        // This is a simplified version - in a real implementation you'd want proper input handling
        // For now, we'll use keyboard shortcuts
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.H))
                ExecuteCommand("help");
            else if (Input.GetKeyDown(KeyCode.S))
                ExecuteCommand("status");
            else if (Input.GetKeyDown(KeyCode.C))
                ExecuteCommand("callbs");
            else if (Input.GetKeyDown(KeyCode.N))
                ExecuteCommand("newgame");
        }
    }

    public void ExecuteCommand(string command)
    {
        commandHistory.Add($"> {command}");
        if (commandHistory.Count > MAX_HISTORY)
        {
            commandHistory.RemoveAt(0);
        }
        
        string[] parts = command.ToLower().Split(' ');
        string cmd = parts[0];
        
        switch (cmd)
        {
            case "help":
                PrintHelp();
                break;
                
            case "status":
                PrintStatus();
                break;
                
            case "hand":
                PrintHand();
                break;
                
            case "play":
                if (parts.Length > 1)
                {
                    PlayCards(parts[1]);
                }
                else
                {
                    AddMessage("Usage: play [rank] (e.g., play ace)");
                }
                break;
                
            case "callbs":
                bsInterface.CallBS();
                AddMessage("Called BS!");
                break;
                
                
            case "newgame":
                bsInterface.StartNewGame();
                AddMessage("Started new game");
                break;
                
            default:
                AddMessage($"Unknown command: {cmd}");
                break;
        }
    }

    void PrintHelp()
    {
        AddMessage("=== BS Game Debug Console ===");
        AddMessage("Commands:");
        AddMessage("  help - Show this help");
        AddMessage("  status - Show game status");
        AddMessage("  hand - Show your hand");
        AddMessage("  play [rank] - Play cards of specified rank");
        AddMessage("  callbs - Call BS on previous player");
        AddMessage("  newgame - Start a new game");
        AddMessage("Keyboard shortcuts (hold Shift):");
        AddMessage("  H - Help, S - Status, C - Call BS, N - New Game");
    }

    void PrintStatus()
    {
        if (bsInterface == null) return;
        
        AddMessage($"Game in progress: {bsInterface.IsGameInProgress()}");
        AddMessage($"Player turn: {bsInterface.IsPlayerTurn()}");
        AddMessage($"Current claim: {bsInterface.GetCurrentClaim()}");
        AddMessage($"Cards to play: {bsInterface.GetCardsToPlay()}");
        AddMessage($"Hand size: {bsInterface.GetPlayerHand().Count}");
    }

    void PrintHand()
    {
        if (bsInterface == null) return;
        
        List<Card> hand = bsInterface.GetPlayerHand();
        if (hand.Count == 0)
        {
            AddMessage("No cards in hand");
            return;
        }
        
        AddMessage($"Hand ({hand.Count} cards):");
        foreach (Card card in hand)
        {
            AddMessage($"  {card.ToString()}");
        }
    }

    void PlayCards(string rankString)
    {
        if (!bsInterface.IsPlayerTurn())
        {
            AddMessage("Not your turn!");
            return;
        }
        
        // Parse rank
        CardRank rank;
        if (System.Enum.TryParse(rankString, true, out rank))
        {
            List<Card> cardsToPlay = bsInterface.GetCardsOfRank(rank);
            if (cardsToPlay.Count > 0)
            {
                bsInterface.PlayCards(cardsToPlay, rank);
                AddMessage($"Played {cardsToPlay.Count} {rank}(s)");
            }
            else
            {
                AddMessage($"You don't have any {rank}s");
            }
        }
        else
        {
            AddMessage($"Invalid rank: {rankString}");
        }
    }

    void AddMessage(string message)
    {
        commandHistory.Add(message);
        if (commandHistory.Count > MAX_HISTORY)
        {
            commandHistory.RemoveAt(0);
        }
        Debug.Log($"BS Console: {message}");
    }
}
