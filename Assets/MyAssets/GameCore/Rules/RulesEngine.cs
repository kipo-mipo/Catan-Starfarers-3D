using System;
using System.Collections.Generic;

namespace GameCore
{
    public enum ActionKind
    {
        StartMatch,
        ConfirmSetup,
        PlaceColony,
        PlaceShip,
        MoveShip,
        EndTurn
    }

    public interface IGameAction
    {
        ActionKind Kind { get; }
        int PlayerId { get; }
    }

    public struct LegalAction
    {
        public ActionKind Kind;
        public string Label;

        public LegalAction(ActionKind kind, string label)
        {
            Kind = kind;
            Label = label;
        }
    }

    public interface IGameEvent { }

    public struct TurnAdvancedEvent : IGameEvent
    {
        public int NewCurrentPlayerId;
        public TurnAdvancedEvent(int newCurrentPlayerId) { NewCurrentPlayerId = newCurrentPlayerId; }
    }

    public struct ShipMovedEvent : IGameEvent
    {
        public int ShipId;
        public int FromNodeId;
        public int ToNodeId;

        public ShipMovedEvent(int shipId, int fromNodeId, int toNodeId)
        {
            ShipId = shipId;
            FromNodeId = fromNodeId;
            ToNodeId = toNodeId;
        }
    }

    public struct ColonyPlacedEvent : IGameEvent
    {
        public int NodeId;
        public int OwnerPlayerId;
        public ColonyKind Kind;

        public ColonyPlacedEvent(int nodeId, int ownerPlayerId, ColonyKind kind)
        {
            NodeId = nodeId;
            OwnerPlayerId = ownerPlayerId;
            Kind = kind;
        }
    }

    public struct ShipPlacedEvent : IGameEvent
    {
        public int ShipId;
        public int OwnerPlayerId;
        public int NodeId;

        public ShipPlacedEvent(int shipId, int ownerPlayerId, int nodeId)
        {
            ShipId = shipId;
            OwnerPlayerId = ownerPlayerId;
            NodeId = nodeId;
        }
    }

    public struct PhaseChangedEvent : IGameEvent
    {
        public MatchPhase Phase;
        public PhaseChangedEvent(MatchPhase phase) { Phase = phase; }
    }

    public struct ApplyResult
    {
        public bool Success;
        public string Error;
        public GameState State;
        public List<IGameEvent> Events;

        public static ApplyResult Reject(string error, GameState state)
        {
            return new ApplyResult
            {
                Success = false,
                Error = error,
                State = state,
                Events = new List<IGameEvent>()
            };
        }

        public static ApplyResult Accept(GameState state, List<IGameEvent> eventsList)
        {
            return new ApplyResult
            {
                Success = true,
                Error = "",
                State = state,
                Events = eventsList ?? new List<IGameEvent>()
            };
        }
    }

    // Actions (simple structs/classes; split into files later if you want)
    public struct StartMatchAction : IGameAction
    {
        public int PlayerId { get; private set; }
        public int Seed { get; private set; }
        public ActionKind Kind { get { return ActionKind.StartMatch; } }

        public StartMatchAction(int playerId, int seed)
        {
            PlayerId = playerId;
            Seed = seed;
        }
    }

    public struct ConfirmSetupAction : IGameAction
    {
        public int PlayerId { get; private set; }
        public ActionKind Kind { get { return ActionKind.ConfirmSetup; } }

        public ConfirmSetupAction(int playerId) { PlayerId = playerId; }
    }

    public struct PlaceColonyAction : IGameAction
    {
        public int PlayerId { get; private set; }
        public int NodeId { get; private set; }
        public ColonyKind ColonyKind { get; private set; }
        public ActionKind Kind { get { return ActionKind.PlaceColony; } }

        public PlaceColonyAction(int playerId, int nodeId, ColonyKind kind)
        {
            PlayerId = playerId;
            NodeId = nodeId;
            ColonyKind = kind;
        }
    }

