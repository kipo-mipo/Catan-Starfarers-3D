using System.Collections.Generic;

namespace MyAssets.GameCore
{
    public sealed class GameCoreEngine
    {
        public GameState State { get; }

        public GameCoreEngine(GameState state) => State = state;

        public List<IGameEvent> Apply(IGameAction action)
        {
            var events = new List<IGameEvent>();

            switch (action)
            {
                case StartGameAction start:
                    if (State.Phase != MatchPhase.Lobby)
                    {
                        events.Add(new ActionRejectedEvent("Game already started."));
                        return events;
                    }
                    State.Phase = MatchPhase.Setup;
                    events.Add(new GameStartedEvent(start.Seed));
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
                    if (!State.Board.AreAdjacent(move.From, move.To))
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
