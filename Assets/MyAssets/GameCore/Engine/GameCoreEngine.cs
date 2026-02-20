using System.Collections.Generic;

namespace MyAssets.GameCore
{
    /// <summary>
    /// Thin wrapper around core rules. Keeps older callers compiling.
    /// Prefer using RulesEngine directly for new code.
    /// </summary>
    public sealed class GameCoreEngine
    {
        public GameState State { get; }

        public GameCoreEngine(GameState state) => State = state;

        public List<IGameEvent> Apply(IGameAction action)
        {
            var events = new List<IGameEvent>();

            switch (action)
            {
                case StartMatchAction start:
                    // Lobby is a NET/session concern. Core starts in Setup.
                    State.Phase = MatchPhase.Setup;
                    events.Add(new MatchStartedEvent(start.Seed));
                    return events;

                case MoveShipAction move:
                    if (State.Phase != MatchPhase.Turn)
                    {
                        events.Add(new ActionRejectedEvent("Not in Turn phase."));
                        return events;
                    }

                    if (!State.ShipPositions.TryGetValue(move.Ship, out var pos) || !pos.Equals(move.From))
                    {
                        events.Add(new ActionRejectedEvent("Ship is not at From node."));
                        return events;
                    }

                    if (!State.Graph.AreAdjacent(move.From, move.To))
                    {
                        events.Add(new ActionRejectedEvent("Nodes are not adjacent."));
                        return events;
                    }

                    State.ShipPositions[move.Ship] = move.To;
                    events.Add(new ShipMovedEvent(move.Player, move.Ship, move.From, move.To));
                    return events;

                default:
                    events.Add(new ActionRejectedEvent($"Unknown action: {action.GetType().Name}"));
                    return events;
            }
        }
    }
}
