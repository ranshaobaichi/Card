using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    public CloseButton closeBtn;
    public Canvas UICanvas;
    public GameObject cardDisplayPanel;
    public GameObject eventCardPrefab;
    [HideInInspector] public Card eventCard;

    public void Start()
    {
        closeBtn.OnClosed += CloseUI;
    }

    public void OpenUI(Vector2 position = default)
    {
        if (position != default)
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = position;
        }
        gameObject.SetActive(true);
        RefreshDisplayCards();
    }

    public void CloseUI()
    {
        foreach (Transform child in cardDisplayPanel.transform)
        {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }

    public void RefreshDisplayCards()
    {
        static void SetText(GameObject obj, string text)
        {
            var textComp = obj.GetComponentInChildren<Text>();
            if (textComp != null)
            {
                textComp.text = text;
            }
        }

        var cards = eventCard.cardSlot.cards;
        foreach (var card in cards)
        {
            if (card.cardDescription.cardType != Category.CardType.Events)
            {
                var cardGO = Instantiate(eventCardPrefab, cardDisplayPanel.transform);
                SetText(cardGO, card.GetCardTypeString());
                Debug.Log($"Displaying card: {card.GetCardTypeString()}");
            }
        }
    }
}