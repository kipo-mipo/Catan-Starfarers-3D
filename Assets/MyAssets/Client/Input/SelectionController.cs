using MyAssets.GameCore;
using MyAssets.Net;
using UnityEngine;
using Mirror;

namespace MyAssets.Client.Input
{
    public sealed class SelectionController : MonoBehaviour
    {
        [Header("Temp selection")]
        public int localPlayerId = 0;
        public int selectedShipId = 0;

        private NodeId? _from;

        // Hook this from a NodeView click later.
        public void OnNodeClicked(int nodeId)
        {
            var n = new NodeId(nodeId);

            if (_from is null)
            {
                _from = n;
                Debug.Log($"From selected: {nodeId}");
                return;
            }

            var req = new MoveShipRequest
            {
                Player = localPlayerId,
                Ship = selectedShipId,
                FromNode = _from.Value.Value,
                ToNode = n.Value
            };

            if (NetworkClient.active)
                NetworkClient.Send(req);

            Debug.Log($"MoveShipRequest sent: {_from.Value.Value} -> {n.Value}");
            _from = null;
        }
    }
}
