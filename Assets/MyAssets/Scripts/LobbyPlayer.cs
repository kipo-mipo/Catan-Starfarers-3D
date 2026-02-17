using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady;

    public event System.Action<bool> ReadyChanged;

    [Command]
    public void CmdSetReady(bool ready)
    {
        isReady = ready;
        Debug.Log($"[Server] Ready set: conn={connectionToClient.connectionId} netId={netId} ready={ready}");
    }

    [Command]
    public void CmdConfirmSetup()
    {
        if (GameManager.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdConfirmSetup from senderNetId={senderNetId}");
        GameManager.Instance.ConfirmSetupAction(senderNetId);
    }

    [Command]
    public void CmdRequestEndTurn()
    {
        if (GameManager.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestEndTurn from senderNetId={senderNetId}");
        GameManager.Instance.RequestEndTurn(senderNetId);
    }

    [Command]
    public void CmdRequestPlaceShip(int a, int b)
    {
        if (GameManager.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestPlaceShip from senderNetId={senderNetId} a={a} b={b}");
        GameManager.Instance.RequestPlaceShip(senderNetId, a, b);
    }

    [Command]
    public void CmdRequestMoveShip(int toNodeId)
    {
        if (GameManager.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestMoveShip from senderNetId={senderNetId} toNodeId={toNodeId}");
        GameManager.Instance.RequestMoveShip(senderNetId, toNodeId);
    }

    [Command]
    public void CmdRequestPlaceColony(int nodeId)
    {
        if (GameManager.Instance == null) return;

        uint senderNetId = connectionToClient.identity.netId;
        Debug.Log($"[Server] CmdRequestPlaceColony from senderNetId={senderNetId} nodeId={nodeId}");
        GameManager.Instance.RequestPlaceColony(senderNetId, nodeId);
    }

    void OnReadyChanged(bool _, bool newValue)
    {
        ReadyChanged?.Invoke(newValue);
    }
}
