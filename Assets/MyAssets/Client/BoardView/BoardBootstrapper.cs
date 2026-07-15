using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyAssets.Client.Net;
using MyAssets.GameCore;
using MyAssets.GameCore.Net;

namespace MyAssets.Client.BoardView
{
    public sealed class BoardBootstrapper : MonoBehaviour
    {
        [SerializeField] private BoardSpawner spawner;
        [SerializeField] private float hexsize = .5f;
        [SerializeField] private PlayerCountMode playerCountMode = PlayerCountMode.Four;

        [Header("Player Piece Visuals")]
        [Tooltip("Optional. Assign MyAssets/Prefabs/PlayerPIeces/Colony.prefab here. If empty, a simple fallback marker is created.")]
        [SerializeField] private GameObject colonyPrefab;
        [Tooltip("Optional. If empty, colonyPrefab is reused and scaled up for spaceports.")]
        [SerializeField] private GameObject spaceportPrefab;
        [Tooltip("Optional. If empty, a simple fallback marker is created.")]
        [SerializeField] private GameObject setupShipPrefab;
        [SerializeField] private float pieceYOffset = 0.04f;

        private readonly Dictionary<int, GameObject> _structureVisualsByNode = new();
        private readonly Dictionary<int, GameObject> _shipVisualsByShip = new();

        private INetBridge _bridge;

        private void Awake()
        {
            if (spawner == null) spawner = GetComponent<BoardSpawner>();
            if (spawner == null) Debug.LogError("[BoardBootstrapper] No BoardSpawner found/assigned.");
        }

        private void OnEnable()
        {
            StartCoroutine(HookWhenBridgeExists());
        }

        private void OnDisable()
        {
            if (_bridge != null) _bridge.OnGameEvent -= OnEvent;
            _bridge = null;
        }

        private IEnumerator HookWhenBridgeExists()
        {
            while (true)
            {
                var b = NetService.Bridge;
                if (b != null)
                {
                    _bridge = b;
                    _bridge.OnGameEvent += OnEvent;
                    Debug.Log("[BoardBootstrapper] Hooked OnGameEvent.");
                    yield break;
                }
                yield return null;
            }
        }

        private void OnEvent(IGameEvent e)
        {
            switch (e)
            {
                case MatchStartedEvent ms:
                    SpawnBoardAndSectors(ms.Seed);
                    break;

                case ColonyPlacedEvent colony:
                    SpawnColonyVisual(colony.Player.Value, colony.Node.Value);
                    break;

                case SpaceportPlacedEvent spaceport:
                    SpawnSpaceportVisual(spaceport.Player.Value, spaceport.Node.Value);
                    break;

                case ShipPlacedEvent ship:
                    SpawnShipVisual(ship.Player.Value, ship.Ship.Value, ship.Node.Value);
                    break;

                case ShipMovedEvent moved:
                    MoveShipVisual(moved.Ship.Value, moved.To.Value);
                    break;
            }
        }

        private void SpawnBoardAndSectors(int matchSeed)
        {
            if (spawner == null)
            {
                Debug.LogError("[BoardBootstrapper] Can't spawn: spawner is null.");
                return;
            }

            ClearPieceVisualCaches();

            var board = BoardGenerator.Generate(hexsize);
            spawner.Spawn(board);

            var pieces = SectorPieceLibraryFactory.CreateDefault();
            var rules = SetupRules.For(playerCountMode);
            var setup = SetupGenerator.Generate(board, pieces, rules, matchSeed);
            var starterSlotToPiece = BuildStarterSlotToPiece(board);

            if (starterSlotToPiece.Count == 0)
                return;

            var allPlacedPieces = new Dictionary<SlotId, SectorPieceId>();
            foreach (var pair in setup.SlotToPiece)
                allPlacedPieces[pair.Key] = pair.Value;
            foreach (var pair in starterSlotToPiece)
                allPlacedPieces[pair.Key] = pair.Value;

            var tokenLibrary = TokenLibraryFactory.CreateDefault(playerCountMode);
            var tokenSetup = TokenSetupGenerator.Generate(
                pieces,
                allPlacedPieces,
                tokenLibrary,
                matchSeed);

            foreach (var pair in setup.SlotToPiece)
            {
                var slotId = pair.Key;
                var pieceId = pair.Value;
                var pieceDef = pieces.Pieces[pieceId];
                var rotation = setup.SlotToRotation[slotId];

                spawner.SpawnSectorPieceIntoSlot(slotId, pieceDef, rotation, tokenSetup);
            }

            SpawnStartingSectors(pieces, starterSlotToPiece, tokenSetup);

            Debug.Log($"[BoardBootstrapper] Board, sectors, starting sectors, and tokens spawned. Unused piece: {setup.UnusedPiece.Value}.");
        }


        private void ClearPieceVisualCaches()
        {
            _structureVisualsByNode.Clear();
            _shipVisualsByShip.Clear();
        }

        private void SpawnColonyVisual(int playerId, int nodeId)
        {
            SpawnStructureVisual(playerId, nodeId, colonyPrefab, "Colony", fallback: PrimitiveType.Sphere, fallbackScale: 0.22f);
        }

