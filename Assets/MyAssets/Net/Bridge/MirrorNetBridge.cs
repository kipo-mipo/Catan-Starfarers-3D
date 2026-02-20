using System;
using Mirror;
using UnityEngine;
using MyAssets.GameCore;
using MyAssets.GameCore.Net;

namespace MyAssets.Net.Bridge
{
    /// <summary>
    /// Mirror implementation of INetBridge. Put this on a prefab in your bootstrap scene.
    /// </summary>
    public sealed class MirrorNetBridge : MonoBehaviour, INetBridge
    {
        public NetRole Role { get; private set; } = NetRole.None;
        public bool IsConnected => NetworkClient.isConnected || NetworkServer.active;

        public event Action<IGameEvent> OnGameEvent;
        public event Action<string> OnNetStatus;

        [SerializeField] private NetworkManager networkManager;

        private bool _handlersRegistered;

        private void Awake()
        {
            if (networkManager == null)
                networkManager = NetworkManager.singleton;

            RegisterClientHandlers();
            DontDestroyOnLoad(gameObject);
        }

        public void StartHost()
        {
            if (networkManager == null)
            {
                OnNetStatus?.Invoke("No NetworkManager in scene.");
                return;
            }

            Role = NetRole.Host;
            networkManager.StartHost();
            OnNetStatus?.Invoke("Host started");
        }

        public void StartClient(string address)
        {
            if (networkManager == null)
            {
                OnNetStatus?.Invoke("No NetworkManager in scene.");
                return;
            }

            Role = NetRole.Client;
            networkManager.networkAddress = address;
            networkManager.StartClient();
            OnNetStatus?.Invoke($"Client connecting to {address}");
        }

        public void Disconnect()
        {
            if (networkManager == null) return;

            if (NetworkServer.active || NetworkClient.active)
                networkManager.StopHost();

            Role = NetRole.None;
            OnNetStatus?.Invoke("Disconnected");
        }

        public void RequestStartMatch(int seed)
        {
            if (!NetworkClient.active)
            {
                OnNetStatus?.Invoke("Not connected; can't start match.");
                return;
            }
            NetworkClient.Send(new StartMatchRequest { Seed = seed });
        }

        public void RequestMoveShip(int playerId, int shipId, int fromNodeId, int toNodeId)
        {
            if (!NetworkClient.active)
            {
                OnNetStatus?.Invoke("Not connected; can't move ship.");
                return;
            }

            NetworkClient.Send(new MoveShipRequest
            {
                Player = playerId,
                Ship = shipId,
                FromNode = fromNodeId,
                ToNode = toNodeId
            });
        }

        private void RegisterClientHandlers()
        {
            if (_handlersRegistered) return;
            _handlersRegistered = true;

            NetworkClient.RegisterHandler<CoreEventMessage>(msg =>
            {
                var e = Translate(msg);
                if (e != null) OnGameEvent?.Invoke(e);
            });
        }

        private static IGameEvent Translate(CoreEventMessage msg)
        {
            return msg.EventType switch
            {
                EventTypes.MatchStarted => new MatchStartedEvent(msg.A),
                EventTypes.ShipMoved => new ShipMovedEvent(new PlayerId(msg.A), new ShipId(msg.B), new NodeId(msg.C), new NodeId(msg.D)),
                EventTypes.SectorRevealed => new SectorRevealedEvent(new SlotId(msg.A), new SectorPieceId(msg.B), msg.C),
                EventTypes.Rejected => new ActionRejectedEvent("Rejected"),
                _ => new ActionRejectedEvent($"Unhandled net event type {msg.EventType}")
            };
        }
    }
}
