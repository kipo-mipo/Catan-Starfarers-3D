using Mirror;
using UnityEngine;

public class MatchParticipant : NetworkBehaviour
{
    [Command]
    public void CmdConfirmSetup()
    {
        if (MatchServer.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdConfirmSetup from senderNetId={senderNetId}");
        MatchServer.Instance.ConfirmSetupAction(senderNetId);
    }

    [Command]
    public void CmdRequestEndTurn()
    {
        if (MatchServer.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestEndTurn from senderNetId={senderNetId}");
        MatchServer.Instance.RequestEndTurn(senderNetId);
    }

    [Command]
    public void CmdRequestPlaceShip(int a, int b)
    {
        if (MatchServer.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestPlaceShip from senderNetId={senderNetId} a={a} b={b}");
        MatchServer.Instance.RequestPlaceShip(senderNetId, a, b);
    }

    [Command]
    public void CmdRequestMoveShip(int toNodeId)
    {
        if (MatchServer.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestMoveShip from senderNetId={senderNetId} toNodeId={toNodeId}");
        MatchServer.Instance.RequestMoveShip(senderNetId, toNodeId);
    }

    [Command]
    public void CmdRequestPlaceColony(int nodeId)
    {
        if (MatchServer.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestPlaceColony from senderNetId={senderNetId} nodeId={nodeId}");
        MatchServer.Instance.RequestPlaceColony(senderNetId, nodeId);
    }
}