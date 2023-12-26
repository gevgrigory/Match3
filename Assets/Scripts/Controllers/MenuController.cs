using UnityEngine;

public class MenuController : SceneController
{
    public void PlayTheGame(int rows, int columns, int colors, bool simulation, int playerMovesCount, bool onlyMatchingMoves)
    {
        if (simulation)
        {
            GameData.InitData(rows, columns, colors, playerMovesCount, onlyMatchingMoves);
        }
        else
        {
            GameData.InitData(rows, columns, colors);
        }
        LoadNextScene();
    }
}
