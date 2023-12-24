using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSetupDialog : MonoBehaviour
{
    [SerializeField]
    private GameDataTemplate defaultGameData;

    [SerializeField]
    private TMP_InputField rowsCountInput;

    [SerializeField]
    private GameObject rowsCountValidation;

    [SerializeField]
    private TMP_InputField columnsCountInput;

    [SerializeField]
    private GameObject columnsCountValidation;

    [SerializeField]
    private TMP_InputField colorsCountInput;

    [SerializeField]
    private GameObject colorsCountValidation;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private Button startButton;

    private Action<int, int, int> onGameSetup;

    protected void Awake()
    {
        rowsCountValidation.SetActive(false);
        columnsCountValidation.SetActive(false);
        colorsCountValidation.SetActive(false);

        closeButton.onClick.AddListener(Hide);
        startButton.onClick.AddListener(OnStartClicked);

        rowsCountInput.onValueChanged.AddListener(ValidateRows);
        columnsCountInput.onValueChanged.AddListener(ValidateColumns);
        colorsCountInput.onValueChanged.AddListener(ValidateColors);

        rowsCountInput.text = defaultGameData.RowsCount.ToString();
        columnsCountInput.text = defaultGameData.ColumnsCount.ToString();
        colorsCountInput.text = defaultGameData.ColorsCount.ToString();
    }

    public void Show(Action<int, int, int> onGameSetup)
    {
        this.onGameSetup = onGameSetup;

        gameObject.SetActive(true);

        SetStartButtonState();
    }

    private void ValidateRows(string value)
    {
        rowsCountValidation.SetActive(!ValidateInput(rowsCountInput));
        SetStartButtonState();
    }

    private void ValidateColumns(string value)
    {
        columnsCountValidation.SetActive(!ValidateInput(columnsCountInput));
        SetStartButtonState();
    }

    private void ValidateColors(string value)
    {
        colorsCountValidation.SetActive(!ValidateInput(colorsCountInput));
        SetStartButtonState();
    }

    private bool ValidateInput(TMP_InputField input)
    {
        return !string.IsNullOrEmpty(input.text) && int.TryParse(input.text, out int count) && count > 0;
    }

    private void SetStartButtonState()
    {
        startButton.interactable = ValidateInput(rowsCountInput) && ValidateInput(columnsCountInput) && ValidateInput(colorsCountInput);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnStartClicked()
    {
        Hide();

        int rows = int.Parse(rowsCountInput.text);
        int columns = int.Parse(columnsCountInput.text);
        int colors = int.Parse(colorsCountInput.text);

        onGameSetup?.Invoke(rows, columns, colors);
    }
}
