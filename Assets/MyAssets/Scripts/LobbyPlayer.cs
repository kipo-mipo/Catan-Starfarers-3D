using Mirror;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnReadyChanged))]
    public bool isReady;

    public event System.Action<bool> ReadyChanged;

    public override void OnStartLocalPlayer()
    {
        // Optional: reset local state UI-side
    }

    [Command]
    public void CmdSetReady(bool ready)
    {
        isReady = ready;
        Debug.Log($"[Server] Ready set: conn={connectionToClient.connectionId} netId={netId} ready={ready}");
    }

    [Command]
    public void CmdConfirmSetup()
    {
        Debug.Log($"[Server] CmdConfirmSetup from netId={netId} phase={(GameManager.Instance ? GameManager.Instance.phase.ToString() : "NO_GM")}");
        GameManager.Instance?.ConfirmSetupAction(netId);
    }

    [Command]
    public void CmdRequestEndTurn()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.RequestEndTurn(netId);
    }

    void OnReadyChanged(bool _, bool newValue)
    {
        ReadyChanged?.Invoke(newValue);
    }

    [Command]
    public void CmdRequestPlaceShip(int nodeAId, int nodeBId)
    {
        GameManager.Instance?.RequestPlaceShip(netId, nodeAId, nodeBId);
    }

    [Command]
    public void CmdRequestMoveShip(int toNodeId)
    {
        GameManager.Instance?.RequestMoveShip(netId, toNodeId);
    }

    [Command]
    public void CmdRequestPlaceColony(int nodeId)
    {
        GameManager.Instance?.RequestPlaceColony(netId, nodeId);
    }

}

