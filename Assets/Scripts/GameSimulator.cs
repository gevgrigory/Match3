using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class GameSimulator
{
    private int playerMovesCount;
    private bool onlyMatchingMoves;

    private MatchThreeLogic matchThreeLogic;
    private Action<int, int> simulatorProgressUpdate;
    private Action<int> onScoreIncreased;
    private Action simulationComplete;

    private int curMoveIndex;

    // Start is called before the first frame update
    public void Initialize(int playerMovesCount, bool onlyMatchingMoves, MatchThreeLogic matchThreeLogic, Action<int, int> simulatorProgressUpdate, Action<int> onScoreIncreased, Action simulationComplete)
    {
        this.playerMovesCount = playerMovesCount;
        this.onlyMatchingMoves = onlyMatchingMoves;
        this.matchThreeLogic = matchThreeLogic;
        this.simulatorProgressUpdate = simulatorProgressUpdate;
        this.onScoreIncreased = onScoreIncreased;
        this.simulationComplete = simulationComplete;
    }

    public void Start()
    {
        Thread thread = new Thread(Simulate);
        thread.Start();
    }

    private void Simulate()
    {
        PlayMoves();
    }

    private void PlayMoves()
    {
        while (curMoveIndex < playerMovesCount)
        {
            if (matchThreeLogic.GetGameMove(onlyMatchingMoves, out Vector2Int tileToMove, out Vector2 direction))
            {
                if (matchThreeLogic.MoveItem(tileToMove.x, tileToMove.y, direction, out (Vector2Int, Vector2Int) movedTiles))
                {
                    List<Vector2Int> tilesToCheck = new List<Vector2Int>();
                    tilesToCheck.Add(movedTiles.Item1);
                    tilesToCheck.Add(movedTiles.Item2);
                    while (tilesToCheck.Count > 0)
                    {
                        List<Vector2Int> destroyedTiles = new List<Vector2Int>();
                        for (int i = 0; i < tilesToCheck.Count; i++)
                        {
                            destroyedTiles.AddRange(matchThreeLogic.DestroyIfNecessary(tilesToCheck[i].x, tilesToCheck[i].y));
                        }
                        tilesToCheck.Clear();
                        onScoreIncreased?.Invoke(destroyedTiles.Count);
                        matchThreeLogic.FillEmptyTiles(destroyedTiles, out List<(Vector2Int, Vector2Int)> changedTiles);
                        tilesToCheck.AddRange(changedTiles.Select(m => m.Item2));
                        matchThreeLogic.CreateNewItems(destroyedTiles, out List<Vector2Int> createdItems);
                        tilesToCheck.AddRange(createdItems);
                    }
                }
                curMoveIndex++;
                simulatorProgressUpdate?.Invoke(curMoveIndex, playerMovesCount);
            }
            else
            {
                break;
            }
        }
        Complete();
    }

    private void Complete()
    {
        if (curMoveIndex == playerMovesCount)
        {
            Debug.Log("Simulation Success");
        }
        else
        {
            Debug.Log($"No moves after {curMoveIndex} step");
        }
        simulationComplete?.Invoke();
    }
}
