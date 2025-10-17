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
    [Header("人口")]
    public Button PopulationBtn;
    public GameObject PopulationPanel;
    [Header("科技树")]
    public Button TechTreeBtn;
    public GameObject TechTreePanel;

    void Start()
    {
        TaskBtn.onClick.AddListener(() => ChangePanelState(TaskPanel));
        CraftTableBtn.onClick.AddListener(() => ChangePanelState(CraftTablePanel));
        PopulationBtn.onClick.AddListener(() => ChangePanelState(PopulationPanel));
    }

    public void ChangePanelState(GameObject panel)
    {
        bool isActive = panel.activeSelf;
        TaskPanel.SetActive(false);
        CraftTablePanel.SetActive(false);
        PopulationPanel.SetActive(false);
        panel.SetActive(!isActive);
    }

    private void AddRecipeNode()
    {
        throw new System.NotImplementedException();
    }
    
}