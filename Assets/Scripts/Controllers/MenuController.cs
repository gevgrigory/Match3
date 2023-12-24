using UnityEngine;

public class MenuController : SceneController
{
    public void PlayTheGame(int rows, int columns, int colors)
    {
        GameData.InitData(rows, columns, colors);
        LoadNextScene();
    }
}
