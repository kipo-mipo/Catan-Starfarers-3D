using System;
using Mirror;
using UnityEngine;
using MyAssets.GameCore;
using MyAssets.GameCore.Net;

namespace MyAssets.Net.Bridge
{
    public sealed class MirrorNetBridge : MonoBehaviour, INetBridge
    {
        public NetRole Role { get; private set; } = NetRole.None;
        public bool IsConnected => NetworkClient.isConnected || NetworkServer.active;
        public int LocalPlayerId => _assignedPlayerId;

        public event Action<IGameEvent> OnGameEvent;
        public event Action<LobbyStateInfo> OnLobbyState;
        public event Action<string> OnNetStatus;

        [SerializeField] private NetworkManager networkManager;

        private int _assignedPlayerId = -1;

        private bool _handlersRegistered;
        private bool _lifecycleHooked;

        private void Awake()
        {
            EnsureNetworkManager();
            RegisterClientHandlers();
            HookClientLifecycle();
            DontDestroyOnLoad(gameObject);
        }

        private void EnsureNetworkManager()
        {
            if (networkManager != null) return;

            networkManager = GetComponent<NetworkManager>();
            if (networkManager != null) return;

            networkManager = NetworkManager.singleton;
            if (networkManager != null) return;

            networkManager = FindAnyObjectByType<NetworkManager>();
        }

        public void StartHost()
        {
            EnsureNetworkManager();
            if (networkManager == null) { OnNetStatus?.Invoke("No NetworkManager found"); return; }

            Role = NetRole.Host;
            networkManager.StartHost();
            OnNetStatus?.Invoke("Host starting...");
        }

        public void StartClient(string address)
        {
            EnsureNetworkManager();
            if (networkManager == null) { OnNetStatus?.Invoke("No NetworkManager found"); return; }

            Role = NetRole.Client;
            networkManager.networkAddress = address;
            networkManager.StartClient();
            OnNetStatus?.Invoke($"Client connecting to {address}...");
        }

        public void Disconnect()
        {
            EnsureNetworkManager();
            if (networkManager == null) return;

            if (NetworkServer.active || NetworkClient.active)
                networkManager.StopHost();

            Role = NetRole.None;
            _assignedPlayerId = -1;
            OnNetStatus?.Invoke("Disconnected");
        }

        public void RequestJoinLobby(string playerName)
        {
            if (!NetworkClient.active) { OnNetStatus?.Invoke("Not connected; can't join lobby."); return; }
            NetworkClient.Send(new JoinLobbyRequest { PlayerName = playerName ?? string.Empty });
        }

        public void SetReady(bool ready)
        {
            if (!NetworkClient.active) { OnNetStatus?.Invoke("Not connected; can't set ready."); return; }
            NetworkClient.Send(new SetReadyRequest { Ready = ready });
        }

        public void RequestStartMatch(int seed)
        {
            if (!NetworkClient.active) { OnNetStatus?.Invoke("Not connected; can't start match."); return; }
            NetworkClient.Send(new StartMatchRequest { Seed = seed });
        }

        public void RequestChooseStartNode(int nodeId)
        {
            if (!NetworkClient.active) { OnNetStatus?.Invoke("Not connected; can't choose start."); return; }
            NetworkClient.Send(new ChooseStartNodeRequest { NodeId = nodeId });
        }

        public void RequestCompleteSetupSpaceportShip(int colonyNodeId, int shipNodeId, SetupShipType shipType, UpgradeType upgrade)
        {
            if (!NetworkClient.active) { OnNetStatus?.Invoke("Not connected; can't complete setup upgrade."); return; }
            NetworkClient.Send(new CompleteSetupSpaceportShipRequest
            {
                ColonyNode = colonyNodeId,
                ShipNode = shipNodeId,
                ShipType = (byte)shipType,
                Upgrade = (byte)upgrade
            });
        }

