using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    private const float TileSize = 1f;
    private const float MinTilesInRowToDestroy = 3;
    private const float DelayAfterDestroy = 0.5f;
    private const float ItemMoveSpeed = 15f;
    private const float AutoPlayMoveDelay = 1f;

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
    private SpriteMask itemsMask;

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
    private bool autoPlay;
    private bool animating;

    private List<Tile> animatingTiles = new List<Tile>();
    private List<Tile> curAnimatingTiles = new List<Tile>();

    private MatchThreeLogic matchThreeLogic;
    private Action<int> onScoreIncreased;

    #region Initialization

    public void InitializeBoard(int rowsCount, int columnsCount, int colorsCount, MatchThreeLogic matchThreeLogic, Action<int> onScoreIncreased)
    {
        this.rowsCount = rowsCount;
        this.columnsCount = columnsCount;
        this.colorsCount = colorsCount;
        this.matchThreeLogic = matchThreeLogic;
        this.onScoreIncreased = onScoreIncreased;

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
                AddItemToTile(CreateItem(i, j), tileMatrix[i, j]);
            }
        }
    }

    private void SetItemsMaskSize()
    {
        Vector2 boardSize = GetBoardSize();
        Vector3 maskSize = itemsMask.bounds.size;

        Vector3 maskScale = new Vector3(boardSize.x / maskSize.x, boardSize.y / maskSize.y, 1);
        itemsMask.transform.localScale = maskScale;
        itemsParent.transform.localScale = new Vector3(1 / maskScale.x, 1 / maskScale.y, maskScale.z);
    }

    #endregion Initialization

    #region ItemCreation

    private Item CreateItem(int row, int column)
    {
        Vector3 position = GetPositionForNewItem(column);
        Item item = Instantiate(itemPrefab, position, Quaternion.identity, itemsParent).GetComponent<Item>();

        int newId = matchThreeLogic.GetItemForPosition(row, column);
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
        SceneController.Instance.BlockInteraction();
        curAnimatingTiles.Add(tile);
        animatingTiles.Add(tile);
        animating = true;
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

    public void AutoPlayValueChanged(bool value)
    {
        if(autoPlay != value)
        {
            autoPlay = value;

            if (autoPlay)
            {
                SceneController.Instance.BlockInteraction();
                if (!animating)
                {
                    Invoke(nameof(AutoPlay), AutoPlayMoveDelay);
                }
            }
            else
            {
                CancelInvoke(nameof(AutoPlay));
                if (!animating)
                {
                    SceneController.Instance.AllowInteraction();
                }
            }
        }
    }

    private void AutoPlay()
    {
        if(matchThreeLogic.GetGameMove(true, out Vector2Int tileToMove, out Vector2 direction))
        {
            TileMoved(tileMatrix[tileToMove.x, tileToMove.y], direction);
        }
        else
        {
            Debug.Log("No more moves");
        }
    }

    public void AnimationsComplete()
    {
        List<Tile> tilesToCheck = new List<Tile>(animatingTiles);
        animatingTiles.Clear();
        List<Tile> destroyedTiles = new List<Tile>();
        foreach (Tile tile in tilesToCheck)
        {
            destroyedTiles.AddRange(DestroyIfNecessary(tile));
        }
        if(destroyedTiles.Count > 0)
        {
            onScoreIncreased?.Invoke(destroyedTiles.Count);
            StartCoroutine(RunAfterDelay(DelayAfterDestroy, () =>
            {
                animating = false;
                if (!autoPlay)
                {
                    SceneController.Instance.AllowInteraction();
                }
                CreateNewItems(destroyedTiles);
            }));
        }
        else
        {
            animating = false;
            if (autoPlay)
            {
                Invoke(nameof(AutoPlay), AutoPlayMoveDelay);
            }
            else
            {
                SceneController.Instance.AllowInteraction();
            }
        }
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
        if (matchThreeLogic.MoveItem(tile.Row, tile.Column, direction, out (Vector2Int, Vector2Int) changedTiles))
        {
            Tile tile1 = tileMatrix[changedTiles.Item1.x, changedTiles.Item1.y];
            Tile tile2 = tileMatrix[changedTiles.Item2.x, changedTiles.Item2.y];
            Item item = tile1.Item;
            AddItemToTile(tile2.Item, tile1);
            AddItemToTile(item, tile2);
        }
        else
        {
            Debug.Log("These items can't swap");
        }
    }

    #endregion TileInteraction

    #region ItemsMatching

    private List<Tile> DestroyIfNecessary(Tile tile)
    {
        List<Tile> tilesToDestroy = new List<Tile>();
        List<Vector2Int> destroyedTiles = matchThreeLogic.DestroyIfNecessary(tile.Row, tile.Column);

        for (int i = 0; i < destroyedTiles.Count; i++)
        {
            Tile curTile = tileMatrix[destroyedTiles[i].x, destroyedTiles[i].y];
            curTile.DestroyItem();
            tilesToDestroy.Add(curTile);
        }
        return tilesToDestroy;
    }

    private void CreateNewItems(List<Tile> destroyedTiles)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        for (int i = 0; i < destroyedTiles.Count; i++)
        {
            tiles.Add(new Vector2Int(destroyedTiles[i].Row, destroyedTiles[i].Column));
        }
        matchThreeLogic.FillEmptyTiles(tiles, out List<(Vector2Int, Vector2Int)> changedTiles);
        foreach (var change in changedTiles)
        {
            Tile from = tileMatrix[change.Item1.x, change.Item1.y];
            Tile to = tileMatrix[change.Item2.x, change.Item2.y];
            AddItemToTile(from.Item, to);
            from.RemoveItem();
        }
        matchThreeLogic.CreateNewItems(tiles, out List<Vector2Int> createdItems);
        foreach (var position in createdItems)
        {
            AddItemToTile(CreateItem(position.x, position.y), tileMatrix[position.x, position.y]);
        }
    }

    #endregion ItemsMatching

    #region Util

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
