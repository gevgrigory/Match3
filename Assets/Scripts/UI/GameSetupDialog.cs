using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSetupDialog : MonoBehaviour
{
    private const int MinCount = 2;

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
    private Toggle simulationToggle;

    [SerializeField]
    private Button closeButton;

    [SerializeField]
    private Button startButton;



    [Header("Simulation")]
    [SerializeField]
    private GameObject simulationContainer;

    [SerializeField]
    private TMP_InputField playerMovesCountInput;

    [SerializeField]
    private GameObject playerMovesCountValidation;

    [SerializeField]
    private Toggle onlyMatchingToggle;

    [SerializeField]
    private Button simulationCloseButton;

    [SerializeField]
    private Button simulationStartButton;



    private Action<GameDataTemplate> onGameSetup;

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
        simulationToggle.isOn = defaultGameData.Simulation;



        HideSimulationSetup();
        simulationCloseButton.onClick.AddListener(HideSimulationSetup);
        simulationStartButton.onClick.AddListener(OnSimulationStartClicked);
        playerMovesCountInput.onValueChanged.AddListener(ValidatePlayerMoves);
        playerMovesCountInput.text = defaultGameData.PlayerMovesCount.ToString();
        onlyMatchingToggle.isOn = defaultGameData.OnlyMatchingMoves;
    }

    public void Show(Action<GameDataTemplate> onGameSetup)
    {
        this.onGameSetup = onGameSetup;

        gameObject.SetActive(true);

        HideSimulationSetup();
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
        return !string.IsNullOrEmpty(input.text) && int.TryParse(input.text, out int count) && count >= MinCount;
    }

    private void SetStartButtonState()
    {
        startButton.interactable = ValidateInput(rowsCountInput) && ValidateInput(columnsCountInput) && ValidateInput(colorsCountInput);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void ShowSimulationSetup()
    {
        simulationContainer.SetActive(true);

        SetSimulationStartButtonState();
    }

    private void ValidatePlayerMoves(string value)
    {
        playerMovesCountValidation.SetActive(!ValidateInput(playerMovesCountInput));
        SetSimulationStartButtonState();
    }

    private void SetSimulationStartButtonState()
    {
        simulationStartButton.interactable = ValidateInput(playerMovesCountInput);
    }

    private void HideSimulationSetup()
    {
        simulationContainer.SetActive(false);
    }

    private void OnStartClicked()
    {
        if (simulationToggle.isOn)
        {
            ShowSimulationSetup();
        }
        else
        {
            Hide();
            onGameSetup?.Invoke(GetMainData());
        }
    }

    private void OnSimulationStartClicked()
    {
        Hide();

        GameDataTemplate data = GetMainData();

        data.Simulation = true;
        data.PlayerMovesCount = int.Parse(playerMovesCountInput.text);
        data.OnlyMatchingMoves = onlyMatchingToggle.isOn;

        onGameSetup?.Invoke(data);
    }

    private GameDataTemplate GetMainData()
    {
        GameDataTemplate data = ScriptableObject.CreateInstance<GameDataTemplate>();

        data.RowsCount = int.Parse(rowsCountInput.text);
        data.ColumnsCount = int.Parse(columnsCountInput.text);
        data.ColorsCount = int.Parse(colorsCountInput.text);

        return data;
    }
}
