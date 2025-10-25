using UnityEngine;
using UnityEngine.UI;

public class StartGameManager : MonoBehaviour
{
    public static StartGameManager Instance;
    public Button StartButton, ContinueButton;

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
    }

    public void StartGame()
    {
        SaveDataManager.Instance.DeleteSaveData();
        SceneManager.LoadScene(SceneManager.ProductionScene);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(SceneManager.ProductionScene);
    }
}