    public struct PlaceShipAction : IGameAction
    {
        public int PlayerId { get; private set; }
        public int ShipId { get; private set; }
        public int NodeId { get; private set; }
        public ActionKind Kind { get { return ActionKind.PlaceShip; } }

        public PlaceShipAction(int playerId, int shipId, int nodeId)
        {
            PlayerId = playerId;
            ShipId = shipId;
            NodeId = nodeId;
        }
    }

    public struct MoveShipAction : IGameAction
    {
        public int PlayerId { get; private set; }
        public int ShipId { get; private set; }
        public int ToNodeId { get; private set; }
        public ActionKind Kind { get { return ActionKind.MoveShip; } }

        public MoveShipAction(int playerId, int shipId, int toNodeId)
        {
            PlayerId = playerId;
            ShipId = shipId;
            ToNodeId = toNodeId;
        }
    }

    public struct EndTurnAction : IGameAction
    {
        public int PlayerId { get; private set; }
        public ActionKind Kind { get { return ActionKind.EndTurn; } }

        public EndTurnAction(int playerId) { PlayerId = playerId; }
    }

    public static class RulesEngine
    {
        public static List<LegalAction> GetLegalActions(GameState s, int playerId)
        {
            var list = new List<LegalAction>();

            if (s.Phase == MatchPhase.NotStarted)
            {
                list.Add(new LegalAction(ActionKind.StartMatch, "Start Match"));
                return list;
            }

            // baseline: only current player can act
            if (playerId != s.CurrentPlayerId)
                return list;

            if (s.Phase == MatchPhase.Setup)
            {
                list.Add(new LegalAction(ActionKind.PlaceColony, "Place Colony"));
                list.Add(new LegalAction(ActionKind.PlaceShip, "Place Ship"));
                list.Add(new LegalAction(ActionKind.ConfirmSetup, "Confirm Setup"));
            }
            else if (s.Phase == MatchPhase.Turn)
            {
                list.Add(new LegalAction(ActionKind.MoveShip, "Move Ship"));
                list.Add(new LegalAction(ActionKind.EndTurn, "End Turn"));
            }

            return list;
        }

        public static ApplyResult Apply(GameState s, IGameAction action)
        {
            var state = s.CloneShallow();
            var ev = new List<IGameEvent>();

            if (action.PlayerId < 0)
                return ApplyResult.Reject("Invalid player id.", s);

            if (state.Phase == MatchPhase.NotStarted && action.Kind != ActionKind.StartMatch)
                return ApplyResult.Reject("Match has not started.", s);

            if (state.Phase != MatchPhase.NotStarted && action.Kind != ActionKind.StartMatch)
            {
                if (action.PlayerId != state.CurrentPlayerId)
                    return ApplyResult.Reject("Not your turn.", s);
            }

            switch (action.Kind)
            {
                case ActionKind.StartMatch:
                    return ApplyStartMatch(state, (StartMatchAction)action, ev);

                case ActionKind.ConfirmSetup:
                    return ApplyConfirmSetup(state, (ConfirmSetupAction)action, ev);

                case ActionKind.PlaceColony:
                    return ApplyPlaceColony(state, (PlaceColonyAction)action, ev);

                case ActionKind.PlaceShip:
                    return ApplyPlaceShip(state, (PlaceShipAction)action, ev);

                case ActionKind.MoveShip:
                    return ApplyMoveShip(state, (MoveShipAction)action, ev);

                case ActionKind.EndTurn:
                    return ApplyEndTurn(state, (EndTurnAction)action, ev);

                default:
                    return ApplyResult.Reject("Unknown action.", s);
            }
        }

        static ApplyResult ApplyStartMatch(GameState s, StartMatchAction a, List<IGameEvent> ev)
        {
            if (s.Players.Count < 2)
                return ApplyResult.Reject("Need at least 2 players to start.", s);

            s.Seed = a.Seed;
            s.Phase = MatchPhase.Setup;
            s.CurrentPlayerIndex = 0;
            s.SetupRound = 0;
            s.ActionIndex++;

            ev.Add(new PhaseChangedEvent(s.Phase));
            return ApplyResult.Accept(s, ev);
        }

