using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField]
    private Button quitButton;

    protected void Awake()
    {
        quitButton.onClick.AddListener(QuitButtonClicked);
    }

    private void QuitButtonClicked()
    {
        GameController.Instance.RestartTheGame();
    }
}
