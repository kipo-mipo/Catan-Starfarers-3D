using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Mirror;

public class GameUI : MonoBehaviour
{
    public Button confirmSetupButton;
    public Button endTurnButton;
    public TMP_Text infoText;

    LobbyPlayer localLP;

    void Awake()
    {
        if (confirmSetupButton) confirmSetupButton.onClick.AddListener(OnConfirmSetup);
        if (endTurnButton) endTurnButton.onClick.AddListener(OnEndTurn);
    }

    int? selectedNode = null;

    void Update()
    {
        if (!NetworkClient.active) return;

        if (localLP == null && NetworkClient.localPlayer != null)
            localLP = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();

        var gm = GameManager.Instance;
        if (gm == null) return;

        uint you = NetworkClient.localPlayer != null ? NetworkClient.localPlayer.netId : 0;

        bool isSetup = gm.phase == MatchPhase.Setup;
        bool isTurn  = gm.phase == MatchPhase.Turn;

        // Show correct button
        if (confirmSetupButton) confirmSetupButton.gameObject.SetActive(isSetup);
        if (endTurnButton)      endTurnButton.gameObject.SetActive(isTurn);

        // Enable only if it's your action
        bool canConfirmSetup = isSetup && you != 0 && you == gm.CurrentSetupPlayerNetId;
        bool canEndTurn      = isTurn  && you != 0 && you == gm.CurrentPlayerNetId;

        if (confirmSetupButton) confirmSetupButton.interactable = canConfirmSetup;
        if (endTurnButton)      endTurnButton.interactable      = canEndTurn;

        // Debug info
        if (infoText)
        {
            infoText.text =
                $"Phase: {gm.phase}\n" +
                $"SetupCurrent: {gm.CurrentSetupPlayerNetId}\n" +
                $"TurnCurrent: {gm.CurrentPlayerNetId}\n" +
                $"You: {you}";
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            TryClickNode(mouse.position.ReadValue());
        }
    }

    void TryClickNode(Vector2 screenPos)
    {
        if (Camera.main == null) return;
        var ray = Camera.main.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out var hit, 1000f)) return;

        var node = hit.collider.GetComponentInParent<BoardNode>();
        if (node == null) return;

        var gm = GameManager.Instance;
        if (gm == null || NetworkClient.localPlayer == null) return;

        uint you = NetworkClient.localPlayer.netId;
        var lp = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        if (lp == null) return;

        if (gm.phase == MatchPhase.Setup && gm.setupRound == SetupRound.SpaceportAndShip && you == gm.CurrentSetupPlayerNetId)
        {
            if (selectedNode == null)
            {
                selectedNode = node.nodeId;
                Debug.Log($"Selected A={selectedNode}");
            }
            else
            {
                int a = selectedNode.Value;
                int b = node.nodeId;
                selectedNode = null;
                lp.CmdRequestPlaceShip(a, b);
            }
        }
        else if (gm.phase == MatchPhase.Setup && you == gm.CurrentSetupPlayerNetId)
        {
            if (gm.setupRound == SetupRound.Colony1 || gm.setupRound == SetupRound.Colony2)
            {
                lp.CmdRequestPlaceColony(node.nodeId);
                return;
            }
        }

    }

    void OnConfirmSetup()
    {
        if (NetworkClient.localPlayer == null) { Debug.Log("No localPlayer"); return; }

        var lp = NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
        if (lp == null) { Debug.Log("localPlayer has no LobbyPlayer"); return; }

        Debug.Log("[Client] Clicking ConfirmSetup");
        lp.CmdConfirmSetup();
    }

    void OnEndTurn()
    {
        if (localLP == null) return;
        localLP.CmdRequestEndTurn();
    }

}
