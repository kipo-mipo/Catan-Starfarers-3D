using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardRegistry : MonoBehaviour
{
    public static BoardRegistry Instance { get; private set; }

    // The physics scene that owns the board objects (critical for multi-scene / Mirror setups)
    public PhysicsScene physicsScene;

    // Lookups by ID
    public readonly Dictionary<int, BoardNode> Nodes = new Dictionary<int, BoardNode>();
    public readonly Dictionary<int, BoardLane> Lanes = new Dictionary<int, BoardLane>();

    // Cached adjacency (undirected)
    private readonly HashSet<ulong> adjacency = new HashSet<ulong>();

    // ------------ Unity lifecycle ------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BoardRegistry] Duplicate detected, destroying this instance.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Scene s = gameObject.scene;
        physicsScene = s.GetPhysicsScene();

        Rebuild();

        Debug.Log($"[BoardRegistry] scene={s.name} sceneValid={s.IsValid()} loaded={s.isLoaded} physValid={physicsScene.IsValid()} Nodes={Nodes.Count} Lanes={Lanes.Count}");
    }

    // Call if you dynamically modify the board later
    public void Rebuild()
    {
        Nodes.Clear();
        Lanes.Clear();
        adjacency.Clear();

        // Include inactive too
        var nodes = GetComponentsInChildren<BoardNode>(true);
        foreach (var n in nodes)
        {
            int id = n.nodeId;
            if (Nodes.ContainsKey(id))
            {
                Debug.LogError($"[BoardRegistry] Duplicate BoardNode id={id} on '{n.name}' and '{Nodes[id].name}'");
                continue;
            }
            Nodes.Add(id, n);
        }

        var lanes = GetComponentsInChildren<BoardLane>(true);
        foreach (var l in lanes)
        {
            int id = GetLaneIdSafe(l);
            if (Lanes.ContainsKey(id))
            {
                Debug.LogError($"[BoardRegistry] Duplicate BoardLane id={id} on '{l.name}' and '{Lanes[id].name}'");
                continue;
            }
            Lanes.Add(id, l);

            // Build adjacency from lane endpoints
            if (TryGetLaneEndpoints(l, out int a, out int b))
            {
                AddAdj(a, b);
                AddAdj(b, a);
            }
            else
            {
                Debug.LogWarning($"[BoardRegistry] Could not determine endpoints for lane '{l.name}'. AreAdjacent() may fail for it.");
            }
        }
    }

    // ------------ Public API your project already expects ------------

    public Vector3 NodePos(int nodeId)
    {
        if (Nodes.TryGetValue(nodeId, out var node) && node != null)
            return node.transform.position;

        Debug.LogError($"[BoardRegistry] NodePos: unknown nodeId={nodeId}");
        return Vector3.zero;
    }

    public bool AreAdjacent(int nodeA, int nodeB)
    {
        return adjacency.Contains(MakeKey(nodeA, nodeB));
    }

    public bool TryGetNode(int nodeId, out BoardNode node) => Nodes.TryGetValue(nodeId, out node);
    public bool TryGetLane(int laneId, out BoardLane lane) => Lanes.TryGetValue(laneId, out lane);

    // ------------ Internals ------------

    private void AddAdj(int a, int b)
    {
        if (a == b) return;
        adjacency.Add(MakeKey(a, b));
    }

    // Packs (a,b) into a single ulong key
    private static ulong MakeKey(int a, int b)
    {
        unchecked
        {
            // cast to uint to avoid negative sign extension issues
            return ((ulong)(uint)a << 32) | (uint)b;
        }
    }

    private static int GetLaneIdSafe(BoardLane lane)
    {
        // Most likely your lane has laneId. If it does, use it.
        // If not, fall back to instance id (still unique, but not stable across runs).
        var t = lane.GetType();
        var f = t.GetField("laneId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(int))
            return (int)f.GetValue(lane);

        var p = t.GetProperty("laneId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(int) && p.CanRead)
            return (int)p.GetValue(lane);

        return lane.GetInstanceID();
    }

    private static bool TryGetLaneEndpoints(BoardLane lane, out int a, out int b)
    {
        a = 0; b = 0;
        if (lane == null) return false;

        // Common endpoint id field/property names seen in board graph scripts
        // (Add more here if your lane uses different names.)
        string[] namesA =
        {
            "a","A","nodeA","nodeAId","nodeIdA","startNode","startNodeId","fromNode","fromNodeId","end0","node0","n0"
        };
        string[] namesB =
        {
            "b","B","nodeB","nodeBId","nodeIdB","endNode","endNodeId","toNode","toNodeId","end1","node1","n1"
        };

        // 1) Try direct int ids
        if (TryGetIntByAnyName(lane, namesA, out a) && TryGetIntByAnyName(lane, namesB, out b))
            return true;

        // 2) Try two-int scan: find any two distinct int fields/properties that look like endpoints
        if (TryFindTwoIntEndpoints(lane, out a, out b))
            return true;

        // 3) Try BoardNode references, then pull nodeId from them
        if (TryGetBoardNodeIdRef(lane, namesA, out a) && TryGetBoardNodeIdRef(lane, namesB, out b))
            return true;

        return false;
    }

    private static bool TryGetIntByAnyName(object obj, string[] names, out int value)
    {
        value = 0;
        var t = obj.GetType();

        foreach (var n in names)
        {
            var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(int))
            {
                value = (int)f.GetValue(obj);
                return true;
            }

            var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && p.PropertyType == typeof(int) && p.CanRead)
            {
                value = (int)p.GetValue(obj);
                return true;
            }
        }

        return false;
    }

    private static bool TryFindTwoIntEndpoints(object obj, out int a, out int b)
    {
        a = 0; b = 0;
        var t = obj.GetType();

        // Find all int fields
        var ints = new List<int>();

        foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.FieldType == typeof(int))
                ints.Add((int)f.GetValue(obj));
        }

        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (p.PropertyType == typeof(int) && p.CanRead && p.GetIndexParameters().Length == 0)
            {
                try { ints.Add((int)p.GetValue(obj)); }
                catch { /* ignore */ }
            }
        }

        // Need at least two values; pick the first two that are not equal.
        for (int i = 0; i < ints.Count; i++)
        {
            for (int j = i + 1; j < ints.Count; j++)
            {
                if (ints[i] != ints[j])
                {
                    a = ints[i];
                    b = ints[j];
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetBoardNodeIdRef(object obj, string[] names, out int nodeId)
    {
        nodeId = 0;
        var t = obj.GetType();

        foreach (var n in names)
        {
            var f = t.GetField(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && typeof(BoardNode).IsAssignableFrom(f.FieldType))
            {
                var bn = f.GetValue(obj) as BoardNode;
                if (bn != null) { nodeId = bn.nodeId; return true; }
            }

            var p = t.GetProperty(n, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (p != null && typeof(BoardNode).IsAssignableFrom(p.PropertyType) && p.CanRead)
            {
                var bn = p.GetValue(obj) as BoardNode;
                if (bn != null) { nodeId = bn.nodeId; return true; }
            }
        }

        return false;
    }
}