        private void SpawnSpaceportVisual(int playerId, int nodeId)
        {
            // Upgrading replaces the colony visual on the same node.
            SpawnStructureVisual(playerId, nodeId, spaceportPrefab != null ? spaceportPrefab : colonyPrefab, "Spaceport", fallback: PrimitiveType.Cylinder, fallbackScale: 0.28f, scaleMultiplier: spaceportPrefab == null ? 1.25f : 1f);
        }

        private void SpawnStructureVisual(
            int playerId,
            int nodeId,
            GameObject prefab,
            string label,
            PrimitiveType fallback,
            float fallbackScale,
            float scaleMultiplier = 1f)
        {
            if (spawner == null || spawner.Registry == null || !spawner.Registry.Nodes.TryGetValue(new NodeId(nodeId), out var node))
            {
                Debug.LogWarning($"[BoardBootstrapper] Cannot spawn {label}: no node visual for node {nodeId}.");
                return;
            }

            if (_structureVisualsByNode.TryGetValue(nodeId, out var existing) && existing != null)
                Destroy(existing);

            var go = InstantiatePieceVisual(prefab, fallback, node, fallbackScale, scaleMultiplier);
            go.name = $"{label}_P{playerId}_Node{nodeId}";
            _structureVisualsByNode[nodeId] = go;
        }

        private void SpawnShipVisual(int playerId, int shipId, int nodeId)
        {
            if (spawner == null || spawner.Registry == null || !spawner.Registry.Nodes.TryGetValue(new NodeId(nodeId), out var node))
            {
                Debug.LogWarning($"[BoardBootstrapper] Cannot spawn setup ship: no node visual for node {nodeId}.");
                return;
            }

            if (_shipVisualsByShip.TryGetValue(shipId, out var existing) && existing != null)
                Destroy(existing);

            var go = InstantiatePieceVisual(setupShipPrefab, PrimitiveType.Capsule, node, 0.18f, 1f);
            go.name = $"SetupShip_P{playerId}_Ship{shipId}_Node{nodeId}";
            _shipVisualsByShip[shipId] = go;
        }

        private void MoveShipVisual(int shipId, int toNodeId)
        {
            if (!_shipVisualsByShip.TryGetValue(shipId, out var go) || go == null)
                return;

            if (spawner == null || spawner.Registry == null || !spawner.Registry.Nodes.TryGetValue(new NodeId(toNodeId), out var node))
                return;

            go.transform.SetParent(node, worldPositionStays: false);
            go.transform.localPosition = new Vector3(0f, pieceYOffset, 0f);
        }

        private GameObject InstantiatePieceVisual(GameObject prefab, PrimitiveType fallback, Transform node, float fallbackScale, float scaleMultiplier)
        {
            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, node);
                go.transform.localPosition = new Vector3(0f, pieceYOffset, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale *= scaleMultiplier;
            }
            else
            {
                go = GameObject.CreatePrimitive(fallback);
                go.transform.SetParent(node, worldPositionStays: false);
                go.transform.localPosition = new Vector3(0f, pieceYOffset, 0f);
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one * fallbackScale;

                // Fallback marker is visual only; the node collider remains the click target.
                foreach (var collider in go.GetComponentsInChildren<Collider>(includeInactive: true))
                    Destroy(collider);
            }

            return go;
        }

        private Dictionary<SlotId, SectorPieceId> BuildStarterSlotToPiece(BoardDefinitionData board)
        {
            var starterPieceIds = new[]
            {
                new SectorPieceId(300),
                new SectorPieceId(301),
                new SectorPieceId(302),
                new SectorPieceId(303),
            };

            var startingSlots = board.Slots
                .Where(slot => board.StartingSlots.Contains(slot.Id))
                .OrderBy(slot => slot.Id.Value)
                .ToList();

            if (startingSlots.Count != starterPieceIds.Length)
            {
                Debug.LogError($"[BoardBootstrapper] Expected {starterPieceIds.Length} starting slots, got {startingSlots.Count}.");
                return new Dictionary<SlotId, SectorPieceId>();
            }

            var mapping = new Dictionary<SlotId, SectorPieceId>();
            for (int i = 0; i < starterPieceIds.Length; i++)
                mapping[startingSlots[i].Id] = starterPieceIds[i];

            return mapping;
        }

        private void SpawnStartingSectors(
            SectorPieceLibraryData pieces,
            IReadOnlyDictionary<SlotId, SectorPieceId> starterSlotToPiece,
            TokenSetupResult tokenSetup)
        {
            foreach (var pair in starterSlotToPiece)
            {
                var slotId = pair.Key;
                var pieceId = pair.Value;

                if (!pieces.Pieces.TryGetValue(pieceId, out var pieceDef))
                {
                    Debug.LogError($"[BoardBootstrapper] Missing starter sector piece {pieceId.Value}.");
                    continue;
                }

                spawner.SpawnSectorPieceIntoSlot(slotId, pieceDef, rotation: 0, tokenSetup);
            }
        }
    }
}
