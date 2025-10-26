using UnityEngine;
using UnityEngine.UI;

public class StartGameManager : MonoBehaviour
{
    public static StartGameManager Instance;
    public Button StartButton, ContinueButton, QuitButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!SaveDataManager.Instance.SaveDataExists())
        {
            ContinueButton.interactable = false;
        }

        StartButton.onClick.AddListener(StartGame);
        ContinueButton.onClick.AddListener(ContinueGame);
        QuitButton.onClick.AddListener(SceneManager.QuitGame);
    }

    public void StartGame()
    {
        SaveDataManager.Instance.DeleteSaveData();
        SaveDataManager.isNewGame = true;
        SceneManager.LoadScene(SceneManager.ProductionScene);
    }

    public void ContinueGame()
    {
        SaveDataManager.isNewGame = false;
        SceneManager.LoadScene(SceneManager.ProductionScene);
    }
}