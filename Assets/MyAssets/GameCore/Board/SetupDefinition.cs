using System.Collections.Generic;

namespace MyAssets.GameCore
{
    // Unique tiles placed onto fixed slots.
    public sealed class SetupDefinitionData
    {
        public string SetupId = "Default";
        public readonly Dictionary<SlotId, TileId> SlotToTile = new();
    }
}
