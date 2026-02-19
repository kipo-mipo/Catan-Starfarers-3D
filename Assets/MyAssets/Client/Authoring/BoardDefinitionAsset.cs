using System.Collections.Generic;
using UnityEngine;

namespace MyAssets.Client.Authoring
{
    [CreateAssetMenu(menuName = "MyAssets/Board Definition")]
    public sealed class BoardDefinitionAsset : ScriptableObject
    {
        [System.Serializable] public struct Node { public int id; public Vector3 position; }
        [System.Serializable] public struct Lane { public int id; public int a; public int b; }
        [System.Serializable] public struct Slot { public int id; public Vector3 position; public List<int> neighborNodes; }

        public List<Node> nodes = new();
        public List<Lane> lanes = new();
        public List<Slot> slots = new();

        public List<int> startingSlotIds = new();
    }
}
