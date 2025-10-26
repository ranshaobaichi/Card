using System.Collections.Generic;
using Category;
using Category.Production;
using UnityEngine;
using UnityEngine.UI;

public class UI_PopulationPanel : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject foldNodePrefab;
    public GameObject subNodePrefab;

    public void UpdatePopulationNode()
    {
        string WorkEfficiencyTypeToString(WorkEfficiencyType type)
        {
            return type switch
            {
                WorkEfficiencyType.VerySlow => "极慢",
                WorkEfficiencyType.Slow => "缓慢",
                WorkEfficiencyType.Normal => "普通",
                WorkEfficiencyType.Fast => "快速",
                WorkEfficiencyType.Frenzy => "狂热",
                WorkEfficiencyType.None => throw new System.NotImplementedException(),
                _ => "未知效率"
            };
        }
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var (cardID, attr) in CardManager.Instance.GetCreatureCardAttributes())
        {
            GameObject foldNode = Instantiate(foldNodePrefab, contentPanel);
            UI_FoldNode foldNodeScript = foldNode.GetComponent<UI_FoldNode>();
            Text foldNodeText = foldNode.GetComponentInChildren<Text>();
            foldNodeText.text = attr.creatureCardType.ToString();

            GameObject subNode = Instantiate(subNodePrefab, contentPanel);
            Text subNodeText = subNode.GetComponentInChildren<Text>();
            string efficiencyStr = "";
            efficiencyStr += "合成效率: " + WorkEfficiencyTypeToString(attr.basicAttributes.workEfficiencyAttributes.craftWorkEfficiency) + "\n";
            efficiencyStr += "探索效率: " + WorkEfficiencyTypeToString(attr.basicAttributes.workEfficiencyAttributes.exploreWorkEfficiency) + "\n";
            efficiencyStr += "互动效率: " + WorkEfficiencyTypeToString(attr.basicAttributes.workEfficiencyAttributes.interactWorkEfficiency);
            subNodeText.text = efficiencyStr;

            foldNodeScript.AddItem(subNode);
            subNode.SetActive(false);
        }
    }
}