using Mirror;

namespace MyAssets.Net
{
    // ===== Lobby =====
    public struct JoinLobbyRequest : NetworkMessage
    {
        public string PlayerName;
    }

    public struct SetReadyRequest : NetworkMessage
    {
        public bool Ready;
    }

    public struct LobbyPlayerState
    {
        public int ConnectionId;
        public int PlayerId;
        public string Name;
        public bool Ready;
    }

    /// <summary>
    /// Server -> one client. This is the authoritative local player id the client must use
    /// for setup turn checks and player-owned actions.
    /// </summary>
    public struct AssignedPlayerIdMessage : NetworkMessage
    {
        public int PlayerId;
    }

    /// <summary>
    /// Server -> clients lobby snapshot.
    /// </summary>
    public struct LobbyStateMessage : NetworkMessage
    {
        public LobbyPlayerState[] Players;
        public bool CanStart;
    }

    // ===== Match =====
    public struct StartMatchRequest : NetworkMessage
    {
        public int Seed;
    }

    public struct ChooseStartNodeRequest : NetworkMessage
    {
        public int NodeId;
    }

    public struct CompleteSetupSpaceportShipRequest : NetworkMessage
    {
        public int ColonyNode;
        public int ShipNode;
        public byte ShipType;
        public byte Upgrade;
    }

    public struct MoveShipRequest : NetworkMessage
    {
        public int Player;
        public int Ship;
        public int FromNode;
        public int ToNode;
    }

    // ===== Core Event Pump =====
    public struct CoreEventMessage : NetworkMessage
    {
        public byte EventType;
        public int A; public int B; public int C; public int D;
        public string Text;
    }

    static class EventTypes
    {
        public const byte MatchStarted = 1;
        public const byte ShipMoved = 2;
        public const byte SectorRevealed = 3;

        public const byte ShipPlaced = 4;
        public const byte SetupCompleted = 5;
        public const byte SetupStepChanged = 6;
        public const byte ColonyPlaced = 7;
        public const byte SpaceportPlaced = 8;
        public const byte SetupUpgradeGranted = 9;
        public const byte StartingResourcesGranted = 10;
        public const byte FameMedalGranted = 11;

        public const byte Rejected = 255;
    }
}
