public static class BoardEventFactory
{
    public static IBoardEvent Create(CellType cellType, MainSceneManager sceneManager = null, IPlayerInputReader[] players = null)
    {
        return cellType switch
        {
            CellType.MiniGame        => new MiniGameEvent(),
            CellType.AffectionSteal  => new AffectionStealEvent(sceneManager, players),
            CellType.StateChange     => new StateChangeEvent(sceneManager),
            CellType.NightmarePit    => new NightmarePitEvent(sceneManager),
            _ => null
        };
    }
}
