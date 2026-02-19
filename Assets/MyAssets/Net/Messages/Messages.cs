using Mirror;
using MyAssets.GameCore;

namespace MyAssets.Net
{
    public struct StartMatchRequest : NetworkMessage { public int Seed; }
    public struct MoveShipRequest : NetworkMessage
    {
        public int Player; public int Ship; public int FromNode; public int ToNode;
    }

    public struct CoreEventMessage : NetworkMessage
    {
        // “type tag + payload” approach (simple now, replace later)
        public byte EventType;
        public int A; public int B; public int C; public int D;
    }

    public static class EventTypes
    {
        public const byte MatchStarted = 1;
        public const byte ShipMoved = 2;
        public const byte SectorRevealed = 3;
        public const byte Rejected = 255;
    }
}
