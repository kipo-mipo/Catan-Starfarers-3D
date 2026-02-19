using Mirror;
using MyAssets.Net;
using UnityEngine;

namespace MyAssets.Client.Net
{
    public sealed class ClientEventReceiver : MonoBehaviour
    {
        private void Awake()
        {
            NetworkClient.RegisterHandler<CoreEventMessage>(OnCoreEvent);
        }

        private void OnCoreEvent(CoreEventMessage msg)
        {
            // Later: dispatch to animation/UI systems
            switch (msg.EventType)
            {
                case EventTypes.SectorRevealed:
                    // msg.A = slotId, msg.B = tileId, msg.C = type
                    break;
                case EventTypes.ShipMoved:
                    break;
            }
        }
    }
}
