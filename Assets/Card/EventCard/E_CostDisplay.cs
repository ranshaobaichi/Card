using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class E_CostDisplay : MonoBehaviour
{
    private EventUIDB.EventUIAttribute.OptionAttribute.CostValue costValue;
    public Text cardsCostText;
    public Text timeCostText;
    public void Initialize(EventUIDB.EventUIAttribute.OptionAttribute.CostValue cost)
    {
        costValue = cost;
        // Prepare cost texts
        if (costValue.cardsCost.Count > 0)
        {
            cardsCostText.text = "";
            Dictionary<Card.CardDescription, int> cardCountDict = new Dictionary<Card.CardDescription, int>();
            foreach (var card in costValue.cardsCost)
            {
                if (cardCountDict.ContainsKey(card))
                {
                    cardCountDict[card]++;
                }
                else
                {
                    cardCountDict[card] = 1;
                }
            }

            foreach (var (cardDes, count) in cardCountDict)
            {
                cardsCostText.text += $"{cardDes} x {count}\n";
            }
            cardsCostText.text = cardsCostText.text.TrimEnd('\n');
        }
        else
        {
            cardsCostText.text = "无";
        }

        timeCostText.text = costValue.timeCost.ToString() + "秒";
    }

    public void DisplayCosts()
    {
        gameObject.SetActive(true);
    }

    public void HideCosts()
    {
        gameObject.SetActive(false);
    }
}