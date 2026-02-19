using Mirror;
using UnityEngine;

public class LobbyParticipant : NetworkBehaviour
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

    void OnReadyChanged(bool _, bool newValue)
    {
        ReadyChanged?.Invoke(newValue);
    }
}