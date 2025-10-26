using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CraftTableDB;

public class UI_CraftTablePanel : MonoBehaviour
{
    public Transform contentPanel;
    public GameObject foldNodePrefab;
    public GameObject subNodePrefab;
    public void AddRecipeNode(Recipe recipe)
    {
        // 创建折叠节点
        GameObject foldNode = Instantiate(foldNodePrefab, contentPanel);
        UI_FoldNode foldNodeScript = foldNode.GetComponent<UI_FoldNode>();
        Text foldNodeText = foldNode.GetComponentInChildren<Text>();
        GameObject inputCardsObj = Instantiate(subNodePrefab, contentPanel);
        Text inputText = inputCardsObj.GetComponentInChildren<Text>();
        GameObject outputCardObj = Instantiate(subNodePrefab, contentPanel);
        Text outputText = outputCardObj.GetComponentInChildren<Text>();
        foldNodeScript.AddItem(inputCardsObj);
        foldNodeScript.AddItem(outputCardObj);

        // 设置文本内容
        foldNodeText.text = $"配方:{recipe.recipeName}";
        if (recipe.inputCards.Count > 0)
        {
            Dictionary<Card.CardDescription, int> inputCardCounts = new Dictionary<Card.CardDescription, int>();
            foreach (var card in recipe.inputCards)
            {
                if (inputCardCounts.ContainsKey(card))
                    inputCardCounts[card]++;
                else
                    inputCardCounts[card] = 1;
            }

            string inputCardsStr = "所需卡牌:\n";
            int index = 0;
            foreach (var kvp in inputCardCounts)
            {
                index++;
                if (index < inputCardCounts.Count)
                    inputCardsStr += $"- {kvp.Key} x{kvp.Value}\n";
                else
                    inputCardsStr += $"- {kvp.Key} x{kvp.Value}";
            }
            inputText.text = inputCardsStr;
        }
        else
        {
            Debug.LogError($"配方 {recipe.recipeName} 没有输入卡牌！");
            inputText.text = "所需卡牌: 无";
        }

        string outputCardsStr = "产出卡牌:\n";
        if (recipe.outputCards.Count > 0)
        {
            for (int i = 0; i < recipe.outputCards.Count - 1; i++)
            {
                outputCardsStr += $"- {recipe.outputCards[i].cardDescription}\n";
            }
            outputCardsStr += $"- {recipe.outputCards[recipe.outputCards.Count - 1].cardDescription}";
            outputText.text = outputCardsStr;
        }
        else
        {
            Debug.LogError($"配方 {recipe.recipeName} 没有输出卡牌！");
            outputText.text = "产出卡牌: 无";
        }

        // 初始时折叠状态
        // inputCardsObj.SetActive(true);
        // outputCardObj.SetActive(true);
        // Canvas.ForceUpdateCanvases();
        // LayoutRebuilder.ForceRebuildLayoutImmediate(inputCardsObj.GetComponent<RectTransform>());
        // LayoutRebuilder.ForceRebuildLayoutImmediate(outputCardObj.GetComponent<RectTransform>());
        inputCardsObj.SetActive(false);
        outputCardObj.SetActive(false);
    }
}
