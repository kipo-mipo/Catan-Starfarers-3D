using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera boardCamera;

    [Header("UI Buttons (assign in Inspector)")]
    [SerializeField] private Button confirmSetupButton;
    [SerializeField] private Button endTurnButton;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 100000f;

    [Header("UI Blocking")]
    [Tooltip("If true, clicking on UI will NOT click the board. If your canvas covers the screen, keep this OFF.")]
    [SerializeField] private bool blockClicksOverUI = false;

    [Header("Debug")]
    [SerializeField] private bool logClicks = false;
    [SerializeField] private bool drawDebugRay = false;
    [SerializeField] private bool logButtonPresses = true;

    private int? selectedNodeA = null;

    void Awake()
    {
        if (boardCamera == null) boardCamera = Camera.main;
    }

    void Start()
    {
        if (confirmSetupButton != null)
        {
            confirmSetupButton.onClick.RemoveListener(OnConfirmSetupClicked);
            confirmSetupButton.onClick.AddListener(OnConfirmSetupClicked);
        }

        if (endTurnButton != null)
        {
            endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            endTurnButton.onClick.AddListener(OnEndTurnClicked);
        }
    }

    void Update()
    {
        if (!NetworkClient.active || NetworkClient.localPlayer == null)
        {
            UpdateButtonInteractable(null);
            return;
        }

        var match = MatchServer.Instance;
        UpdateButtonInteractable(match);

        var mouse = Mouse.current;
        if (mouse == null) return;

        var registry = BoardViewRegistry.Instance;
        if (registry == null) return;

        if (boardCamera == null) boardCamera = Camera.main;
        if (boardCamera == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
            TryClickNode(mouse.position.ReadValue(), registry, match);
    }

    private MatchParticipant GetLocalMatchParticipant()
    {
        if (NetworkClient.localPlayer == null) return null;
        return NetworkClient.localPlayer.GetComponent<MatchParticipant>();
    }

    private void UpdateButtonInteractable(MatchServer match)
    {
        bool canConfirm = false;
        bool canEndTurn = false;

        if (match != null && NetworkClient.localPlayer != null)
        {
            uint you = NetworkClient.localPlayer.netId;

            if (match.phase == MatchPhase.Setup)
            {
                canConfirm = (you == match.CurrentSetupPlayerNetId);
            }
            else if (match.phase == MatchPhase.Turn)
            {
                canEndTurn = (you == match.CurrentPlayerNetId);
            }
        }

        if (confirmSetupButton != null) confirmSetupButton.interactable = canConfirm;
        if (endTurnButton != null) endTurnButton.interactable = canEndTurn;
    }

    // ---------- BUTTON HANDLERS ----------
    public void OnConfirmSetupClicked()
    {
        var p = GetLocalMatchParticipant();
        if (p == null)
        {
            Debug.LogError("[GameUI] Confirm clicked but local MatchParticipant is missing.");
            return;
        }

        if (logButtonPresses)
            Debug.Log("[GameUI] ConfirmSetup button pressed -> CmdConfirmSetup()");

        p.CmdConfirmSetup();
    }

    public void OnEndTurnClicked()
    {
        var p = GetLocalMatchParticipant();
        if (p == null)
        {
            Debug.LogError("[GameUI] EndTurn clicked but local MatchParticipant is missing.");
            return;
        }

        if (logButtonPresses)
            Debug.Log("[GameUI] EndTurn button pressed -> CmdRequestEndTurn()");

        p.CmdRequestEndTurn();
    }

    // ---------- BOARD CLICKING ----------
    private void TryClickNode(Vector2 screenPos, BoardViewRegistry registry, MatchServer match)
    {
        var p = GetLocalMatchParticipant();
        if (p == null || match == null) return;

        if (blockClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = boardCamera.ScreenPointToRay(screenPos);

        if (drawDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * 5000f, Color.green, 0.25f);

        var ps = registry.physicsScene;
        if (!ps.IsValid())
        {
            if (logClicks) Debug.Log("[GameUI] Board physicsScene invalid (yet).");
            return;
        }

        if (!ps.Raycast(ray.origin, ray.direction, out var hit, rayDistance))
        {
            if (logClicks) Debug.Log("[GameUI] Raycast MISS");
            return;
        }

        var node = hit.collider.GetComponentInParent<BoardNodeView>();
        if (node == null)
        {
            if (logClicks) Debug.Log($"[GameUI] Hit '{hit.collider.name}' but not a BoardNode.");
            return;
        }

        if (logClicks) Debug.Log($"[GameUI] Hit nodeId={node.nodeId}");

        uint you = NetworkClient.localPlayer.netId;

        // ----- SETUP PHASE -----
        if (match.phase == MatchPhase.Setup)
        {
            if (you != match.CurrentSetupPlayerNetId)
            {
                if (logClicks) Debug.Log("[GameUI] Ignored: not your setup turn.");
                return;
            }

            if (match.setupRound == SetupRound.Colony1 || match.setupRound == SetupRound.Colony2)
            {
                p.CmdRequestPlaceColony(node.nodeId);
                return;
            }

            if (match.setupRound == SetupRound.SpaceportAndShip)
            {
                if (selectedNodeA == null)
                {
                    selectedNodeA = node.nodeId;
                    if (logClicks) Debug.Log($"[GameUI] Selected ship endpoint A={selectedNodeA.Value}");
                    return;
                }

                int a = selectedNodeA.Value;
                int b = node.nodeId;
                selectedNodeA = null;

                p.CmdRequestPlaceShip(a, b);
                return;
            }

            if (logClicks) Debug.Log($"[GameUI] Setup round {match.setupRound} ignores node clicks.");
            return;
        }

        // ----- TURN PHASE -----
        if (match.phase == MatchPhase.Turn)
        {
            if (you != match.CurrentPlayerNetId)
            {
                if (logClicks) Debug.Log("[GameUI] Ignored: not your turn.");
                return;
            }

            p.CmdRequestMoveShip(node.nodeId);
            return;
        }
    }
}
