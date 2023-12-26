public static class GameData
{
    public static int RowsCount { get; private set; }
    public static int ColumnsCount { get; private set; }
    public static int ColorsCount { get; private set; }
    public static bool Simulation { get; private set; }
    public static int PlayerMovesCount { get; private set; }
    public static bool OnlyMatchingMoves { get; private set; }

    public static void InitData(int rows, int columns, int colors)
    {
        RowsCount = rows;
        ColumnsCount = columns;
        ColorsCount = colors;
        Simulation = false;
    }

    public static void InitData(int rows, int columns, int colors, int playerMovesCount, bool onlyMatchingMoves)
    {
        InitData(rows, columns, colors);
        Simulation = true;
        PlayerMovesCount = playerMovesCount;
        OnlyMatchingMoves = onlyMatchingMoves;
    }
}
