using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private const float TileSize = 1f;
    private const float MinTilesInRowToDestroy = 3;
    private const float DelayAfterDestroy = 0.5f;
    private const float ItemMoveSpeed = 15f;

    [SerializeField]
    [Range(0,1)]
    private float baseColorSaturation;

    [SerializeField]
    [Range(0, 1)]
    private float baseColorValue;

    [SerializeField]
    private Sprite[] items;

    [SerializeField]
    private Transform tilesParent;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private Transform itemsMask;

    [SerializeField]
    private Transform itemsParent;

    [SerializeField]
    private GameObject itemPrefab;

    private int rowsCount;
    private int columnsCount;
    private int colorsCount;
    private Tile[,] tileMatrix;
    private Color[] itemColors;
    private Tile selectedTile;

    private bool usingSprites;

    private List<Tile> animatingTiles = new List<Tile>();
    private List<Tile> curAnimatingTiles = new List<Tile>();

    #region Initialization

    public void InitializeBoard(int rowsCount, int columnsCount, int colorsCount)
    {
        this.rowsCount = rowsCount;
        this.columnsCount = columnsCount;
        this.colorsCount = colorsCount;

        usingSprites = colorsCount <= items.Length;
        if (!usingSprites)
        {
            InitItemColors();
        }

        CreateTiles();
        FillItems();

        SetItemsMaskSize();
    }

    private void InitItemColors()
    {
        float hueStep = 1f / colorsCount;
        itemColors = new Color[colorsCount];
        for (int i = 0; i < colorsCount; i++)
        {
            float hue = i * hueStep;
            itemColors[i] = Color.HSVToRGB(hue, baseColorSaturation, baseColorValue);
        }
    }

    private void CreateTiles()
    {
        tileMatrix = new Tile[rowsCount, columnsCount];
        float startPointX = -(columnsCount - 1) * TileSize / 2;
        float startPointY = -(rowsCount - 1) * TileSize / 2;
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                Vector3 position = new Vector3(startPointX + j * TileSize, startPointY + i * TileSize);
                Tile tile = Instantiate(tilePrefab, position, Quaternion.identity, tilesParent).GetComponent<Tile>();
                tile.Init(i, j, TileClicked, TileMoved);
                tileMatrix[i, j] = tile;
            }
        }
    }

    private void FillItems()
    {
        for (int i = 0; i < rowsCount; i++)
        {
            for (int j = 0; j < columnsCount; j++)
            {
                AddItemToTile(CreateRandomItem(i, j), tileMatrix[i, j]);
            }
        }
    }

    private void SetItemsMaskSize()
    {
        Vector2 boardSize = GetBoardSize();
        itemsMask.transform.localScale = new Vector3(boardSize.x, boardSize.y, 1);
        itemsParent.transform.localScale = new Vector3(1 / boardSize.x, 1 / boardSize.y, 1);
    }

    #endregion Initialization

    #region ItemCreation

    private Item CreateRandomItem(int row, int column)
    {
        Vector3 position = GetPositionForNewItem(column);
        Item item = Instantiate(itemPrefab, position, Quaternion.identity, itemsParent).GetComponent<Item>();

        int newId = GetNewIdForPosition(row, column);
        if (usingSprites)
        {
            item.Init(newId, items[newId]);
        }
        else
        {
            item.Init(newId, itemColors[newId]);
        }
        return item;
    }

    private int GetNewIdForPosition(int row, int column)
    {
        List<int> availableIdsToChoose = new List<int>();
        for (int i = 0; i < colorsCount; i++)
        {
            if (!CanBeDestroyed(row, column, i))
            {
                availableIdsToChoose.Add(i);
            }
        }
        if (availableIdsToChoose.Count > 0)
        {
            return availableIdsToChoose[UnityEngine.Random.Range(0, availableIdsToChoose.Count)];
        }

        return UnityEngine.Random.Range(0, colorsCount);
    }

    private Vector3 GetPositionForNewItem(int column)
    {
        Vector3 targetPosition = tileMatrix[rowsCount - 1, column].transform.position;
        for (int i = rowsCount - 1; i >= 0; i--)
        {
            Tile curTile = tileMatrix[i, column];
            if (curTile.Item != null && curTile.Item.transform.position.y > targetPosition.y)
            {
                targetPosition = curTile.Item.transform.position;
                break;
            }
        }
        return targetPosition + TileSize * Vector3.up;
    }

    private void AddItemToTile(Item item, Tile tile)
    {
        tile.SetItem(item);
        AnimateTile(tile);
    }

    #endregion ItemCreation

    #region Animation

    public void AnimateTile(Tile tile)
    {
        GameController.Instance.BlockInteraction();
        curAnimatingTiles.Add(tile);
        animatingTiles.Add(tile);
    }

    protected void Update()
    {
        if(animatingTiles.Count > 0)
        {
            for (int i = curAnimatingTiles.Count - 1; i >= 0; i--)
            {
                Item item = curAnimatingTiles[i].Item;
                Vector3 curTilePosition = curAnimatingTiles[i].transform.position;
                Vector3 itemResultPosition = item.transform.position;
                itemResultPosition = Vector3.MoveTowards(itemResultPosition, curTilePosition, ItemMoveSpeed * Time.deltaTime);
                if (itemResultPosition == curTilePosition)
                {
                    curAnimatingTiles.RemoveAt(i);
                }
                item.transform.position = itemResultPosition;
            }
            
            if (curAnimatingTiles.Count == 0)
            {
                AnimationsComplete();
            }
        }
    }

    public void AnimationsComplete()
    {
        List<Tile> tilesToCheck = new List<Tile>(animatingTiles);
        animatingTiles.Clear();
        List<Tile> destroyedTiles = new List<Tile>();
        foreach (Tile tile in tilesToCheck)
        {
            if (tile.Item != null)
            {
                destroyedTiles.AddRange(DestroyIfNecessary(tile, tile.Item.Id));
            }
        }
        float delay = destroyedTiles.Count > 0 ? DelayAfterDestroy : 0;
        StartCoroutine(RunAfterDelay(delay, () =>
        {
            CreateNewItems(destroyedTiles);
            GameController.Instance.AllowInteraction();
        }));
    }

    #endregion Animation

    #region TileInteraction

    private void TileClicked(Tile tile)
    {
        if (selectedTile == null || selectedTile == tile)
        {
            SetSelectedTile(tile);
        }
        else
        {
            Vector2Int diff = new Vector2Int(tile.Column - selectedTile.Column, tile.Row - selectedTile.Row);
            if (Mathf.Abs(diff.x + diff.y) == 1)
            {
                TileMoved(selectedTile, diff);
            }
            else
            {
                SetSelectedTile(tile);
            }
        }
    }

    private void SetSelectedTile(Tile tile)
    {
        RemoveSelectedTile();
        if (tile != null)
        {
            selectedTile = tile;
            selectedTile.Select();
        }
    }

    private void RemoveSelectedTile()
    {
        if (selectedTile != null)
        {
            selectedTile.Deselect();
            selectedTile = null;
        }
    }

    public void TileMoved(Tile tile, Vector2 direction)
    {
        RemoveSelectedTile();
        int otherTileRow = tile.Row + (int)direction.y;
        int otherTileColumn = tile.Column + (int)direction.x;

        if (CheckTileExists(otherTileRow, otherTileColumn))
        {
            Tile otherTile = tileMatrix[otherTileRow, otherTileColumn];
            SwapTilesItems(tile, otherTile);
            if (CanBeDestroyed(tile.Row, tile.Column, tile.Item.Id) || CanBeDestroyed(otherTile.Row, otherTile.Column, otherTile.Item.Id))
            {
                AddItemToTile(tile.Item, tile);
                AddItemToTile(otherTile.Item, otherTile);
            }
            else
            {
                SwapTilesItems(tile, otherTile);
                Debug.Log("These items can't swap");
            }
        }
    }

    private void SwapTilesItems(Tile tile1, Tile tile2)
    {
        Item item = tile1.Item;
        tile1.SetItem(tile2.Item);
        tile2.SetItem(item);
    }

    #endregion TileInteraction

    #region ItemsMatching

    private List<Tile> DestroyIfNecessary(Tile tile, int itemId)
    {
        List<Tile> tilesToDestroy = new List<Tile>();
        if (CanBeDestroyed(tile.Row, tile.Column, itemId))
        {
            GetItemsToDestroy(tile.Row, tile.Column, itemId, new List<Tile>(), tilesToDestroy);
            for (int i = 0; i < tilesToDestroy.Count; i++)
            {
                tilesToDestroy[i].DestroyItem();
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

    private void GetItemsToDestroy(int row, int column, int itemId, List<Tile> consideredTiles, List<Tile> tilesToDestroy)
    {
        if(CheckTileItem(row, column, itemId))
        {
            Tile curTile = tileMatrix[row, column];
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

    private void CreateNewItems(List<Tile> destroyedTiles)
    {
        List<Tile> lowerItems = destroyedTiles.GroupBy(m => m.Column).Select(g => g.OrderBy(m => m.Row).First()).ToList();
        for (int i = 0; i < lowerItems.Count; i++)
        {
            int curColumn = lowerItems[i].Column;
            int existingItemIndex = lowerItems[i].Row + 1;
            for (int j = lowerItems[i].Row; j < rowsCount; j++)
            {
                while (existingItemIndex < rowsCount && tileMatrix[existingItemIndex, curColumn].Item == null)
                {
                    existingItemIndex++;
                }
                if (existingItemIndex < rowsCount)
                {
                    AddItemToTile(tileMatrix[existingItemIndex, curColumn].Item, tileMatrix[j, curColumn]);
                    tileMatrix[existingItemIndex, curColumn].RemoveItem();
                    existingItemIndex++;
                }
                else
                {
                    AddItemToTile(CreateRandomItem(j, curColumn), tileMatrix[j, curColumn]);
                }
            }
        }
    }

    #endregion ItemsMatching

    #region Util

    private bool CheckTileExists(int row, int column)
    {
        return row >= 0 && column >= 0 && row < rowsCount && column < columnsCount;
    }

    private bool CheckTileItem(int row, int column, int itemId)
    {
        return CheckTileExists(row, column) && tileMatrix[row, column].Item != null && tileMatrix[row, column].Item.Id == itemId;
    }

    public Vector2 GetBoardSize()
    {
        return new Vector2(columnsCount * TileSize, rowsCount * TileSize);
    }

    private IEnumerator RunAfterDelay(float time, Action action)
    {
        if (time > 0)
        {
            yield return new WaitForSeconds(time);
        }
        action?.Invoke();
    }

    #endregion Util
}
