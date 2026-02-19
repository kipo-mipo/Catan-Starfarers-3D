using System.Collections.Generic;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Client.BoardView
{
    public sealed class BoardSpawner : MonoBehaviour
    {
        public GameObject nodePrefab;
        public GameObject lanePrefab;
        public GameObject slotPrefab;

        public BoardRegistry Registry { get; private set; }

        public void Spawn(BoardDefinitionData def)
        {
            Registry = new BoardRegistry();

            foreach (var n in def.Nodes)
            {
                var go = Instantiate(nodePrefab, ToUnity(n.Position), Quaternion.identity, transform);
                Registry.Nodes[n.Id] = go.transform;
            }

            foreach (var s in def.Slots)
            {
                var go = Instantiate(slotPrefab, ToUnity(s.Position), Quaternion.identity, transform);
                Registry.Slots[s.Id] = go.transform;
            }

            // lanes: you’ll render as line/mesh between nodes later
            foreach (var l in def.Lanes)
            {
                var go = Instantiate(lanePrefab, Vector3.zero, Quaternion.identity, transform);
                go.name = $"Lane_{l.Id.Value}";
            }
        }

        private static Vector3 ToUnity(System.Numerics.Vector3 v) => new Vector3(v.X, v.Y, v.Z);
    }

    public sealed class BoardRegistry
    {
        public readonly Dictionary<NodeId, Transform> Nodes = new();
        public readonly Dictionary<SlotId, Transform> Slots = new();
    }
}
