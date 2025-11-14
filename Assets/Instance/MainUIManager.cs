using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    public Canvas mainUICanvas;
    public GameObject foldNodePrefab;
    public GameObject subNodePrefab;
    [Header("按钮面板区")]
    [Header("任务")]
    public Button TaskBtn;
    public GameObject TaskPanel;
    public GameObject MainTaskObj;
    public GameObject SubTaskObj;
    [Header("合成表")]
    public Button CraftTableBtn;
    public GameObject CraftTablePanel;
    public UI_CraftTablePanel craftTableList;
    [Header("人口")]
    public Button PopulationBtn;
    public GameObject PopulationPanel;
    public UI_PopulationPanel populationPanel;
    [Header("设置面板")]
    public Button SettingsBtn;
    public GameObject SettingsPanel;
    public Button CloseSettingsBtn;
    [Header("文字信息")]
    public Text foodText;
    [Header("退出按钮")]
    public Button BackToStartBtn;
    public Button ExitButton;

    void Start()
    {
        TaskBtn.onClick.AddListener(() => ChangePanelState(TaskPanel));
        CraftTableBtn.onClick.AddListener(() => ChangePanelState(CraftTablePanel));
        PopulationBtn.onClick.AddListener(() => ChangePanelState(PopulationPanel));
        SettingsBtn.onClick.AddListener(() =>
        {
            SettingsPanel.SetActive(true);
            TimeManager.Instance.ChangePauseGameState();
        });
        CloseSettingsBtn.onClick.AddListener(() =>
        {
            SettingsPanel.SetActive(false);
            TimeManager.Instance.ChangePauseGameState();
        });
        ExitButton.onClick.AddListener(SceneManager.QuitToDesktop);
        BackToStartBtn.onClick.AddListener(SceneManager.QuitToStartScene);
    }

    public void InitMainUI()
    {
        foreach (var recipe in CardManager.Instance.GetUnlockedCraftableRecipes())
        {
            craftTableList.AddRecipeNode(recipe);
        }

        UpdateFoodText();
        populationPanel.UpdatePopulationNode();
        CardManager.Instance.onCardCreated += UpdatePanel;
        CardManager.Instance.onCardDeleted += UpdatePanel;
    }

    public void OnDestroy()
    {
        CardManager.Instance.onCardCreated -= UpdatePanel;
        CardManager.Instance.onCardDeleted -= UpdatePanel;
    }

    public void ChangePanelState(GameObject panel)
    {
        bool isActive = panel.activeSelf;
        TaskPanel.SetActive(false);
        CraftTablePanel.SetActive(false);
        PopulationPanel.SetActive(false);
        panel.SetActive(!isActive);
    }

    private void UpdatePanel(Card changedCard = null)
    {
        if (changedCard == null)
        {
            // Update all panels
            UpdateFoodText();
            populationPanel.UpdatePopulationNode();
        }
        else
        {
            // Update specific panel based on changed card
            if (changedCard.cardDescription.cardType == Category.CardType.Resources)
            {
                UpdateFoodText();
            }
            else if (changedCard.cardDescription.cardType == Category.CardType.Creatures)
            {
                populationPanel.UpdatePopulationNode();
            }
        }
    }

    public void UpdateFoodText()
    {
        int foodAmount = 0;
        foreach (var (cardID, resourceCardAttr) in CardManager.Instance.GetResourceCardAttributes())
        {
            if (resourceCardAttr.resourceClassification == Category.ResourceCardClassification.Food)
            {
                foodAmount += resourceCardAttr.satietyValue;
            }
        }

        int neededFoodAmount = 0;
        foreach (var (cardID, creatureCardAttr) in CardManager.Instance.GetCreatureCardAttributes())
        {
            neededFoodAmount += creatureCardAttr.basicAttributes.satiety;
        }

        foodText.text = $"食物: {foodAmount}/{neededFoodAmount}";
    }
}