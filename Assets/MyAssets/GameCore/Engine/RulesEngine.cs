using System;
using System.Collections.Generic;
using System.Linq;

namespace MyAssets.GameCore
{
    public sealed class RulesEngine
    {
        private readonly GameState _state;

        public RulesEngine(GameState state) => _state = state;

        public List<IGameEvent> Apply(IGameAction action)
        {
            var ev = new List<IGameEvent>();

            switch (action)
            {
                case StartMatchAction start:
                    _state.Phase = MatchPhase.Setup;
                    BeginSetupOrder(start.Seed);
                    ev.Add(new MatchStartedEvent(start.Seed));
                    if (_state.SetupTurnOrder.Count > 0)
                        ev.Add(new SetupStepChangedEvent(_state.CurrentSetupRound, _state.CurrentSetupPlayer));
                    RevealFromAllShips(ev);
                    return ev;

                case ChooseStartingNodeAction choose:
                    return ApplySetupColonyChoice(choose, ev);

                case CompleteSetupSpaceportShipAction complete:
                    return ApplySetupSpaceportShipChoice(complete, ev);

                case MoveShipAction move:
                    if (_state.Phase != MatchPhase.Turn)
                        return Reject(ev, "Not in Turn phase.");

                    if (!_state.ShipOwners.TryGetValue(move.Ship, out var owner) || !owner.Equals(move.Player))
                        return Reject(ev, "Not your ship.");

                    if (!_state.ShipPositions.TryGetValue(move.Ship, out var pos) || !pos.Equals(move.From))
                        return Reject(ev, "Ship is not at From node.");

                    if (!_state.Graph.IsPassable(move.From))
                        return Reject(ev, "Ship is on a blocked node.");

                    if (!_state.Graph.IsPassable(move.To))
                        return Reject(ev, "Cannot move onto a blocked node.");

                    if (_state.IsShipOccupied(move.To))
                        return Reject(ev, "Another ship is already there.");

                    if (!_state.Graph.AreAdjacent(move.From, move.To))
                        return Reject(ev, "Nodes are not adjacent.");

                    _state.ShipPositions[move.Ship] = move.To;
                    ev.Add(new ShipMovedEvent(move.Player, move.Ship, move.From, move.To));

                    RevealFromNode(move.To, ev);
                    return ev;

                case EndTurnAction end:
                    if (_state.Phase != MatchPhase.Turn)
                        return Reject(ev, "Not in Turn phase.");
                    return ev;

                case SettleNextToPlanetAction settle:
                    return ev;

                default:
                    return Reject(ev, "Unknown action.");
            }
        }

        private List<IGameEvent> ApplySetupColonyChoice(ChooseStartingNodeAction choose, List<IGameEvent> ev)
        {
            if (_state.Phase != MatchPhase.Setup)
                return Reject(ev, "Not in Setup phase.");

            if (_state.CurrentSetupRound == SetupRound.SpaceportShipUpgrade)
                return Reject(ev, "Round 4 requires selecting one of your colonies, then an adjacent ship site.");

            if (!IsCurrentSetupPlayer(choose.Player))
                return Reject(ev, "Not this player's setup turn.");

            if (!_state.Graph.HasNode(choose.Node))
                return Reject(ev, "Invalid node.");

            if (!_state.Graph.IsLandable(choose.Node))
                return Reject(ev, "Cannot place a colony on a planet sector slot or blocked node.");

            if (_state.IsStructureOccupied(choose.Node))
                return Reject(ev, "A colony or spaceport is already on that node.");

            _state.ColonyOwners[choose.Node] = choose.Player;

            if (!_state.StartingNodeByPlayer.ContainsKey(choose.Player))
                _state.StartingNodeByPlayer[choose.Player] = choose.Node;

            ev.Add(new ColonyPlacedEvent(choose.Player, choose.Node));
            AdvanceSetup(ev);
            return ev;
        }

