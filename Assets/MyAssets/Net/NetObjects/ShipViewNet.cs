using Mirror;
using UnityEngine;

public class ShipViewNet : NetworkBehaviour
{
    [SyncVar] public uint ownerNetId;
    [SyncVar(hook = nameof(OnNodeChanged))] public int currentNodeId;

    void OnNodeChanged(int _, int newNode)
    {
        if (BoardViewRegistry.Instance == null) return;
        transform.position = BoardViewRegistry.Instance.NodePos(newNode);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Snap on spawn
        if (BoardViewRegistry.Instance != null)
            transform.position = BoardViewRegistry.Instance.NodePos(currentNodeId);
    }
}
