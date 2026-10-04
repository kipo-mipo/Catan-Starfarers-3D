using System.Collections.Generic;
using Mirror;

namespace MyAssets.Net
{
    internal sealed class NetLobbyDirectory
    {
        private readonly int _minPlayersToStart;
        private int? _dedicatedHostConnectionId;
        private int _nextPlayerId;

        private readonly Dictionary<int, LobbyPlayerState> _roster = new();

        public NetLobbyDirectory(int minPlayersToStart)
        {
            _minPlayersToStart = minPlayersToStart < 1 ? 1 : minPlayersToStart;
        }

        public int PlayerCount => _roster.Count;

        public IEnumerable<int> ConnectionIds => _roster.Keys;

        public IEnumerable<int> PlayerIds
        {
            get
            {
                var ids = new List<int>(_roster.Count);
                foreach (var kv in _roster)
                    ids.Add(kv.Value.PlayerId);

                ids.Sort();
                return ids;
            }
        }

        public void OnServerConnected(NetworkConnectionToClient conn)
        {
            if (_dedicatedHostConnectionId == null)
                _dedicatedHostConnectionId = conn.connectionId;

            EnsurePlayer(conn);
        }

        public void OnServerDisconnected(NetworkConnectionToClient conn)
        {
            _roster.Remove(conn.connectionId);

            if (NetworkServer.localConnection == null && _dedicatedHostConnectionId == conn.connectionId)
            {
                _dedicatedHostConnectionId = null;
                foreach (var kv in NetworkServer.connections)
                {
                    _dedicatedHostConnectionId = kv.Key;
                    break;
                }
            }
        }

        public void OnJoinLobby(NetworkConnectionToClient conn, JoinLobbyRequest msg)
        {
            var current = EnsurePlayer(conn);
            var name = string.IsNullOrWhiteSpace(msg.PlayerName) ? $"Player {current.PlayerId}" : msg.PlayerName.Trim();

            current.Name = name;
            _roster[conn.connectionId] = current;
        }

        public void OnSetReady(NetworkConnectionToClient conn, SetReadyRequest msg)
        {
            if (_roster.TryGetValue(conn.connectionId, out var p))
            {
                p.Ready = msg.Ready;
                _roster[conn.connectionId] = p;
            }
        }

        public bool TryGetPlayerId(NetworkConnectionToClient conn, out int playerId)
        {
            if (conn != null && _roster.TryGetValue(conn.connectionId, out var p))
            {
                playerId = p.PlayerId;
                return true;
            }

            playerId = -1;
            return false;
        }

        public LobbyStateMessage BuildLobbyState()
        {
            var players = new List<LobbyPlayerState>(_roster.Values);
            players.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));

            return new LobbyStateMessage
            {
                Players = players.ToArray(),
                CanStart = players.Count >= _minPlayersToStart
            };
        }

        public bool CanStartMatch() => _roster.Count >= _minPlayersToStart;

        public bool IsHostConnection(NetworkConnectionToClient conn)
        {
            if (NetworkServer.localConnection != null)
                return ReferenceEquals(conn, NetworkServer.localConnection);

            return _dedicatedHostConnectionId != null && conn.connectionId == _dedicatedHostConnectionId.Value;
        }

        private LobbyPlayerState EnsurePlayer(NetworkConnectionToClient conn)
        {
            if (_roster.TryGetValue(conn.connectionId, out var existing))
                return existing;

            var playerId = _nextPlayerId++;
            var created = new LobbyPlayerState
            {
                ConnectionId = conn.connectionId,
                PlayerId = playerId,
                Name = $"Player {playerId}",
                Ready = false
            };

            _roster[conn.connectionId] = created;
            return created;
        }
    }
}
