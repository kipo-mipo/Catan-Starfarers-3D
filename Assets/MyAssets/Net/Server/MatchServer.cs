using Mirror;
using MyAssets.GameCore;
using UnityEngine;

namespace MyAssets.Net
{
    public sealed class MatchServer : NetworkBehaviour
    {
        public SessionState SessionState { get; private set; } = SessionState.Lobby;

        private GameState _state;
        private RulesEngine _rules;

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkServer.RegisterHandler<StartMatchRequest>(OnStartMatchRequest);
            NetworkServer.RegisterHandler<MoveShipRequest>(OnMoveShipRequest);
        }

        private void OnStartMatchRequest(NetworkConnectionToClient conn, StartMatchRequest msg)
        {
            if (SessionState != SessionState.Lobby) return;

            // TODO: roster lock, seat assignment, scene switch
            // For now, just init core.
            InitCoreForMatch(seed: msg.Seed);
            SessionState = SessionState.InMatch;

            BroadcastEvents(_rules.Apply(new StartMatchAction(msg.Seed)));
        }

        private void OnMoveShipRequest(NetworkConnectionToClient conn, MoveShipRequest msg)
        {
            if (SessionState != SessionState.InMatch) return;

            var action = new MoveShipAction(
                new PlayerId(msg.Player),
                new ShipId(msg.Ship),
                new NodeId(msg.FromNode),
                new NodeId(msg.ToNode)
            );

            BroadcastEvents(_rules.Apply(action));
        }

        private void InitCoreForMatch(int seed)
        {
            // TODO: get these from shared assets + exporter (Client side will have same data)
            // For server build (Unity headless), you can also load ScriptableObjects from Resources.
            var boardDef = new BoardDefinitionData();
            var tiles = new TileLibraryData();
            var setup = new SetupDefinitionData();

            _state = new GameState(boardDef, setup, tiles);
            _rules = new RulesEngine(_state);
        }

        private void BroadcastEvents(System.Collections.Generic.List<IGameEvent> events)
        {
            foreach (var e in events)
                NetworkServer.SendToAll(ToMessage(e));
        }

        private CoreEventMessage ToMessage(IGameEvent e)
        {
            return e switch
            {
                MatchStartedEvent ms => new CoreEventMessage { EventType = EventTypes.MatchStarted, A = ms.Seed },
                ShipMovedEvent sm => new CoreEventMessage { EventType = EventTypes.ShipMoved, A = sm.Player.Value, B = sm.Ship.Value, C = sm.From.Value, D = sm.To.Value },
                SectorRevealedEvent sr => new CoreEventMessage { EventType = EventTypes.SectorRevealed, A = sr.Slot.Value, B = sr.Tile.Value, C = (int)sr.Type },
                ActionRejectedEvent r => new CoreEventMessage { EventType = EventTypes.Rejected /* string later */ },
                _ => new CoreEventMessage { EventType = EventTypes.Rejected }
            };
        }
    }
}
