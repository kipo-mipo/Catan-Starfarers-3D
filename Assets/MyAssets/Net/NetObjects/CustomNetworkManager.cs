using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public GameObject matchServerPrefab;
    MatchServer match;

    public override void Awake()
    {
        base.Awake();
        autoCreatePlayer = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        var go = Instantiate(matchServerPrefab);
        NetworkServer.Spawn(go);
        match = go.GetComponent<MatchServer>();
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        if (!NetworkClient.ready)
            NetworkClient.Ready();

        if (NetworkClient.localPlayer == null)
            NetworkClient.AddPlayer();
    }

    public override void OnClientSceneChanged()
    {
        base.OnClientSceneChanged();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        if (conn == null)
        {
            Debug.LogError("[Server] OnServerAddPlayer: conn is null (should never happen)");
            return;
        }

        if (conn.identity == null)
        {
            Debug.LogWarning($"[Server] OnServerAddPlayer: connId={conn.connectionId} identity is null right after base(). Skipping seat add.");
            return;
        }

        if (match == null) match = FindFirstObjectByType<MatchServer>();
        if (match == null)
        {
            Debug.LogError("[Server] OnServerAddPlayer: MatchServer not found. Did it fail to spawn?");
            return;
        }

        match.AddPlayerSeat(conn.identity);
        Debug.Log($"[Server] Player added. connId={conn.connectionId} netId={conn.identity.netId}");
    }

    [Server]
    public bool AreAllPlayersReady()
    {
        int players = 0;

        foreach (var kvp in NetworkServer.connections)
        {
            if (kvp.Value is not NetworkConnectionToClient conn)
                continue;

            // ignore connections that haven't spawned a player yet
            if (conn.identity == null)
                continue;

            var lp = conn.identity.GetComponent<LobbyParticipant>();
            if (lp == null)
                return false;

            players++;
            if (!lp.isReady)
                return false;
        }

        return players >= 2;
    }

    [Server]
    public void ServerStartGameIfReady()
    {
        if (match == null) match = FindFirstObjectByType<MatchServer>();
        if (!AreAllPlayersReady()) return;

        match.StartGame();
    }
}
