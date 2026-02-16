using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera boardCamera;

    [Header("Raycast")]
    [SerializeField] private float rayDistance = 100000f;

    [Header("UI Blocking")]
    [Tooltip("If true, clicking on UI will NOT click the board. If your canvas covers the screen, keep this OFF.")]
    [SerializeField] private bool blockClicksOverUI = false;

    [Header("Debug")]
    [SerializeField] private bool logClicks = false;
    [SerializeField] private bool drawDebugRay = false;

    private int? selectedNodeA = null;

    void Awake()
    {
        // Don’t touch BoardRegistry here. It may not be initialized yet depending on execution order.
        if (boardCamera == null) boardCamera = Camera.main;
    }

    void Update()
    {
        // Require local player
        if (!NetworkClient.active || NetworkClient.localPlayer == null) return;

        // Require input
        var mouse = Mouse.current;
        if (mouse == null) return;

        // Require registry (lazy)
        var registry = BoardRegistry.Instance;
        if (registry == null) return;

        // Require camera (lazy)
        if (boardCamera == null) boardCamera = Camera.main;
        if (boardCamera == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            TryClickNode(mouse.position.ReadValue(), registry);
        }
    }

    private void TryClickNode(Vector2 screenPos, BoardRegistry registry)
    {
        if (blockClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = boardCamera.ScreenPointToRay(screenPos);

        if (drawDebugRay)
            Debug.DrawRay(ray.origin, ray.direction * 5000f, Color.green, 0.25f);

        // IMPORTANT: use the board scene's physics scene (fixes Mirror / multi-scene mismatch)
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

        // Get game + local player component that owns commands
        var gm = GameManager.Instance;
        if (gm == null) return;

        var localIdentity = NetworkClient.localPlayer;
        var lp = localIdentity.GetComponent<LobbyPlayer>();
        if (lp == null)
        {
            Debug.LogError("[GameUI] Local player missing LobbyPlayer component.");
            return;
        }

        uint you = localIdentity.netId;

        // ----- SETUP PHASE -----
        if (gm.phase == MatchPhase.Setup)
        {
            // Not your turn in setup
            if (you != gm.CurrentSetupPlayerNetId)
            {
                if (logClicks) Debug.Log("[GameUI] Ignored: not your setup turn.");
                return;
            }

            // Colony placement: single click
            if (gm.setupRound == SetupRound.Colony1 || gm.setupRound == SetupRound.Colony2)
            {
                lp.CmdRequestPlaceColony(node.nodeId);
                return;
            }

            // Ship placement: two endpoints
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
