public static class GameData
{
    public static int RowsCount { get; private set; }
    public static int ColumnsCount { get; private set; }
    public static int ColorsCount { get; private set; }

    public static void InitData(int rows, int columns, int colors)
    {
        RowsCount = rows;
        ColumnsCount = columns;
        ColorsCount = colors;
    }
}
