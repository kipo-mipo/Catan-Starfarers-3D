using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyAssets.GameCore;
using MyAssets.GameCore.Net;

namespace MyAssets.Client.Net
{
    /// <summary>
    /// Client-side projection of authoritative server state.
    ///
    /// Lives on the persistent NetworkRoot and binds to NetService.Bridge at runtime,
    /// because the game scene cannot serialize references to objects carried across
    /// scenes by DontDestroyOnLoad.
    /// </summary>
    public sealed class ClientSessionModel : MonoBehaviour
    {
        [Header("Local client")]
        [Tooltip("Fallback only. The server should assign this from the lobby seat id through MirrorNetBridge at runtime.")]
        [SerializeField] private int localPlayerId = -1;

        public int LocalPlayerId
        {
            get => localPlayerId;
            set
            {
                if (localPlayerId == value)
                    return;

                localPlayerId = value;
                Changed?.Invoke();
            }
        }

        public MatchPhase Phase { get; private set; } = MatchPhase.Setup;
        public SetupRound CurrentSetupRound { get; private set; } = SetupRound.FirstColony;
        public int CurrentSetupPlayerId { get; private set; } = -1;
        public int FirstTurnPlayerId { get; private set; } = -1;
        public int LastMatchSeed { get; private set; }

        public readonly Dictionary<int, int> ShipToNode = new();
        public readonly Dictionary<int, int> ShipToPlayer = new();
        public readonly Dictionary<int, int> ColonyToPlayer = new();
        public readonly Dictionary<int, int> SpaceportToPlayer = new();
        public readonly Dictionary<int, UpgradeType> SetupUpgradeByPlayer = new();
        public readonly Dictionary<int, int> StartingResourcesByPlayer = new();
        public readonly HashSet<int> FameMedalPlayers = new();

        public bool IsSetupPhase => Phase == MatchPhase.Setup;
        public bool IsTurnPhase => Phase == MatchPhase.Turn;
        public bool IsLocalSetupTurn => IsSetupPhase && CurrentSetupPlayerId == LocalPlayerId;
        public bool IsLocalTurn => IsTurnPhase && CurrentSetupPlayerId == LocalPlayerId;
        public bool LocalPlacedStartShip => ShipToPlayer.ContainsValue(LocalPlayerId);
        public bool LocalHasSpaceport => SpaceportToPlayer.ContainsValue(LocalPlayerId);
        public bool LocalHasSetupUpgrade => SetupUpgradeByPlayer.ContainsKey(LocalPlayerId);

        public event Action Changed;
        public event Action<ActionRejectedEvent> ActionRejected;

        private INetBridge _bridge;
        private Coroutine _hookRoutine;

        private void OnEnable()
        {
            _hookRoutine = StartCoroutine(HookBridgeWhenReady());
        }

        private void OnDisable()
        {
            if (_hookRoutine != null)
            {
                StopCoroutine(_hookRoutine);
                _hookRoutine = null;
            }

            UnhookBridge();
        }

        private IEnumerator HookBridgeWhenReady()
        {
            while (enabled)
            {
                var bridge = NetService.Bridge;
                if (bridge != null)
                {
                    HookBridge(bridge);
                    yield break;
                }

                yield return null;
            }
        }

        private void HookBridge(INetBridge bridge)
        {
            if (_bridge == bridge)
                return;

            UnhookBridge();
            _bridge = bridge;
            _bridge.OnGameEvent += OnGameEvent;
            _bridge.OnLobbyState += OnLobbyState;
            SyncLocalPlayerIdFromBridge();
            Debug.Log($"[ClientSessionModel] Hooked NetService.Bridge. LocalPlayerId={LocalPlayerId}");
        }

        private void UnhookBridge()
        {
            if (_bridge == null)
                return;

            _bridge.OnGameEvent -= OnGameEvent;
            _bridge.OnLobbyState -= OnLobbyState;
            _bridge = null;
        }

