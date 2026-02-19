using Mirror;
using MyAssets.GameCore;

namespace MyAssets.Net.Server
{
    public sealed class ServerActionRouter
    {
        private readonly RulesEngine _rules;

        public ServerActionRouter(RulesEngine rules) => _rules = rules;

        public System.Collections.Generic.List<IGameEvent> HandleMove(MoveShipRequest msg)
        {
            var action = new MoveShipAction(
                new PlayerId(msg.Player),
                new ShipId(msg.Ship),
                new NodeId(msg.FromNode),
                new NodeId(msg.ToNode)
            );
            return _rules.Apply(action);
        }

        public System.Collections.Generic.List<IGameEvent> HandleStart(StartMatchRequest msg)
        {
            return _rules.Apply(new StartMatchAction(msg.Seed));
        }
    }
}
