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

    private void OnGameSetup(int rows, int columns, int colors)
    {
        (MenuController.Instance as MenuController).PlayTheGame(rows, columns, colors);
    }
}
