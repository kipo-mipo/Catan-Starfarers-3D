using System.Collections.Generic;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Client.BoardView
{
    public sealed class Registry : MonoBehaviour
    {
        public readonly Dictionary<NodeId, Transform> Nodes = new();
        public readonly Dictionary<SlotId, Transform> Slots = new();

        public void Clear()
        {
            Nodes.Clear();
            Slots.Clear();
        }

        public bool TryGetNode(NodeId id, out Transform t) => Nodes.TryGetValue(id, out t);
        public bool TryGetSlot(SlotId id, out Transform t) => Slots.TryGetValue(id, out t);
    }
}
