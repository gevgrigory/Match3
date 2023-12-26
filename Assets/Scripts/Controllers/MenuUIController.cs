using UnityEngine;
using UnityEngine.UI;

public class MenuUIController : MonoBehaviour
{
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private GameSetupDialog gameSetupDialog;

    protected void Awake()
    {
        playButton.onClick.AddListener(PlayButtonClicked);
        gameSetupDialog.Hide();
    }

    private void PlayButtonClicked()
    {
        gameSetupDialog.Show(OnGameSetup);
    }

    private void OnGameSetup(GameDataTemplate gameDataTemplate)
    {
        (SceneController.Instance as MenuController).PlayTheGame(
            gameDataTemplate.RowsCount,
            gameDataTemplate.ColumnsCount,
            gameDataTemplate.ColorsCount,
            gameDataTemplate.Simulation,
            gameDataTemplate.PlayerMovesCount,
            gameDataTemplate.OnlyMatchingMoves);
    }
}
