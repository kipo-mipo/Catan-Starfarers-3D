using Mirror;
using UnityEngine;

public partial class MatchServer
{

    [Server]
    public void RequestEndTurn(uint requesterNetId)
    {
        if (phase != MatchPhase.Turn) return;
        if (requesterNetId != CurrentPlayerNetId) return;

        AdvanceTurn(requesterNetId);
    }

    [Server]
    public void AdvanceTurn(uint requesterNetId)
    {
        if (phase != MatchPhase.Turn) return;
        if (seatNetIds.Count < 2) return;
        if (requesterNetId != CurrentPlayerNetId) return;

        turnIndex = (turnIndex + 1) % seatNetIds.Count;
        Debug.Log($"[Server] Turn advanced. CurrentPlayerNetId={CurrentPlayerNetId}");
    }
}