        private List<IGameEvent> ApplySetupSpaceportShipChoice(CompleteSetupSpaceportShipAction complete, List<IGameEvent> ev)
        {
            if (_state.Phase != MatchPhase.Setup)
                return Reject(ev, "Not in Setup phase.");

            if (_state.CurrentSetupRound != SetupRound.SpaceportShipUpgrade)
                return Reject(ev, "Spaceports and setup ships are only placed in setup round 4.");

            if (!IsCurrentSetupPlayer(complete.Player))
                return Reject(ev, "Not this player's setup turn.");

            if (!_state.ColonyOwners.TryGetValue(complete.ColonyNode, out var colonyOwner) || !colonyOwner.Equals(complete.Player))
                return Reject(ev, "You must upgrade one of your own colonies.");

            if (!_state.Graph.IsLandable(complete.ShipNode))
                return Reject(ev, "Cannot place a ship on that node.");

            if (!_state.Graph.AreAdjacent(complete.ColonyNode, complete.ShipNode))
                return Reject(ev, "Setup ship must be placed beside the new spaceport.");

            if (_state.IsStructureOccupied(complete.ShipNode))
                return Reject(ev, "Cannot place a setup ship on a colony or spaceport.");

            if (_state.IsShipOccupied(complete.ShipNode))
                return Reject(ev, "Another ship is already there.");

            if (!SetupUpgradeAvailable(complete.Upgrade))
                return Reject(ev, "That setup bonus upgrade is no longer available.");

            _state.ColonyOwners.Remove(complete.ColonyNode);
            _state.SpaceportOwners[complete.ColonyNode] = complete.Player;

            var ship = CreateSetupShipId(complete.Player);
            _state.ShipOwners[ship] = complete.Player;
            _state.ShipPositions[ship] = complete.ShipNode;
            _state.ShipTypes[ship] = complete.ShipType;
            _state.SetupUpgradeByPlayer[complete.Player] = complete.Upgrade;

            ev.Add(new SpaceportPlacedEvent(complete.Player, complete.ColonyNode));
            ev.Add(new ShipPlacedEvent(complete.Player, ship, complete.ShipNode));
            ev.Add(new SetupUpgradeGrantedEvent(complete.Player, complete.Upgrade));
            RevealFromNode(complete.ShipNode, ev);

            AdvanceSetup(ev);
            return ev;
        }

        private bool SetupUpgradeAvailable(UpgradeType upgrade)
        {
            int used = _state.SetupUpgradeByPlayer.Values.Count(u => u == upgrade);
            int limit = upgrade == UpgradeType.Booster ? 2 : 1;
            return used < limit;
        }

        private void BeginSetupOrder(int seed)
        {
            _state.CurrentSetupRound = SetupRound.FirstColony;
            _state.CurrentSetupTurnIndex = 0;
            _state.SetupTurnOrder.Clear();

            var rng = new Random(seed);
            var rolled = _state.SetupPlayers
                .Select(p => new { Player = p, Roll = rng.Next(1, 7) + rng.Next(1, 7) })
                .OrderByDescending(x => x.Roll)
                .ThenBy(x => x.Player.Value)
                .Select(x => x.Player)
                .ToList();

            foreach (var p in rolled)
                _state.SetupTurnOrder.Add(p);

            _state.SetupStartingPlayer = _state.SetupTurnOrder.Count > 0 ? _state.SetupTurnOrder[0] : new PlayerId(-1);
        }

        private bool IsCurrentSetupPlayer(PlayerId player) =>
            _state.SetupTurnOrder.Count > 0 && _state.CurrentSetupPlayer.Equals(player);

        private void AdvanceSetup(List<IGameEvent> ev)
        {
            if (_state.SetupTurnOrder.Count == 0)
                return;

            var order = _state.GetCurrentSetupOrder();
            if (_state.CurrentSetupTurnIndex + 1 < order.Count)
            {
                _state.CurrentSetupTurnIndex++;
                ev.Add(new SetupStepChangedEvent(_state.CurrentSetupRound, _state.CurrentSetupPlayer));
                return;
            }

            if (_state.CurrentSetupRound != SetupRound.SpaceportShipUpgrade)
            {
                _state.CurrentSetupRound = (SetupRound)((int)_state.CurrentSetupRound + 1);
                _state.CurrentSetupTurnIndex = 0;
                ev.Add(new SetupStepChangedEvent(_state.CurrentSetupRound, _state.CurrentSetupPlayer));
                return;
            }

            foreach (var p in _state.SetupTurnOrder)
            {
                _state.StartingResourcesGranted.Add(p);
                _state.FameMedalGranted.Add(p);
                ev.Add(new StartingResourcesGrantedEvent(p, 3));
                ev.Add(new FameMedalGrantedEvent(p));
            }

            _state.Phase = MatchPhase.Turn;
            ev.Add(new SetupCompletedEvent(_state.SetupStartingPlayer));
        }

        private static ShipId CreateSetupShipId(PlayerId player) => new ShipId(player.Value * 100);

        private static List<IGameEvent> Reject(List<IGameEvent> ev, string reason)
        {
            ev.Add(new ActionRejectedEvent(reason));
            return ev;
        }

        private void RevealFromAllShips(List<IGameEvent> ev)
        {
            foreach (var kv in _state.ShipPositions)
                RevealFromNode(kv.Value, ev);
        }

        private void RevealFromNode(NodeId node, List<IGameEvent> ev)
        {
            // TODO: vision + graph reveal
        }
    }
}