        public void RequestMoveShip(int playerId, int shipId, int fromNodeId, int toNodeId)
        {
            if (!NetworkClient.active) { OnNetStatus?.Invoke("Not connected; can't move ship."); return; }
            NetworkClient.Send(new MoveShipRequest { Player = playerId, Ship = shipId, FromNode = fromNodeId, ToNode = toNodeId });
        }

        private void RegisterClientHandlers()
        {
            if (_handlersRegistered) return;
            _handlersRegistered = true;

            NetworkClient.RegisterHandler<AssignedPlayerIdMessage>(OnAssignedPlayerIdMessage);
            NetworkClient.RegisterHandler<LobbyStateMessage>(OnLobbyStateMessage);

            NetworkClient.RegisterHandler<CoreEventMessage>(msg =>
            {
                var e = Translate(msg);
                if (e != null) OnGameEvent?.Invoke(e);
            });
        }

        private void HookClientLifecycle()
        {
            if (_lifecycleHooked) return;
            _lifecycleHooked = true;

            NetworkClient.OnConnectedEvent += () => OnNetStatus?.Invoke(Role == NetRole.Host ? $"Host connected (local client id={LocalPlayerId})" : $"Client connected id={LocalPlayerId}");
            NetworkClient.OnDisconnectedEvent += () => OnNetStatus?.Invoke("Client disconnected");
            NetworkClient.OnErrorEvent += (err, msg) => OnNetStatus?.Invoke($"Client error: {err} {msg}");
        }

        private void OnAssignedPlayerIdMessage(AssignedPlayerIdMessage msg)
        {
            _assignedPlayerId = msg.PlayerId;
            OnNetStatus?.Invoke($"Assigned local player id={_assignedPlayerId}");
        }

        private void OnLobbyStateMessage(LobbyStateMessage msg)
        {
            var players = msg.Players ?? Array.Empty<LobbyPlayerState>();
            var infos = new LobbyPlayerInfo[players.Length];
            for (int i = 0; i < players.Length; i++)
                infos[i] = new LobbyPlayerInfo(players[i].ConnectionId, players[i].PlayerId, players[i].Name, players[i].Ready);

            OnLobbyState?.Invoke(new LobbyStateInfo(infos, msg.CanStart));
        }

        private static IGameEvent Translate(CoreEventMessage msg)
        {
            return msg.EventType switch
            {
                EventTypes.MatchStarted => new MatchStartedEvent(msg.A),
                EventTypes.ShipMoved => new ShipMovedEvent(new PlayerId(msg.A), new ShipId(msg.B), new NodeId(msg.C), new NodeId(msg.D)),
                EventTypes.SectorRevealed => new SectorRevealedEvent(new SlotId(msg.A), new SectorPieceId(msg.B), msg.C),

                EventTypes.ShipPlaced => new ShipPlacedEvent(new PlayerId(msg.A), new ShipId(msg.B), new NodeId(msg.C)),
                EventTypes.SetupCompleted => new SetupCompletedEvent(new PlayerId(msg.A)),
                EventTypes.SetupStepChanged => new SetupStepChangedEvent((SetupRound)msg.A, new PlayerId(msg.B)),
                EventTypes.ColonyPlaced => new ColonyPlacedEvent(new PlayerId(msg.A), new NodeId(msg.B)),
                EventTypes.SpaceportPlaced => new SpaceportPlacedEvent(new PlayerId(msg.A), new NodeId(msg.B)),
                EventTypes.SetupUpgradeGranted => new SetupUpgradeGrantedEvent(new PlayerId(msg.A), (UpgradeType)msg.B),
                EventTypes.StartingResourcesGranted => new StartingResourcesGrantedEvent(new PlayerId(msg.A), msg.B),
                EventTypes.FameMedalGranted => new FameMedalGrantedEvent(new PlayerId(msg.A)),

                EventTypes.Rejected => new ActionRejectedEvent(string.IsNullOrWhiteSpace(msg.Text) ? "Rejected" : msg.Text),
                _ => new ActionRejectedEvent($"Unhandled net event type {msg.EventType}")
            };
        }
    }
}
