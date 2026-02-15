using System.Collections.Generic;
using UnityEngine;

public class BoardRegistry : MonoBehaviour
{
    public static BoardRegistry Instance { get; private set; }

    public readonly Dictionary<int, BoardNode> Nodes = new();
    public readonly Dictionary<int, BoardLane> Lanes = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Nodes.Clear(); Lanes.Clear();

        foreach (var n in GetComponentsInChildren<BoardNode>(true))
        {
            if (Nodes.ContainsKey(n.nodeId))
                throw new System.Exception($"Duplicate nodeId {n.nodeId}");
            Nodes.Add(n.nodeId, n);
        }

        foreach (var l in GetComponentsInChildren<BoardLane>(true))
        {
            if (Lanes.ContainsKey(l.laneId))
                throw new System.Exception($"Duplicate laneId {l.laneId}");
            if (!Nodes.ContainsKey(l.nodeAId) || !Nodes.ContainsKey(l.nodeBId))
                throw new System.Exception($"Lane {l.laneId} references missing node(s): {l.nodeAId}, {l.nodeBId}");
            Lanes.Add(l.laneId, l);
        }

        Debug.Log($"[BoardRegistry] Nodes={Nodes.Count} Lanes={Lanes.Count}");
    }

    public bool AreAdjacent(int a, int b)
    {
        foreach (var lane in Lanes.Values)
            if (lane.Connects(a, b)) return true;
        return false;
    }

    public Vector3 NodePos(int nodeId) => Nodes[nodeId].transform.position;
}
