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

        void StartHost();
        void StartClient(string address);
        void Disconnect();

        void RequestStartMatch(int seed);
        void RequestMoveShip(int playerId, int shipId, int fromNodeId, int toNodeId);

        event Action<MyAssets.GameCore.IGameEvent> OnGameEvent;
        event Action<string> OnNetStatus;
    }
}
