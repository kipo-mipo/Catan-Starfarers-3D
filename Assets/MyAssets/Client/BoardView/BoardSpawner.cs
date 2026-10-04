using System;
using System.Collections.Generic;
using MyAssets.GameCore;
using MyAssets.Client.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MyAssets.Client.BoardView
{
    public sealed class BoardSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject nodePrefab;
        public GameObject slotPrefab;

        [Header("Sector Piece Visuals")]
        public GameObject planetSectorPrefab;
        public GameObject tradeSectorPrefab;
        public GameObject emptySectorPrefab;

        [Header("Lane Rendering")]
        [Tooltip("Optional prefab to use as the lane GameObject. If omitted, a new GameObject is created per lane.")]
        public GameObject lanePrefab;
        public Material laneMaterial;
        [Tooltip("World units width of lane strip mesh.")]
        public float laneWidth = 0.08f;
        [Tooltip("Y offset for lane mesh (to avoid z-fighting).")]
        public float laneZ = 0f;

        public BoardRegistry Registry { get; private set; }

        public event Action<Bounds> OnBoardSpawned;

        public void Spawn(BoardDefinitionData def)
        {
            // Clear previous children
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);

            Registry = new BoardRegistry();

            foreach (var n in def.Nodes)
            {
                var go = Instantiate(nodePrefab, ToUnity(n.Position), Quaternion.identity, transform);
                go.name = string.IsNullOrWhiteSpace(n.ObjectName) ? BoardGenerator.DefaultNodeObjectName(n.Id) : n.ObjectName;

                if (n.IsInteractable)
                    EnsureNodeClickTarget(go, n.Id);
                else
                    DisableAllColliders(go);

                if (n.IsPlanetSectorSlot)
                    Registry.PlanetSectorNodes.Add(n.Id);

                Registry.Nodes[n.Id] = go.transform;
            }

            foreach (var s in def.Slots)
            {
                var go = Instantiate(slotPrefab, ToUnity(s.Position), Quaternion.identity, transform);
                go.name = $"Slot_{s.Id.Value}_{s.Orientation}";
                Registry.Slots[s.Id] = go.transform;
                Registry.SlotOrientations[s.Id] = s.Orientation;
            }

            foreach (var l in def.Lanes)
            {
                if (!Registry.Nodes.TryGetValue(l.A, out var a) || !Registry.Nodes.TryGetValue(l.B, out var b))
                    continue;

                CreateLaneStrip(l.Id.Value, a.position, b.position);
            }

            var bounds = ComputeBounds();
            Debug.Log($"[BoardSpawner] Spawned {Registry.Nodes.Count} nodes. Interactable click targets={GetComponentsInChildren<BoardNodeClickTarget>(true).Length}.");
            OnBoardSpawned?.Invoke(bounds);
        }


        public void SpawnSectorPieceIntoSlot(SlotId slotId, SectorPieceDef pieceDef, int rotation)
        {
            SpawnSectorPieceIntoSlot(slotId, pieceDef, rotation, null);
        }

        public void SpawnSectorPieceIntoSlot(
            SlotId slotId,
            SectorPieceDef pieceDef,
            int rotation,
            TokenSetupResult tokenSetup)
        {
            if (Registry == null)
            {
                Debug.LogError("Cannot spawn sector piece before board has been spawned.");
                return;
            }

            if (!Registry.Slots.TryGetValue(slotId, out var slotTransform))
            {
                Debug.LogError($"No slot found for {slotId.Value}.");
                return;
            }

            var prefab = pieceDef.Type switch
            {
                SectorPieceType.Planetary => planetSectorPrefab,
                SectorPieceType.StarterPlanetary => planetSectorPrefab,
                SectorPieceType.Trade => tradeSectorPrefab,
                SectorPieceType.Empty => emptySectorPrefab,
                _ => null
            };

            if (prefab == null)
            {
                Debug.LogError($"No prefab assigned for sector type {pieceDef.Type}.");
                return;
            }

            ClearExistingSectorPiece(slotTransform);

            var go = Instantiate(prefab, slotTransform.position, slotTransform.rotation, slotTransform);
            go.name = $"SectorPiece_{pieceDef.Id.Value}_{pieceDef.Type}";

            var slotOrientation = Registry.SlotOrientations.TryGetValue(slotId, out var orientation)
                ? orientation
                : SlotOrientation.Down;

            if (go.TryGetComponent(out PlanetSectorView planetView))
                planetView.Apply(pieceDef, rotation, slotOrientation, slotId, tokenSetup?.PlanetTokens);

            if (go.TryGetComponent(out TradeSectorView tradeView))
                tradeView.Apply(pieceDef, rotation, slotOrientation);
        }

        private static void ClearExistingSectorPiece(Transform slotTransform)
        {
            for (int i = slotTransform.childCount - 1; i >= 0; i--)
            {
                var child = slotTransform.GetChild(i);
                if (child.name.StartsWith("SectorPiece_", StringComparison.Ordinal))
                    Destroy(child.gameObject);
            }
        }

        private void CreateLaneStrip(int laneId, Vector3 a, Vector3 b)
        {
            GameObject go = lanePrefab != null
                ? Instantiate(lanePrefab, Vector3.zero, Quaternion.identity, transform)
                : new GameObject($"Lane_{laneId}");

            go.name = $"Lane_{laneId}";
            go.transform.SetParent(transform, worldPositionStays: true);
            go.layer = LayerMask.NameToLayer("BG");

            var mf = go.GetComponent<MeshFilter>();
            if (mf == null) mf = go.AddComponent<MeshFilter>();

            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) mr = go.AddComponent<MeshRenderer>();

            if (laneMaterial != null)
                mr.sharedMaterial = laneMaterial;

            // Quad strip mesh in XZ plane
            var dir = b - a;
            dir.y = 0f;
            float len = dir.magnitude;
            if (len < 0.0001f) return;

            dir /= len;
            var perp = new Vector3(-dir.z, 0f, dir.x);
            var offset = perp * (laneWidth * 0.5f);

            Vector3 baseA = new Vector3(a.x, laneZ, a.z);
            Vector3 baseB = new Vector3(b.x, laneZ, b.z);

            Vector3 v0 = baseA - offset;
            Vector3 v1 = baseA + offset;
            Vector3 v2 = baseB - offset;
            Vector3 v3 = baseB + offset;

            var mesh = new Mesh();
            mesh.name = $"LaneMesh_{laneId}";
            mesh.vertices = new[] { v0, v1, v2, v3 };
            mesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
            mesh.uv = new[]
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1),
            };

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            mf.sharedMesh = mesh;
        }


        private static void EnsureNodeClickTarget(GameObject go, NodeId nodeId)
        {
            // Do not trust the node prefab. Add/normalize a root collider so the central
            // SelectionController raycast can hit the node even if child colliders/layers are wrong.
            var sphere = go.GetComponent<SphereCollider>();
            if (sphere == null)
                sphere = go.AddComponent<SphereCollider>();

            sphere.radius = 0.45f;
            sphere.center = Vector3.zero;
            sphere.isTrigger = false;
            sphere.enabled = true;

            var target = go.GetComponent<BoardNodeClickTarget>();
            if (target == null)
                target = go.AddComponent<BoardNodeClickTarget>();

            target.Init(nodeId.Value);
        }

        private static void DisableAllColliders(GameObject go)
        {
            foreach (var collider in go.GetComponentsInChildren<Collider>(includeInactive: true))
                collider.enabled = false;
        }

        private Bounds ComputeBounds()
        {
            bool hasAny = false;
            var bounds = new Bounds(transform.position, Vector3.zero);

            void Encapsulate(Transform t)
            {
                if (!hasAny)
                {
                    bounds = new Bounds(t.position, Vector3.zero);
                    hasAny = true;
                }
                else
                {
                    bounds.Encapsulate(t.position);
                }
            }

            if (Registry != null)
            {
                foreach (var kv in Registry.Nodes) Encapsulate(kv.Value);
                foreach (var kv in Registry.Slots) Encapsulate(kv.Value);
            }

            if (!hasAny)
                bounds = new Bounds(transform.position, new Vector3(10f, 1f, 10f));

            bounds.Expand(2f);
            return bounds;
        }

        private static Vector3 ToUnity(System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
    }

    public sealed class BoardRegistry
    {
        public readonly Dictionary<NodeId, Transform> Nodes = new();
        public readonly Dictionary<SlotId, Transform> Slots = new();
        public readonly Dictionary<SlotId, SlotOrientation> SlotOrientations = new();
        public readonly HashSet<NodeId> PlanetSectorNodes = new();
    }

    /// <summary>
    /// Tiny click adapter attached to spawned board nodes.
    /// Keeps node prefabs dumb and routes all selection logic to the one scene-level SelectionController.
    /// </summary>
    public sealed class BoardNodeClickTarget : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private int nodeId = -1;
        public int NodeId => nodeId;
        private SelectionController _selection;

        public void Init(int id)
        {
            nodeId = id;
        }

        private void Awake()
        {
            ResolveSelectionController();
            EnsureCameraCanSendPhysicsPointerEvents();
        }

        // Works with EventSystem-based pointer input. This avoids both the disabled legacy
        // UnityEngine.Input API and direct references to the new Input System package.
        public void OnPointerClick(PointerEventData eventData)
        {
            ForwardClick();
        }

        // Kept as a fallback for editor/project configurations where OnMouseDown still fires.
        private void OnMouseDown()
        {
            ForwardClick();
        }

        private void ForwardClick()
        {
            if (nodeId < 0)
            {
                Debug.LogWarning($"[BoardNodeClickTarget] Clicked node target with invalid id on {name}.");
                return;
            }

            ResolveSelectionController();
            if (_selection == null)
            {
                Debug.LogWarning("[BoardNodeClickTarget] No SelectionController found. Put one on persistent NetworkRoot.");
                return;
            }

            Debug.Log($"[BoardNodeClickTarget] Node clicked: {nodeId} ({name})");
            _selection.OnNodeClicked(nodeId);
        }

        private void ResolveSelectionController()
        {
            if (_selection == null)
                _selection = FindAnyObjectByType<SelectionController>();
        }

        private static void EnsureCameraCanSendPhysicsPointerEvents()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            if (cam.GetComponent<PhysicsRaycaster>() == null)
                cam.gameObject.AddComponent<PhysicsRaycaster>();
        }
    }

}