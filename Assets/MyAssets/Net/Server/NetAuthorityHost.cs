using System.Collections.Generic;
using Mirror;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Net
{
    public sealed class NetAuthorityHost : NetworkBehaviour
    {
        [Header("Lobby")]
        [SerializeField] private int minPlayersToStart = 2;

        private NetLobbyDirectory _lobby;
        private NetMatchOrchestrator _match;

        public override void OnStartServer()
        {
            base.OnStartServer();

            _lobby = new NetLobbyDirectory(minPlayersToStart);
            _match = new NetMatchOrchestrator();

            NetworkServer.OnConnectedEvent -= OnServerConnected;
            NetworkServer.OnConnectedEvent += OnServerConnected;

            NetworkServer.OnDisconnectedEvent -= OnServerDisconnected;
            NetworkServer.OnDisconnectedEvent += OnServerDisconnected;

            NetworkServer.RegisterHandler<JoinLobbyRequest>(OnJoinLobbyRequest);
            NetworkServer.RegisterHandler<SetReadyRequest>(OnSetReadyRequest);

            NetworkServer.RegisterHandler<StartMatchRequest>(OnStartMatchRequest);
            NetworkServer.RegisterHandler<ChooseStartNodeRequest>(OnChooseStartNodeRequest);
            NetworkServer.RegisterHandler<CompleteSetupSpaceportShipRequest>(OnCompleteSetupSpaceportShipRequest);
            NetworkServer.RegisterHandler<MoveShipRequest>(OnMoveShipRequest);
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            NetworkServer.OnConnectedEvent -= OnServerConnected;
            NetworkServer.OnDisconnectedEvent -= OnServerDisconnected;
        }

        private void OnServerConnected(NetworkConnectionToClient conn)
        {
            _lobby.OnServerConnected(conn);
            SendAssignedPlayerId(conn);
            BroadcastLobby();
        }

        private void OnServerDisconnected(NetworkConnectionToClient conn)
        {
            _lobby.OnServerDisconnected(conn);
            BroadcastLobby();
        }

        private void OnJoinLobbyRequest(NetworkConnectionToClient conn, JoinLobbyRequest msg)
        {
            _lobby.OnJoinLobby(conn, msg);
            SendAssignedPlayerId(conn);
            BroadcastLobby();
        }

        private void OnSetReadyRequest(NetworkConnectionToClient conn, SetReadyRequest msg)
        {
            _lobby.OnSetReady(conn, msg);
            SendAssignedPlayerId(conn);
            BroadcastLobby();
        }

        private void BroadcastLobby()
        {
            SendAssignedPlayerIdsToAll();
            NetworkServer.SendToAll(_lobby.BuildLobbyState());
        }

        private void SendAssignedPlayerIdsToAll()
        {
            foreach (var kv in NetworkServer.connections)
            {
                if (kv.Value != null)
                    SendAssignedPlayerId(kv.Value);
            }
        }

        private void SendAssignedPlayerId(NetworkConnectionToClient conn)
        {
            if (conn == null)
                return;

            if (_lobby.TryGetPlayerId(conn, out int playerId))
                conn.Send(new AssignedPlayerIdMessage { PlayerId = playerId });
        }

        private void OnStartMatchRequest(NetworkConnectionToClient conn, StartMatchRequest msg)
        {
            if (!_lobby.IsHostConnection(conn))
            {
                conn.Send(new CoreEventMessage { EventType = EventTypes.Rejected });
                return;
            }

            if (!_lobby.CanStartMatch())
            {
                conn.Send(new CoreEventMessage { EventType = EventTypes.Rejected });
                return;
            }

            // Stable player ids are lobby seats: 0, 1, 2, ...
            // Do NOT use Mirror connectionId as a game player id. It can be ugly/unstable.
            var playerIds = new List<PlayerId>(_lobby.PlayerCount);
            foreach (var playerId in _lobby.PlayerIds)
                playerIds.Add(new PlayerId(playerId));

            SendAssignedPlayerIdsToAll();
            var events = _match.StartMatch(msg.Seed, playerIds);
            BroadcastEvents(events);
        }

        private void OnChooseStartNodeRequest(NetworkConnectionToClient conn, ChooseStartNodeRequest msg)
        {
            if (!_lobby.TryGetPlayerId(conn, out int playerId))
            {
                conn.Send(new CoreEventMessage { EventType = EventTypes.Rejected });
                return;
            }

            var events = _match.ChooseStartNode(new PlayerId(playerId), msg.NodeId);
            BroadcastEvents(events);
        }

        private void OnCompleteSetupSpaceportShipRequest(NetworkConnectionToClient conn, CompleteSetupSpaceportShipRequest msg)
        {
            if (!_lobby.TryGetPlayerId(conn, out int playerId))
            {
                conn.Send(new CoreEventMessage { EventType = EventTypes.Rejected });
                return;
            }

            var events = _match.CompleteSetupSpaceportShip(new PlayerId(playerId), msg);
            BroadcastEvents(events);
        }

        private void OnMoveShipRequest(NetworkConnectionToClient conn, MoveShipRequest msg)
        {
            if (!_lobby.TryGetPlayerId(conn, out int playerId))
            {
                conn.Send(new CoreEventMessage { EventType = EventTypes.Rejected });
                return;
            }

            msg.Player = playerId; // server-authoritative owner; never trust the client payload
            var events = _match.MoveShip(msg);
            BroadcastEvents(events);
        }

        private static void BroadcastEvents(List<IGameEvent> events)
        {
            foreach (var e in events)
                NetworkServer.SendToAll(NetEventEncoder.ToMessage(e));
        }
    }
}