        private void Update()
        {
            // Mirror may not know the local connection id on the exact frame the bridge is hooked.
            SyncLocalPlayerIdFromBridge();
        }

        private void OnLobbyState(LobbyStateInfo lobby)
        {
            SyncLocalPlayerIdFromBridge();
            Changed?.Invoke();
        }

        private void SyncLocalPlayerIdFromBridge()
        {
            if (_bridge == null)
                return;

            int bridgePlayerId = _bridge.LocalPlayerId;
            if (bridgePlayerId >= 0 && bridgePlayerId != localPlayerId)
            {
                localPlayerId = bridgePlayerId;
                Changed?.Invoke();
            }
        }

        private void OnGameEvent(IGameEvent e)
        {
            SyncLocalPlayerIdFromBridge();
            bool changed = true;

            switch (e)
            {
                case MatchStartedEvent started:
                    ResetForMatch(started.Seed);
                    break;

                case SetupStepChangedEvent step:
                    Phase = MatchPhase.Setup;
                    CurrentSetupRound = step.Round;
                    CurrentSetupPlayerId = step.CurrentPlayer.Value;
                    break;

                case ColonyPlacedEvent colony:
                    ColonyToPlayer[colony.Node.Value] = colony.Player.Value;
                    break;

                case SpaceportPlacedEvent spaceport:
                    ColonyToPlayer.Remove(spaceport.Node.Value);
                    SpaceportToPlayer[spaceport.Node.Value] = spaceport.Player.Value;
                    break;

                case ShipPlacedEvent ship:
                    ShipToNode[ship.Ship.Value] = ship.Node.Value;
                    ShipToPlayer[ship.Ship.Value] = ship.Player.Value;
                    break;

                case SetupUpgradeGrantedEvent upgrade:
                    SetupUpgradeByPlayer[upgrade.Player.Value] = upgrade.Upgrade;
                    break;

                case StartingResourcesGrantedEvent resources:
                    StartingResourcesByPlayer[resources.Player.Value] = resources.Count;
                    break;

                case FameMedalGrantedEvent fame:
                    FameMedalPlayers.Add(fame.Player.Value);
                    break;

                case SetupCompletedEvent completed:
                    Phase = MatchPhase.Turn;
                    FirstTurnPlayerId = completed.FirstPlayer.Value;
                    CurrentSetupPlayerId = completed.FirstPlayer.Value;
                    break;

                case ShipMovedEvent moved:
                    ShipToNode[moved.Ship.Value] = moved.To.Value;
                    ShipToPlayer[moved.Ship.Value] = moved.Player.Value;
                    break;

                case ActionRejectedEvent rejected:
                    ActionRejected?.Invoke(rejected);
                    changed = false;
                    break;

                default:
                    changed = false;
                    break;
            }

            if (changed)
                Changed?.Invoke();
        }

        private void ResetForMatch(int seed)
        {
            LastMatchSeed = seed;
            Phase = MatchPhase.Setup;
            CurrentSetupRound = SetupRound.FirstColony;
            CurrentSetupPlayerId = -1;
            FirstTurnPlayerId = -1;

            ShipToNode.Clear();
            ShipToPlayer.Clear();
            ColonyToPlayer.Clear();
            SpaceportToPlayer.Clear();
            SetupUpgradeByPlayer.Clear();
            StartingResourcesByPlayer.Clear();
            FameMedalPlayers.Clear();
        }

        public bool IsLocalColony(int nodeId)
        {
            return ColonyToPlayer.TryGetValue(nodeId, out int owner) && owner == LocalPlayerId;
        }

        public bool IsLocalSpaceport(int nodeId)
        {
            return SpaceportToPlayer.TryGetValue(nodeId, out int owner) && owner == LocalPlayerId;
        }

        public bool TryGetShipNode(int shipId, out int nodeId)
        {
            return ShipToNode.TryGetValue(shipId, out nodeId);
        }
    }
}
