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
        // Wire button listeners so you don't rely on inspector event hookups.
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
        // Require local player
        if (!NetworkClient.active || NetworkClient.localPlayer == null)
        {
            UpdateButtonInteractable(null, null);
            return;
        }

        var gm = GameManager.Instance;
        var lp = GetLocalLobbyPlayer();

        UpdateButtonInteractable(gm, lp);

        // Require mouse
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Require registry
        var registry = BoardRegistry.Instance;
        if (registry == null) return;

        // Require camera
        if (boardCamera == null) boardCamera = Camera.main;
        if (boardCamera == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryClickNode(mouse.position.ReadValue(), registry, gm, lp);
        }
    }

    private LobbyPlayer GetLocalLobbyPlayer()
    {
        if (NetworkClient.localPlayer == null) return null;
        return NetworkClient.localPlayer.GetComponent<LobbyPlayer>();
    }

    private void UpdateButtonInteractable(GameManager gm, LobbyPlayer lp)
    {
        // Default: disabled if not ready
        bool canConfirm = false;
        bool canEndTurn = false;

        if (gm != null && lp != null && NetworkClient.localPlayer != null)
        {
            uint you = NetworkClient.localPlayer.netId;

            if (gm.phase == MatchPhase.Setup)
            {
                // Let only current setup player confirm
                canConfirm = (you == gm.CurrentSetupPlayerNetId);
                canEndTurn = false;
            }
            else if (gm.phase == MatchPhase.Turn)
            {
                canConfirm = false;
                canEndTurn = (you == gm.CurrentPlayerNetId);
            }
        }

        if (confirmSetupButton != null) confirmSetupButton.interactable = canConfirm;
        if (endTurnButton != null) endTurnButton.interactable = canEndTurn;
    }

    // ---------- BUTTON HANDLERS ----------
    public void OnConfirmSetupClicked()
    {
        var lp = GetLocalLobbyPlayer();
        if (lp == null)
        {
            Debug.LogError("[GameUI] Confirm clicked but local LobbyPlayer is missing.");
            return;
        }

        if (logButtonPresses)
            Debug.Log("[GameUI] ConfirmSetup button pressed -> CmdConfirmSetup()");

        lp.CmdConfirmSetup();
    }

    public void OnEndTurnClicked()
    {
        var lp = GetLocalLobbyPlayer();
        if (lp == null)
        {
            Debug.LogError("[GameUI] EndTurn clicked but local LobbyPlayer is missing.");
            return;
        }

        if (logButtonPresses)
            Debug.Log("[GameUI] EndTurn button pressed -> CmdRequestEndTurn()");

        lp.CmdRequestEndTurn();
    }

    // ---------- BOARD CLICKING ----------
    private void TryClickNode(Vector2 screenPos, BoardRegistry registry, GameManager gm, LobbyPlayer lp)
    {
        if (lp == null) return;
        if (gm == null) return;

        if (blockClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = boardCamera.ScreenPointToRay(screenPos);

        if (drawDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * 5000f, Color.green, 0.25f);

        // Use board scene physics scene
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

        var node = hit.collider.GetComponentInParent<BoardNode>();
        if (node == null)
        {
            if (logClicks) Debug.Log($"[GameUI] Hit '{hit.collider.name}' but not a BoardNode.");
            return;
        }

        if (logClicks) Debug.Log($"[GameUI] Hit nodeId={node.nodeId}");

        uint you = NetworkClient.localPlayer.netId;

        // ----- SETUP PHASE -----
        if (gm.phase == MatchPhase.Setup)
        {
            if (you != gm.CurrentSetupPlayerNetId)
            {
                if (logClicks) Debug.Log("[GameUI] Ignored: not your setup turn.");
                return;
            }

            if (gm.setupRound == SetupRound.Colony1 || gm.setupRound == SetupRound.Colony2)
            {
                lp.CmdRequestPlaceColony(node.nodeId);
                return;
            }

            if (gm.setupRound == SetupRound.SpaceportAndShip)
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

                lp.CmdRequestPlaceShip(a, b);
                return;
            }

            if (logClicks) Debug.Log($"[GameUI] Setup round {gm.setupRound} ignores node clicks.");
            return;
        }

        // ----- TURN PHASE -----
        if (gm.phase == MatchPhase.Turn)
        {
            if (you != gm.CurrentPlayerNetId)
            {
                if (logClicks) Debug.Log("[GameUI] Ignored: not your turn.");
                return;
            }

            lp.CmdRequestMoveShip(node.nodeId);
            return;
        }
    }
}
