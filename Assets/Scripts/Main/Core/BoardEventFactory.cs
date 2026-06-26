public static class BoardEventFactory
{
    public static IBoardEvent Create(CellType cellType)
    {
        return cellType switch
        {
            CellType.MiniGame => new MiniGameEvent(),
            _ => null
        };
    }
}
