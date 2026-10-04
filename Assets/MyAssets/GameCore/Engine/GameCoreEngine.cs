using System.Collections.Generic;

namespace MyAssets.GameCore
{
    /// <summary>
    /// Thin wrapper around core rules. Keeps older callers compiling.
    /// RulesEngine is the single source of truth; do not duplicate rule logic here.
    /// </summary>
    public sealed class GameCoreEngine
    {
        private readonly RulesEngine _rules;
        public GameState State { get; }

        public GameCoreEngine(GameState state)
        {
            State = state;
            _rules = new RulesEngine(state);
        }

        public List<IGameEvent> Apply(IGameAction action) => _rules.Apply(action);
    }
}
