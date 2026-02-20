using System.Collections.Generic;

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
                    ev.Add(new MatchStartedEvent(start.Seed));
                    RevealFromAllShips(ev);
                    return ev;

                case MoveShipAction move:
                    if (_state.Phase != MatchPhase.Turn)
                        return Reject(ev, "Not in Turn phase.");

                    if (!_state.ShipOwners.TryGetValue(move.Ship, out var owner) || !owner.Equals(move.Player))
                        return Reject(ev, "Not your ship.");

                    if (!_state.ShipPositions.TryGetValue(move.Ship, out var pos) || !pos.Equals(move.From))
                        return Reject(ev, "Ship is not at From node.");

                    if (!_state.Graph.AreAdjacent(move.From, move.To))
                        return Reject(ev, "Nodes are not adjacent.");

                    _state.ShipPositions[move.Ship] = move.To;
                    ev.Add(new ShipMovedEvent(move.Player, move.Ship, move.From, move.To));

                    RevealFromNode(move.To, ev);
                    return ev;

                case EndTurnAction:
                    return ev;

                default:
                    return Reject(ev, $"Unknown action: {action.GetType().Name}");
            }
        }

        private List<IGameEvent> Reject(List<IGameEvent> ev, string reason)
        {
            ev.Add(new ActionRejectedEvent(reason));
            return ev;
        }

        // Reveal rule: slot revealed when a ship is on a neighboring node.
        private void RevealFromNode(NodeId node, List<IGameEvent> ev)
        {
            foreach (var slot in _state.BoardDef.Slots)
            {
                if (_state.RevealedSlots.Contains(slot.Id)) continue;
                if (!slot.NeighborNodes.Contains(node)) continue;

                _state.RevealedSlots.Add(slot.Id);

                // Sector reveal is authoritative; mapping to a concrete piece is handled by setup.
                // During transition we fall back to using TileId as the piece id if SlotToPiece isn't populated.
                if (!_state.SlotToPiece.TryGetValue(slot.Id, out var pieceId))
                {
                    var tileId = _state.SlotToTile[slot.Id];
                    pieceId = new SectorPieceId(tileId.Value);
                    _state.SlotToPiece[slot.Id] = pieceId;
                }

                if (!_state.SlotToRotation.TryGetValue(slot.Id, out var rotation))
                {
                    rotation = 0;
                    _state.SlotToRotation[slot.Id] = rotation;
                }

                ev.Add(new SectorRevealedEvent(slot.Id, pieceId, rotation));
            }
        }

        private void RevealFromAllShips(List<IGameEvent> ev)
        {
            foreach (var kvp in _state.ShipPositions)
                RevealFromNode(kvp.Value, ev);
        }
    }
}
