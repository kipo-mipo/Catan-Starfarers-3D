using System.Collections.Generic;
using UnityEngine;

namespace MyAssets.Client.Authoring
{
    [CreateAssetMenu(menuName = "MyAssets/Tile Library")]
    public sealed class TileLibraryAsset : ScriptableObject
    {
        public enum TileType { Empty, Planet, TradeStation }

        [System.Serializable]
        public struct Tile
        {
            public int id;
            public TileType type;

            public int rollToken;
            public int yield;
            public int starValue;
            public int resourceType; // map to GameCore.ResourceType later
        }

        public List<Tile> tiles = new();
    }
}
