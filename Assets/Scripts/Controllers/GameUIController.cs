using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField]
    private Button quitButton;

    [SerializeField]
    private TMP_Text score;

    [SerializeField]
    private GameObject autoPlayMarkerContainer;

    [SerializeField]
    private GameObject autoPlayContainer;

    [SerializeField]
    private Toggle autoPlay;

    [SerializeField]
    private GameObject loadingContainer;

    [SerializeField]
    private Slider loadingSlider;

    [SerializeField]
    private TMP_Text playerMovesCountProgress;

    private Action<bool> onAutoPlayValueChange;
    private Action onQuit;

    protected void Awake()
    {
        quitButton.onClick.AddListener(QuitButtonClicked);
        autoPlay.onValueChanged.AddListener(AutoPlayValueChanged);
        loadingContainer.SetActive(false);
        autoPlayMarkerContainer.SetActive(false);
    }

    public void Initialize(Action<bool> onAutoPlayValueChange, Action onQuit, int initialScore)
    {
        this.onAutoPlayValueChange = onAutoPlayValueChange;
        this.onQuit = onQuit;

        UpdateScore(initialScore);
        autoPlayContainer.SetActive(false);
    }

    public void UpdateScore(int score)
    {
        this.score.text = score.ToString();
    }

    public void UpdateProgress(int curMovesCount, int totalMovesCount)
    {
        loadingContainer.SetActive(true);
        loadingSlider.value = curMovesCount / (float)totalMovesCount;
        playerMovesCountProgress.text = $"({curMovesCount} / {totalMovesCount})";
    }

    public void ShowGame()
    {
        autoPlayContainer.SetActive(true);
        loadingContainer.SetActive(false);
    }

    private void AutoPlayValueChanged(bool value)
    {
        autoPlayMarkerContainer.SetActive(value);
        onAutoPlayValueChange?.Invoke(value);
    }

    private void QuitButtonClicked()
    {
        onQuit?.Invoke();
    }
}
