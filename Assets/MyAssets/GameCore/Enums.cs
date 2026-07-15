namespace MyAssets.GameCore
{
    public enum MatchPhase { Setup, Turn, GameOver }

    public enum SetupRound
    {
        FirstColony = 1,
        SecondColony = 2,
        ThirdColony = 3,
        SpaceportShipUpgrade = 4
    }

    public enum SetupShipType
    {
        ColonyShip = 0,
        TradeShip = 1
    }

    public enum SectorType
    {
        Empty,
        Planet,
        TradeStation
    }

    public enum ResourceType
    {
        None,
        Ore,
        Carbon,
        Food,
        Goods,
        Fuel,
    }

    public enum UpgradeType
    {
        FreightPod,
        Cannon,
        Booster,
    }
}
