namespace MyAssets.GameCore
{
    public readonly record struct PlayerId(int Value);
    public readonly record struct ShipId(int Value);

    public readonly record struct NodeId(int Value);
    public readonly record struct LaneId(int Value);

    public readonly record struct SlotId(int Value);   // sector location on the fixed board
    public readonly record struct TileId(int Value);   // unique physical piece
}
