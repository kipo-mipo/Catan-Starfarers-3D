using System;

namespace MyAssets.GameCore.Net
{
    public enum NetRole { None, Host, Client }

    /// <summary>
    /// Client-facing networking bridge. Implemented by Net assembly, consumed by Client assembly.
    /// No UnityEngine or Mirror types allowed here.
    /// </summary>
    public interface INetBridge
    {
        NetRole Role { get; }
        bool IsConnected { get; }
        int LocalPlayerId { get; }

        void StartHost();
        void StartClient(string address);
        void Disconnect();

        // Lobby
        void RequestJoinLobby(string playerName);
        void SetReady(bool ready);

        // Match
        void RequestStartMatch(int seed);
        void RequestChooseStartNode(int nodeId);
        void RequestCompleteSetupSpaceportShip(int colonyNodeId, int shipNodeId, SetupShipType shipType, UpgradeType upgrade);
        void RequestMoveShip(int playerId, int shipId, int fromNodeId, int toNodeId);

        event Action<MyAssets.GameCore.IGameEvent> OnGameEvent;
        event Action<LobbyStateInfo> OnLobbyState;
        event Action<string> OnNetStatus;
    }
}
