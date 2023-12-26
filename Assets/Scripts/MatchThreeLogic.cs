using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchThreeLogic
{
    private const float MinTilesInRowToDestroy = 3;

    private int rowsCount;
    private int columnsCount;
    private int[] ids;

    private int[,] matrix;

    private System.Random randomGenerator;

    public void Initialize(int rowsCount, int columnsCount, int[] ids)
    {
        this.rowsCount = rowsCount;
        this.columnsCount = columnsCount;
        this.ids = ids;

        randomGenerator = new System.Random();

        matrix = new int[rowsCount, columnsCount];
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                matrix[i, j] = -1;
            }
        }
        FillMatrix();
    }

    public int GetItemForPosition(int row, int column)
    {
        return matrix[row, column];
    }

    private void FillMatrix()
    {
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                matrix[i, j] = CreateRandomIdForPosition(i, j);
            }
        }
    }

    private int CreateRandomIdForPosition(int row, int column)
    {
        List<int> availableIdsToChoose = new List<int>();
        for (int i = 0; i < ids.Length; i++)
        {
            if (!CanBeDestroyed(row, column, i))
            {
                availableIdsToChoose.Add(i);
            }
        }
        if (availableIdsToChoose.Count > 0)
        {
            return availableIdsToChoose[randomGenerator.Next(0, availableIdsToChoose.Count)];
        }

        return randomGenerator.Next(0, ids.Length);
    }

    public bool MoveItem(int row, int column, Vector2 direction, out (Vector2Int, Vector2Int) changedTiles)
    {
        changedTiles = new(Vector2Int.zero, Vector2Int.zero);

        int otherItemRow = row + (int)direction.y;
        int otherItemColumn = column + (int)direction.x;

        if (CheckTileExists(otherItemRow, otherItemColumn))
        {
            SwapTilesItems(row, column, otherItemRow, otherItemColumn);
            if (CanBeDestroyed(row, column, matrix[row, column]) || CanBeDestroyed(otherItemRow, otherItemColumn, matrix[otherItemRow, otherItemColumn]))
            {
                changedTiles = new(new Vector2Int(row, column), new Vector2Int(otherItemRow, otherItemColumn));
                return true;
            }
            SwapTilesItems(row, column, otherItemRow, otherItemColumn);
        }
        return false;
    }

    private void SwapTilesItems(int row1, int column1, int row2, int column2)
    {
        int item = matrix[row1, column1];
        matrix[row1, column1] = matrix[row2, column2];
        matrix[row2, column2] = item;
    }

    public List<Vector2Int> DestroyIfNecessary(int row, int column)
    {
        int itemId = matrix[row, column];
        List<Vector2Int> tilesToDestroy = new List<Vector2Int>();
        if (itemId != -1 && CanBeDestroyed(row, column, itemId))
        {
            GetItemsToDestroy(row, column, itemId, new List<Vector2Int>(), tilesToDestroy);
            for (int i = 0; i < tilesToDestroy.Count; i++)
            {
                matrix[tilesToDestroy[i].x, tilesToDestroy[i].y] = -1;
            }
        }
        return tilesToDestroy;
    }

    private bool CanBeDestroyed(int row, int column, int itemId)
    {
        //Don't return from the first match if we need to know what pattern exactly was destroyed.

        //For horizontal patterns detection
        int leftCount = DirectionCount(row, column - 1, Vector2Int.left, itemId);
        if (leftCount + 1 >= MinTilesInRowToDestroy) return true;

        int rightCount = DirectionCount(row, column + 1, Vector2Int.right, itemId);
        if (rightCount + 1 >= MinTilesInRowToDestroy) return true;

        int hotizontalCount = leftCount + rightCount + 1;
        if (hotizontalCount >= MinTilesInRowToDestroy) return true;

        //For vertical patterns detection
        int topCount = DirectionCount(row + 1, column, Vector2Int.up, itemId);
        if (topCount + 1 >= MinTilesInRowToDestroy) return true;

        int bottomCount = DirectionCount(row - 1, column, Vector2Int.down, itemId);
        if (bottomCount + 1 >= MinTilesInRowToDestroy) return true;

        int verticalCount = topCount + bottomCount + 1;
        if (verticalCount >= MinTilesInRowToDestroy) return true;

        //For square patterns detection
        if (leftCount > 0 && topCount > 0 && CheckTileItem(row + 1, column - 1, itemId)) return true;
        if (rightCount > 0 && topCount > 0 && CheckTileItem(row + 1, column + 1, itemId)) return true;
        if (leftCount > 0 && bottomCount > 0 && CheckTileItem(row - 1, column - 1, itemId)) return true;
        if (rightCount > 0 && bottomCount > 0 && CheckTileItem(row - 1, column + 1, itemId)) return true;

        return false;
    }

    private int DirectionCount(int row, int column, Vector2Int direction, int itemId)
    {
        if (CheckTileItem(row, column, itemId))
        {
            return DirectionCount(row + direction.y, column + direction.x, direction, itemId) + 1;
        }
        return 0;
    }

    private void GetItemsToDestroy(int row, int column, int itemId, List<Vector2Int> consideredTiles, List<Vector2Int> tilesToDestroy)
    {
        if (CheckTileItem(row, column, itemId))
        {
            Vector2Int curTile = new Vector2Int(row, column);
            if (!consideredTiles.Contains(curTile))
            {
                if (CanBeDestroyed(row, column, itemId))
                {
                    tilesToDestroy.Add(curTile);
                }
                consideredTiles.Add(curTile);
                GetItemsToDestroy(row, column - 1, itemId, consideredTiles, tilesToDestroy);
                GetItemsToDestroy(row, column + 1, itemId, consideredTiles, tilesToDestroy);
                GetItemsToDestroy(row + 1, column, itemId, consideredTiles, tilesToDestroy);
                GetItemsToDestroy(row - 1, column, itemId, consideredTiles, tilesToDestroy);
            }
        }
    }

    public void FillEmptyTiles(List<Vector2Int> destroyedTiles, out List<(Vector2Int, Vector2Int)> changedTiles)
    {
        changedTiles = new List<(Vector2Int, Vector2Int)>();
        List<Vector2Int> lowerItems = destroyedTiles.GroupBy(m => m.y).Select(g => g.OrderBy(m => m.x).First()).ToList();
        for (int i = 0; i < lowerItems.Count; i++)
        {
            int curColumn = lowerItems[i].y;
            int existingItemIndex = lowerItems[i].x + 1;
            for (int j = lowerItems[i].x; j < rowsCount; j++)
            {
                while (existingItemIndex < rowsCount && matrix[existingItemIndex, curColumn] == -1)
                {
                    existingItemIndex++;
                }
                if (existingItemIndex < rowsCount)
                {
                    matrix[j, curColumn] = matrix[existingItemIndex, curColumn];
                    matrix[existingItemIndex, curColumn] = -1;
                    changedTiles.Add(new (new Vector2Int(existingItemIndex, curColumn), new Vector2Int(j, curColumn)));
                    existingItemIndex++;
                }
                else
                {
                    break;
                }
            }
        }
    }

    public void CreateNewItems(List<Vector2Int> destroyedTiles, out List<Vector2Int> createdItems)
    {
        createdItems = new List<Vector2Int>();
        List<Vector2Int> lowerItems = destroyedTiles.GroupBy(m => m.y).Select(g => g.OrderBy(m => m.x).First()).ToList();
        for (int i = 0; i < lowerItems.Count; i++)
        {
            int curColumn = lowerItems[i].y;
            for (int j = lowerItems[i].x; j < rowsCount; j++)
            {
                if (matrix[j, curColumn] == -1)
                {
                    matrix[j, curColumn] = CreateRandomIdForPosition(j, curColumn);
                    createdItems.Add(new Vector2Int(j, curColumn));
                }
            }
        }
    }

    public List<Vector2> GetAvailableMoveDirections(int row, int column)
    {
        List<Vector2> directions = new List<Vector2>();
        if (row > 0) directions.Add(Vector2.down);
        if (row < rowsCount - 1) directions.Add(Vector2.up);
        if (column > 0) directions.Add(Vector2.left);
        if (column < columnsCount - 1) directions.Add(Vector2.right);
        return directions;
    }

    public bool GetGameMove(bool onlyMatchingMoves, out Vector2Int tileToMove, out Vector2 direction)
    {
        if (onlyMatchingMoves)
        {
            return GetMatchingMove(out tileToMove, out direction);
        }

        return GetRandomMove(out tileToMove, out direction);
    }

    private bool GetMatchingMove(out Vector2Int tileToMove, out Vector2 direction)
    {
        tileToMove = Vector2Int.zero;
        direction = Vector2.zero;

        List<(Vector2Int, Vector2)> possibleMoves = new List<(Vector2Int, Vector2)>();
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                List<Vector2> directions = GetAvailableMoveDirections(i, j);
                for (int k = 0; k < directions.Count; k++)
                {
                    if (MoveItem(i, j, directions[k], out (Vector2Int, Vector2Int) changedTiles))
                    {
                        Vector2Int otherTile = changedTiles.Item2;
                        SwapTilesItems(i, j, otherTile.x, otherTile.y);
                        possibleMoves.Add(new(new Vector2Int(i, j), directions[k]));
                    }
                }
            }
        }
        if(possibleMoves.Count > 0)
        {
            int random = randomGenerator.Next(0, possibleMoves.Count);
            tileToMove = possibleMoves[random].Item1;
            direction = possibleMoves[random].Item2;
            return true;
        }
        return false;
    }

    private bool GetRandomMove(out Vector2Int tileToMove, out Vector2 direction)
    {
        tileToMove = Vector2Int.zero;
        direction = Vector2.zero;

        if (rowsCount > 1 && columnsCount > 1)
        {
            int verticalEdge = rowsCount - 2;
            int horizontalEdge = columnsCount - 2;
            int cornerPossibility = 2 * 4; //4 corners, 2 moves per each
            int edgesPossibility = 3 * (verticalEdge * 2 + horizontalEdge * 2);//all 4 edges - corners duplication
            int otherPossibility = 4 * verticalEdge * horizontalEdge;
            int allPossibilities = cornerPossibility + edgesPossibility + otherPossibility;
            int random = randomGenerator.Next(0, allPossibilities);
            if (random < cornerPossibility)
            {
                random /= 2;
                tileToMove = new Vector2Int(random / 2 * (rowsCount - 1), random % 2 * (columnsCount - 1));
            }
            else
            {
                random -= cornerPossibility;
                if (random < edgesPossibility)
                {
                    random /= 3;
                    if (random < verticalEdge * 2)
                    {
                        tileToMove = new Vector2Int(random % verticalEdge + 1, random / verticalEdge * (columnsCount - 1));
                    }
                    else
                    {
                        random -= verticalEdge * 2;
                        tileToMove = new Vector2Int(random / horizontalEdge * (rowsCount - 1), random % horizontalEdge + 1);
                    }
                }
                else
                {
                    random -= edgesPossibility;

                    random /= 4;
                    tileToMove = new Vector2Int(random / horizontalEdge + 1, random % horizontalEdge + 1);
                }
            }

            List<Vector2> availableMoveDirections = GetAvailableMoveDirections(tileToMove.x, tileToMove.y);
            direction = availableMoveDirections[randomGenerator.Next(0, availableMoveDirections.Count)];
            return true;
        }
        return false;
    }

    private bool CheckTileExists(int row, int column)
    {
        return row >= 0 && column >= 0 && row < rowsCount && column < columnsCount;
    }

    private bool CheckTileItem(int row, int column, int itemId)
    {
        return CheckTileExists(row, column) && matrix[row, column] == itemId;
    }
}
