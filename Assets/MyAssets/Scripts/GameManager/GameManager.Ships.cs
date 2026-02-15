using Mirror;
using UnityEngine;

public partial class GameManager
{
    [Header("Prefabs")]
    public GameObject shipPrefab;

    // One ship per seat for now. Store spawned ship netIds aligned to seatNetIds.
    public readonly SyncList<uint> shipNetIdsBySeat = new();

    [Server]
    void EnsureShipListSized()
    {
        while (shipNetIdsBySeat.Count < seatNetIds.Count)
            shipNetIdsBySeat.Add(0u);
    }

    int SeatIndexOf(uint playerNetId)
    {
        for (int i = 0; i < seatNetIds.Count; i++)
            if (seatNetIds[i] == playerNetId) return i;
        return -1;
    }

    [Server]
    public void RequestPlaceShip(uint requesterNetId, int nodeAId, int nodeBId)
    {
        if (phase != MatchPhase.Setup) return;
        if (setupRound != SetupRound.SpaceportAndShip) return;
        if (requesterNetId != CurrentSetupPlayerNetId) return;

        if (BoardRegistry.Instance == null) { Debug.LogError("No BoardRegistry in scene"); return; }
        if (!BoardRegistry.Instance.Nodes.ContainsKey(nodeAId) || !BoardRegistry.Instance.Nodes.ContainsKey(nodeBId)) return;
        if (!BoardRegistry.Instance.AreAdjacent(nodeAId, nodeBId)) return;

        EnsureShipListSized();
        int seat = SeatIndexOf(requesterNetId);
        if (seat < 0) return;

        // Already placed a ship this setup step
        if (shipNetIdsBySeat[seat] != 0u) return;

        var go = Object.Instantiate(shipPrefab);
        var ship = go.GetComponent<Ship>();
        ship.ownerNetId = requesterNetId;
        ship.currentNodeId = nodeBId; // decide start node; nodeB is fine

        NetworkServer.Spawn(go);
        shipNetIdsBySeat[seat] = go.GetComponent<NetworkIdentity>().netId;

        Debug.Log($"[Server] Ship placed for {requesterNetId} at node {nodeBId}");
    }

    [Server]
    public void RequestMoveShip(uint requesterNetId, int toNodeId)
    {
        if (phase != MatchPhase.Turn) return;
        if (requesterNetId != CurrentPlayerNetId) return;

        EnsureShipListSized();
        int seat = SeatIndexOf(requesterNetId);
        if (seat < 0) return;

        uint shipNetId = shipNetIdsBySeat[seat];
        if (shipNetId == 0u) return;

        if (!NetworkServer.spawned.TryGetValue(shipNetId, out var shipIdentity)) return;
        var ship = shipIdentity.GetComponent<Ship>();
        if (ship == null) return;

        if (BoardRegistry.Instance == null) return;
        if (!BoardRegistry.Instance.Nodes.ContainsKey(toNodeId)) return;
        if (!BoardRegistry.Instance.AreAdjacent(ship.currentNodeId, toNodeId)) return;

        ship.currentNodeId = toNodeId;
        Debug.Log($"[Server] Ship moved for {requesterNetId} -> node {toNodeId}");
    }
}
