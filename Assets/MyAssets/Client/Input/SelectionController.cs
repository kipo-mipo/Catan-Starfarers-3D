using UnityEngine;
using UnityEngine.EventSystems;
using MyAssets.Client.Net;
using MyAssets.GameCore;

namespace MyAssets.Client.Input
{
    public sealed class SelectionController : MonoBehaviour
    {
        // BoardSpawner checks this to decide whether board-node adapters should handle clicks.
        // True means this controller does the board raycast centrally every mouse click.
        public static bool IsUsingCentralPointerRaycast => true;

        [Header("Temp selection")]
        public int localPlayerId = -1;
        public int selectedShipId = 0;

        [Header("Setup defaults until proper setup UI exists")]
        [SerializeField] private SetupShipType defaultSetupShipType = SetupShipType.ColonyShip;
        [SerializeField] private UpgradeType defaultSetupUpgrade = UpgradeType.Booster;

        [Header("Refs")]
        [SerializeField] private ClientSessionModel session;

        [Header("Click Diagnostics")]
        [SerializeField] private bool logClickDiagnostics = true;
        [SerializeField] private bool ignoreClicksOverUI = false;
        [SerializeField] private float raycastDistance = 10000f;

        private int? _fromNode;
        private int? _setupColonyToUpgrade;

        private void Awake()
        {
            ResolvePersistentRefs();
        }

        private void OnEnable()
        {
            ResolvePersistentRefs();
            Debug.Log($"[SelectionController] Enabled on {name}. Session found={(session != null)}. LocalPlayerId={(session != null ? session.LocalPlayerId : localPlayerId)}");
        }

        private void Update()
        {
            var mouse = global::UnityEngine.InputSystem.Mouse.current;
            if (mouse == null)
                return;

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            Vector2 screenPosition = mouse.position.ReadValue();

            if (ignoreClicksOverUI && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                if (logClickDiagnostics)
                    Debug.Log($"[SelectionController] Click ignored because pointer is over UI. screen={screenPosition}");
                return;
            }

            if (logClickDiagnostics)
                Debug.Log($"[SelectionController] Mouse click received. screen={screenPosition}");

            TryRaycastBoardNode(screenPosition);
        }

        private void TryRaycastBoardNode(Vector2 screenPosition)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[SelectionController] Click received, but Camera.main is null. Tag your board camera as MainCamera.");
                return;
            }

            Ray ray = cam.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance, ~0, QueryTriggerInteraction.Collide);

            if (hits == null || hits.Length == 0)
            {
                Debug.Log($"[SelectionController] Click raycast hit nothing at screen={screenPosition} using camera={cam.name}.");
                return;
            }

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                var target = hit.collider.GetComponentInParent<MyAssets.Client.BoardView.BoardNodeClickTarget>();
                if (target == null)
                    target = hit.collider.GetComponentInChildren<MyAssets.Client.BoardView.BoardNodeClickTarget>();

                if (target == null)
                    continue;

                Debug.Log($"[SelectionController] Node clicked by central raycast: {target.NodeId} hit={hit.collider.name} target={target.name} distance={hit.distance:0.00}");
                OnNodeClicked(target.NodeId);
                return;
            }

            string hitNames = string.Join(", ", System.Array.ConvertAll(hits, h => h.collider.name));
            Debug.Log($"[SelectionController] Click hit {hits.Length} collider(s), but none had BoardNodeClickTarget. Hits: {hitNames}");
        }

        private void ResolvePersistentRefs()
        {
            if (session == null)
                session = FindAnyObjectByType<ClientSessionModel>();
        }

        public void OnNodeClicked(int nodeId)
        {
            ResolvePersistentRefs();

            if (session != null && session.Phase == MatchPhase.Setup)
            {
                HandleSetupClick(nodeId);
                return;
            }

            if (_fromNode is null)
            {
                _fromNode = nodeId;
                Debug.Log($"From selected: {nodeId}");
                return;
            }

            int playerId = ResolveLocalPlayerId();
            int shipId = ResolveSelectedShipId(playerId);
            NetService.Bridge?.RequestMoveShip(playerId, shipId, _fromNode.Value, nodeId);
            Debug.Log($"MoveShip requested: player={playerId} ship={shipId} {_fromNode.Value} -> {nodeId}");
            _fromNode = null;
        }

        private int ResolveLocalPlayerId()
        {
            if (session != null && session.LocalPlayerId >= 0)
                return session.LocalPlayerId;

            return localPlayerId;
        }

        private int ResolveSelectedShipId(int playerId)
        {
            if (selectedShipId != 0 || playerId == 0 || session == null)
                return selectedShipId;

            foreach (var kv in session.ShipToPlayer)
                if (kv.Value == playerId)
                    return kv.Key;

            return selectedShipId;
        }

        private void HandleSetupClick(int nodeId)
        {
            if (session == null)
            {
                Debug.LogWarning("[SelectionController] No ClientSessionModel found. Put one on persistent NetworkRoot.");
                return;
            }

            if (!session.IsLocalSetupTurn)
            {
                Debug.Log($"Not your setup turn. Local player: {session.LocalPlayerId}, current setup player: {session.CurrentSetupPlayerId}");
                return;
            }

            if (session.CurrentSetupRound != SetupRound.SpaceportShipUpgrade)
            {
                var colonyBridge = NetService.Bridge;
                if (colonyBridge == null)
                {
                    Debug.LogWarning("[SelectionController] No NetService.Bridge found; cannot request setup colony.");
                    return;
                }

                colonyBridge.RequestChooseStartNode(nodeId);
                Debug.Log($"Setup colony requested: player={session.LocalPlayerId} node={nodeId}");
                return;
            }

            if (_setupColonyToUpgrade is null)
            {
                _setupColonyToUpgrade = nodeId;
                Debug.Log($"Setup round 4: colony selected for spaceport upgrade: {nodeId}. Now click an adjacent ship site.");
                return;
            }

            var setupBridge = NetService.Bridge;
            if (setupBridge == null)
            {
                Debug.LogWarning("[SelectionController] No NetService.Bridge found; cannot complete setup spaceport/ship.");
                return;
            }

            setupBridge.RequestCompleteSetupSpaceportShip(
                _setupColonyToUpgrade.Value,
                nodeId,
                defaultSetupShipType,
                defaultSetupUpgrade);

            Debug.Log($"Setup round 4 requested: spaceport={_setupColonyToUpgrade.Value}, shipSite={nodeId}, ship={defaultSetupShipType}, upgrade={defaultSetupUpgrade}");
            _setupColonyToUpgrade = null;
        }

        public void SetSetupShipType(SetupShipType shipType)
        {
            defaultSetupShipType = shipType;
            Debug.Log($"Setup ship type selected: {shipType}");
        }

        public void SetSetupUpgrade(UpgradeType upgrade)
        {
            defaultSetupUpgrade = upgrade;
            Debug.Log($"Setup upgrade selected: {upgrade}");
        }
    }
}
