using Mirror;
using UnityEngine;

public enum MatchPhase { Lobby, Setup, Turn, Resolve, GameOver }

public partial class MatchServer : NetworkBehaviour
{
    GameCore.GameState state;
    
    [Server]
    public bool TryApply(GameCore.IGameAction action, out string error)
    {
        var result = GameCore.RulesEngine.Apply(state, action);
        if (!result.Success)
        {
            error = result.Error;
            return false;
        }

        state = result.State;
        error = "";
        //BroadcastEvents(result.Events); // or broadcast snapshot
        return true;
    }

    public static MatchServer Instance { get; private set; }

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
        state = new GameCore.GameState();
        state.Seed = seed;
        state.Phase = GameCore.MatchPhase.NotStarted;
        Debug.Log($"[Server] Seed set to {seed}");
    }
}
