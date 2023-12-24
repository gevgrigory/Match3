using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : SceneController
{
    [SerializeField]
    private Board board;

    [SerializeField]
    private float minCameraSize;

    [SerializeField]
    private float egdeOffsetWidthCoef;

    [SerializeField]
    private float egdeOffsetHeightCoef;

    protected override void Start()
    {
        board.InitializeBoard(GameData.RowsCount, GameData.ColumnsCount, GameData.ColorsCount);
        CalculateCameraSize();
        base.Start();
    }

    protected void CalculateCameraSize()
    {
        Vector2 halfSize = board.GetBoardSize() * 0.5f;
        Vector2 cameraHalfSize = GetHalfCameraSize();

        float coefX = halfSize.x / cameraHalfSize.x / egdeOffsetWidthCoef;
        float coefY = halfSize.y / cameraHalfSize.y / egdeOffsetHeightCoef;
        gameCamera.orthographicSize = Mathf.Max(minCameraSize, cameraHalfSize.y * Mathf.Max(coefX, coefY));
    }
}
