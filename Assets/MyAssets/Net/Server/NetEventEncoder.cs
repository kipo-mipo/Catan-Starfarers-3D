using MyAssets.GameCore;

namespace MyAssets.Net
{
    internal static class NetEventEncoder
    {
        public static CoreEventMessage ToMessage(IGameEvent e)
        {
            return e switch
            {
                MatchStartedEvent ms => new CoreEventMessage { EventType = EventTypes.MatchStarted, A = ms.Seed },

                ShipPlacedEvent sp => new CoreEventMessage
                {
                    EventType = EventTypes.ShipPlaced,
                    A = sp.Player.Value,
                    B = sp.Ship.Value,
                    C = sp.Node.Value
                },

                SetupStepChangedEvent step => new CoreEventMessage
                {
                    EventType = EventTypes.SetupStepChanged,
                    A = (int)step.Round,
                    B = step.CurrentPlayer.Value
                },

                ColonyPlacedEvent cp => new CoreEventMessage
                {
                    EventType = EventTypes.ColonyPlaced,
                    A = cp.Player.Value,
                    B = cp.Node.Value
                },

                SpaceportPlacedEvent spp => new CoreEventMessage
                {
                    EventType = EventTypes.SpaceportPlaced,
                    A = spp.Player.Value,
                    B = spp.Node.Value
                },

                SetupUpgradeGrantedEvent up => new CoreEventMessage
                {
                    EventType = EventTypes.SetupUpgradeGranted,
                    A = up.Player.Value,
                    B = (int)up.Upgrade
                },

                StartingResourcesGrantedEvent res => new CoreEventMessage
                {
                    EventType = EventTypes.StartingResourcesGranted,
                    A = res.Player.Value,
                    B = res.Count
                },

                FameMedalGrantedEvent fm => new CoreEventMessage
                {
                    EventType = EventTypes.FameMedalGranted,
                    A = fm.Player.Value
                },

                SetupCompletedEvent sc => new CoreEventMessage
                {
                    EventType = EventTypes.SetupCompleted,
                    A = sc.FirstPlayer.Value
                },

                ShipMovedEvent sm => new CoreEventMessage
                {
                    EventType = EventTypes.ShipMoved,
                    A = sm.Player.Value,
                    B = sm.Ship.Value,
                    C = sm.From.Value,
                    D = sm.To.Value
                },

                SectorRevealedEvent sr => new CoreEventMessage
                {
                    EventType = EventTypes.SectorRevealed,
                    A = sr.Slot.Value,
                    B = sr.Piece.Value,
                    C = sr.Rotation
                },

                ActionRejectedEvent rejected => new CoreEventMessage { EventType = EventTypes.Rejected, Text = rejected.Reason },
                _ => new CoreEventMessage { EventType = EventTypes.Rejected }
            };
        }
    }
}
