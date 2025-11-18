using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class E_OptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private EventUI eventUI;
    public E_CostDisplay CostDisplay;
    public Text OptionText;
    public Button OptionButton;
    private EventUIDB.EventUIAttribute.OptionAttribute optionAttr;
    private int optionIndex;


    public void Initialize(EventUI ui, EventUIDB.EventUIAttribute.OptionAttribute attr, int index)
    {
        optionAttr = attr;
        optionIndex = index;
        eventUI = ui;

        OptionText.text = optionAttr.optionText;
        // check whether interactable
        var cost = optionAttr.cost;
        bool canAfford = true;
        List<Card.CardDescription> cardsInSlot = eventUI.eventCard.cardSlot.cards.ConvertAll(c => c.cardDescription);
        foreach (var costCard in cost.cardsCost)
        {
            if (!cardsInSlot.Contains(costCard))
            {
                canAfford = false;
                break;
            }
        }
        CostDisplay.Initialize(optionAttr.cost);

        Debug.Log($"Option {optionIndex} canAfford: {canAfford}");
        if (!canAfford)
        {
            OptionButton.interactable = false;
        }
        else
        {
            OptionButton.onClick.AddListener(() =>
            {
                eventUI.eventCard.StartEvent(optionIndex, eventUI: eventUI);
                OptionButton.interactable = false;
            });
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CostDisplay.DisplayCosts();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CostDisplay.HideCosts();
    }
}