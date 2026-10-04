using System;

namespace MyAssets.GameCore.Net
{
    [Serializable]
    public struct LobbyPlayerInfo
    {
        public int ConnectionId;
        public int PlayerId;
        public string Name;
        public bool Ready;

        public LobbyPlayerInfo(int connectionId, string name, bool ready)
            : this(connectionId, connectionId, name, ready)
        {
        }

        public LobbyPlayerInfo(int connectionId, int playerId, string name, bool ready)
        {
            ConnectionId = connectionId;
            PlayerId = playerId;
            Name = name ?? string.Empty;
            Ready = ready;
        }
    }

    [Serializable]
    public struct LobbyStateInfo
    {
        public LobbyPlayerInfo[] Players;
        public bool CanStart;

        public LobbyStateInfo(LobbyPlayerInfo[] players, bool canStart)
        {
            Players = players ?? Array.Empty<LobbyPlayerInfo>();
            CanStart = canStart;
        }
    }
}
