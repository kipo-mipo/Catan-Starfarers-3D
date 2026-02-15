using Mirror;
using UnityEngine;

public enum MatchPhase { Lobby, Setup, Turn, Resolve, GameOver }

public partial class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [SyncVar] public int seed;
    [SyncVar] public MatchPhase phase = MatchPhase.Lobby;

    // Seats + turn
    public readonly SyncList<uint> seatNetIds = new();
    [SyncVar] public int turnIndex = 0;

    public uint CurrentPlayerNetId =>
        seatNetIds.Count == 0 ? 0u : seatNetIds[turnIndex % seatNetIds.Count];

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnStartServer()
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
        Debug.Log($"[Server] Seed set to {seed}");
    }
}
