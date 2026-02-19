using System.Collections.Generic;
using UnityEngine;

namespace MyAssets.Client.Authoring
{
    [CreateAssetMenu(menuName = "MyAssets/Setup")]
    public sealed class SetupAsset : ScriptableObject
    {
        public string setupId = "Default";

        [System.Serializable]
        public struct SlotTile
        {
            public int slotId;
            public int tileId;
        }

        public List<SlotTile> placements = new();
    }
}
