using Mirror;
using UnityEngine;

public class PlayerHello : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        Debug.Log($"I am the local player. netId={netId}");
        GetComponent<Renderer>().material.color = Color.green;
    }

    public override void OnStartClient()
    {
        Debug.Log($"Client spawned player. netId={netId} isLocal={isLocalPlayer}");
    }
}
