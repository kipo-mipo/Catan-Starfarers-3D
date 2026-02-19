namespace MyAssets.GameCore
{
    public enum MatchPhase { Setup, Turn, GameOver }

    public enum SectorType
    {
        Empty,
        Planet,
        TradeStation
    }

    public enum ResourceType
    {
        None,
        // Add your actual Starfarers resources here
        Ore,
        Carbon,
        Food,
        Goods,
        Gas,
    }

    public enum UpgradeType
    {
        FreightPod,
        Cannon,
        Booster,
    }
}
