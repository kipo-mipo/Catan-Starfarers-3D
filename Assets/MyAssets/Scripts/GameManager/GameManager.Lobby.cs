using Mirror;
using UnityEngine;

public partial class GameManager
{
    [Server]
    public void AddPlayerSeat(NetworkIdentity playerId)
    {
        if (playerId == null) return;
        if (seatNetIds.Contains(playerId.netId)) return;

        seatNetIds.Add(playerId.netId);
        Debug.Log($"[Server] Added seat: netId={playerId.netId} seats={seatNetIds.Count}");
    }

    [Server]
    public void StartGame()
    {
        if (phase != MatchPhase.Lobby) return;
        if (seatNetIds.Count < 2) return;

        phase = MatchPhase.Setup;
        turnIndex = 0;

        // setupRound/setupIndex init goes in GameManager.Setup.cs
        ServerBeginSetup();

        Debug.Log($"[Server] Game started. Phase=Setup CurrentSetup={CurrentSetupPlayerNetId}");
    }
}