        static ApplyResult ApplyConfirmSetup(GameState s, ConfirmSetupAction a, List<IGameEvent> ev)
        {
            if (s.Phase != MatchPhase.Setup)
                return ApplyResult.Reject("Not in setup.", s);

            // Placeholder: finish setup immediately.
            s.Phase = MatchPhase.Turn;
            s.ActionIndex++;

            ev.Add(new PhaseChangedEvent(s.Phase));
            return ApplyResult.Accept(s, ev);
        }

        static ApplyResult ApplyPlaceColony(GameState s, PlaceColonyAction a, List<IGameEvent> ev)
        {
            if (s.Phase != MatchPhase.Setup)
                return ApplyResult.Reject("Not in setup.", s);

            if (!s.Map.HasNode(a.NodeId))
                return ApplyResult.Reject("Invalid node.", s);

            if (s.ColoniesByNode.ContainsKey(a.NodeId))
                return ApplyResult.Reject("Node already has a colony.", s);

            s.ColoniesByNode[a.NodeId] = new ColonyState(a.PlayerId, a.ColonyKind);
            s.ActionIndex++;

            ev.Add(new ColonyPlacedEvent(a.NodeId, a.PlayerId, a.ColonyKind));
            return ApplyResult.Accept(s, ev);
        }

        static ApplyResult ApplyPlaceShip(GameState s, PlaceShipAction a, List<IGameEvent> ev)
        {
            if (s.Phase != MatchPhase.Setup)
                return ApplyResult.Reject("Not in setup.", s);

            if (!s.Map.HasNode(a.NodeId))
                return ApplyResult.Reject("Invalid node.", s);

            if (s.Ships.ContainsKey(a.ShipId))
                return ApplyResult.Reject("Ship id already exists.", s);

            s.Ships[a.ShipId] = new ShipState(a.ShipId, a.PlayerId, a.NodeId);
            s.ActionIndex++;

            ev.Add(new ShipPlacedEvent(a.ShipId, a.PlayerId, a.NodeId));
            return ApplyResult.Accept(s, ev);
        }

        static ApplyResult ApplyMoveShip(GameState s, MoveShipAction a, List<IGameEvent> ev)
        {
            if (s.Phase != MatchPhase.Turn)
                return ApplyResult.Reject("Not in turn phase.", s);

            ShipState ship;
            if (!s.Ships.TryGetValue(a.ShipId, out ship))
                return ApplyResult.Reject("Unknown ship.", s);

            if (ship.OwnerPlayerId != a.PlayerId)
                return ApplyResult.Reject("You do not own that ship.", s);

            if (!s.Map.HasNode(a.ToNodeId))
                return ApplyResult.Reject("Invalid destination node.", s);

            int from = ship.NodeId;

            if (!s.Map.AreAdjacent(from, a.ToNodeId))
                return ApplyResult.Reject("Destination not adjacent.", s);

            ship.NodeId = a.ToNodeId;
            s.Ships[a.ShipId] = ship;
            s.ActionIndex++;

            ev.Add(new ShipMovedEvent(a.ShipId, from, a.ToNodeId));
            return ApplyResult.Accept(s, ev);
        }

        static ApplyResult ApplyEndTurn(GameState s, EndTurnAction a, List<IGameEvent> ev)
        {
            if (s.Phase != MatchPhase.Turn)
                return ApplyResult.Reject("Not in turn phase.", s);

            if (s.Players.Count == 0)
                return ApplyResult.Reject("No players.", s);

            s.CurrentPlayerIndex = (s.CurrentPlayerIndex + 1) % s.Players.Count;
            s.ActionIndex++;

            ev.Add(new TurnAdvancedEvent(s.CurrentPlayerId));
            return ApplyResult.Accept(s, ev);
        }
    }
}
