using UnityEngine;
using MyAssets.Client.Net;

namespace MyAssets.Client.Input
{
    public sealed class SelectionController : MonoBehaviour
    {
        [Header("Temp selection")]
        public int localPlayerId = 0;
        public int selectedShipId = 0;

        private int? _fromNode;

        public void OnNodeClicked(int nodeId)
        {
            if (_fromNode is null)
            {
                _fromNode = nodeId;
                Debug.Log($"From selected: {nodeId}");
                return;
            }

            NetService.Bridge?.RequestMoveShip(localPlayerId, selectedShipId, _fromNode.Value, nodeId);
            Debug.Log($"MoveShip requested: {_fromNode.Value} -> {nodeId}");
            _fromNode = null;
        }
    }
}
