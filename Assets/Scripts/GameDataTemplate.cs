using UnityEngine;

[CreateAssetMenu(fileName = "GameDataTemplate", menuName = "Data/GameDataTemplate", order = 1)]
public class GameDataTemplate : ScriptableObject
{
    public int RowsCount;
    public int ColumnsCount;
    public int ColorsCount;
    public bool Simulation;
    public int PlayerMovesCount;
    public bool OnlyMatchingMoves;
}
