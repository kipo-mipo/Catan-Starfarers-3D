using System.Collections.Generic;
using MyAssets.GameCore;

namespace MyAssets.Net
{
    internal sealed class NetMatchOrchestrator
    {
        public SessionState SessionState { get; private set; } = SessionState.Lobby;

        private GameState _state;
        private RulesEngine _rules;

        public List<IGameEvent> StartMatch(int seed, List<PlayerId> players)
        {
            if (SessionState != SessionState.Lobby)
                return new List<IGameEvent> { new ActionRejectedEvent("Match already started") };

            InitCoreForMatch(seed, players);

            SessionState = SessionState.InMatch;
            return _rules.Apply(new StartMatchAction(seed));
        }

        public List<IGameEvent> ChooseStartNode(PlayerId player, int nodeId)
        {
            if (SessionState != SessionState.InMatch)
                return new List<IGameEvent> { new ActionRejectedEvent("Not in match") };

            return _rules.Apply(new ChooseStartingNodeAction(player, new NodeId(nodeId)));
        }

        public List<IGameEvent> CompleteSetupSpaceportShip(PlayerId player, CompleteSetupSpaceportShipRequest msg)
        {
            if (SessionState != SessionState.InMatch)
                return new List<IGameEvent> { new ActionRejectedEvent("Not in match") };

            var action = new CompleteSetupSpaceportShipAction(
                player,
                new NodeId(msg.ColonyNode),
                new NodeId(msg.ShipNode),
                (SetupShipType)msg.ShipType,
                (UpgradeType)msg.Upgrade
            );

            return _rules.Apply(action);
        }

        public List<IGameEvent> MoveShip(MoveShipRequest msg)
        {
            if (SessionState != SessionState.InMatch)
                return new List<IGameEvent> { new ActionRejectedEvent("Not in match") };

            var action = new MoveShipAction(
                new PlayerId(msg.Player),
                new ShipId(msg.Ship),
                new NodeId(msg.FromNode),
                new NodeId(msg.ToNode)
            );

            return _rules.Apply(action);
        }

        private void InitCoreForMatch(int seed, List<PlayerId> players)
        {
            // TODO: replace with real data (exported board + tiles + setup)
            var boardDef = BoardGenerator.Generate(hexSize: 1f);
            var tiles = new TileLibraryData();
            var setup = new SetupDefinitionData();

            _state = new GameState(boardDef, setup, tiles);

            _state.SetupPlayers.Clear();
            foreach (var p in players)
                _state.SetupPlayers.Add(p);

            _rules = new RulesEngine(_state);
        }
    }
}
