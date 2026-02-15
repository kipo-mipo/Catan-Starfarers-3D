using Mirror;
using UnityEngine;

public class Ship : NetworkBehaviour
{
    [SyncVar] public uint ownerNetId;
    [SyncVar(hook = nameof(OnNodeChanged))] public int currentNodeId;

    void OnNodeChanged(int _, int newNode)
    {
        if (BoardRegistry.Instance == null) return;
        transform.position = BoardRegistry.Instance.NodePos(newNode);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Snap on spawn
        if (BoardRegistry.Instance != null)
            transform.position = BoardRegistry.Instance.NodePos(currentNodeId);
    }
}
