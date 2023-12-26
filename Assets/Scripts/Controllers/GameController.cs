using UnityEngine;

public class GameController : SceneController
{
    [SerializeField]
    private GameUIController gameUIController;

    [SerializeField]
    private Board board;

    [SerializeField]
    private float minCameraSize;

    [SerializeField]
    private float egdeOffsetWidthCoef;

    [SerializeField]
    private float egdeOffsetHeightCoef;

    private MatchThreeLogic matchThreeLogic;
    private GameSimulator gameSimulator;

    private int rowsCount;
    private int columnsCount;
    private int colorsCount;
    private int playerMovesCount;
    private bool onlyMatchingMoves;

    private int score = 0;

    protected override void Start()
    {
        this.rowsCount = GameData.RowsCount;
        this.columnsCount = GameData.ColumnsCount;
        this.colorsCount = GameData.ColorsCount;
        this.playerMovesCount = GameData.PlayerMovesCount;
        this.onlyMatchingMoves = GameData.OnlyMatchingMoves;

        gameUIController.Initialize(AutoPlayValueChanged, Quit, score);

        InitLogic();

        if (GameData.Simulation)
        {
            Simulate();
        }
        else
        {
            ShowBoard();
        }
        
        base.Start();
    }

    protected void InitLogic()
    {
        int[] ids = new int[colorsCount];
        for (int i = 0; i < colorsCount; i++)
        {
            ids[i] = i;
        }

        matchThreeLogic = new MatchThreeLogic();
        matchThreeLogic.Initialize(rowsCount, columnsCount, ids);
    }

    protected void Simulate()
    {
        gameUIController.UpdateProgress(0, playerMovesCount);
        gameSimulator = new GameSimulator();
        gameSimulator.Initialize(playerMovesCount, onlyMatchingMoves, matchThreeLogic, SimulatorProgressUpdate, ScoreIncreased, SimulatorComplete);
        gameSimulator.Start();
    }

    protected void ShowBoard()
    {
        board.InitializeBoard(rowsCount, columnsCount, colorsCount, matchThreeLogic, ScoreIncreased);
        CalculateCameraSize();
        CalculateBackgroundSize();
        gameUIController.ShowGame();
    }

    protected void AutoPlayValueChanged(bool value)
    {
        board.AutoPlayValueChanged(value);
    }

    protected void SimulatorComplete()
    {
        RunInMainThread(() =>
        {
            ShowBoard();
        });
    }

    protected void ScoreIncreased(int addedScore)
    {
        RunInMainThread(()=>
        {
            score += addedScore;
            gameUIController.UpdateScore(score);
        });
    }

    protected void SimulatorProgressUpdate(int curMovesCount, int totalMovesCount)
    {
        RunInMainThread(() =>
        {
            gameUIController.UpdateProgress(curMovesCount, totalMovesCount);
        });
    }

    protected void Quit()
    {
        RestartTheGame();
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
