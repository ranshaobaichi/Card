using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    [Serializable]
    public struct CardData
    {
        public long cardID;
        public Card.CardDescription cardDescription;
        public string attribute;
    }
    [Serializable]
    public struct CardSlotData
    {
        public List<long> cardIDs;
        public Vector2 position;
    }
    [Serializable]
    public struct SaveData
    {
        public long curCardID;
        public long curCardSlotID;
        public List<CardData> allCardData;
        public List<CardSlotData> allCardSlotData;
    }

    public static SaveDataManager Instance;
    public readonly string SaveDataFileName = "saveData.json";

    public bool SaveDataExists() => System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, "saveData.json"));

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        SceneManager.BeforeSceneChanged += () =>
        {
            if (SceneManager.currentScene == SceneManager.ProductionScene)
            {
                SaveGame();
            }
        };
    }

    [ContextMenu("Debug Serialize One Attribute")]
    public void DebugSerializeOneAttribute()
    {
        foreach (var kv in CardManager.Instance.allCards)
        {
            foreach (var card in kv.Value)
            {
                var cardDesc = card.cardDescription;
                if (cardDesc.cardType == Category.CardType.Creatures)
                {
                    var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(card.cardID);
                    Debug.Log($"Serialize Creature attr for {card.cardID}:\n{JsonUtility.ToJson(attr, true)}");
                    return;
                }
                if (cardDesc.cardType == Category.CardType.Resources)
                {
                    var attr = CardManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(card.cardID);
                    Debug.Log($"Serialize Resource attr for {card.cardID}:\n{JsonUtility.ToJson(attr, true)}");
                    return;
                }
            }
        }
    }

    [ContextMenu("Save Game")]
    public void SaveGame()
    {
        Debug.Log("Game saved.");
        List<CardData> allCardData = new List<CardData>();
        // Save all the cards attributes
        // Debug.Log($"Has cards: {CardManager.Instance.allCards.Count}");
        int count = 0;
        foreach (var cardsKV in CardManager.Instance.allCards)
        {
            Category.CardType cardType = cardsKV.Key;
            foreach (var card in cardsKV.Value)
            {
                count++;
                CardData cardData = new CardData
                {
                    cardID = card.cardID,
                    cardDescription = card.cardDescription,
                };
                switch (cardType)
                {
                    case Category.CardType.Creatures:
                        cardData.attribute = JsonUtility.ToJson(CardManager.Instance.GetCardAttribute<CardAttributeDB.CreatureCardAttribute>(card.cardID));
                        break;
                    case Category.CardType.Resources:
                        cardData.attribute = JsonUtility.ToJson(CardManager.Instance.GetCardAttribute<CardAttributeDB.ResourceCardAttribute>(card.cardID));
                        break;
                    case Category.CardType.Events:
                        // Currently no event card attributes to save
                        break;
                    default:
                        Debug.LogError($"Unknown card type: {cardType}");
                        break;
                }
                allCardData.Add(cardData);
            }
        }
        // Debug.Log($"Total cards saved: {count}");

        // Save all the card slots
        // Debug.Log($"Has card slots: {CardManager.Instance.allCardSlots.Count}");
        List<CardSlotData> allCardSlotData = new List<CardSlotData>();
        foreach (var cardSlot in CardManager.Instance.allCardSlots.Values)
        {
            CardSlotData cardSlotData = new CardSlotData
            {
                position = cardSlot.transform.position,
                cardIDs = new List<long>(),
            };
            foreach (var card in cardSlot.cards)
            {
                cardSlotData.cardIDs.Add(card.cardID);
            }
            allCardSlotData.Add(cardSlotData);
        }

        // Save to file
        SaveData saveData = new SaveData
        {
            curCardID = CardManager.Instance.CurCardID,
            curCardSlotID = CardManager.Instance.CurCardSlotID,
            allCardData = allCardData,
            allCardSlotData = allCardSlotData,
        };

        string json = JsonUtility.ToJson(saveData, prettyPrint: true);
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveDataFileName), json);
        Debug.Log($"Game saved to {System.IO.Path.Combine(Application.persistentDataPath, SaveDataFileName)}");
    }

    public bool TryGetSaveData(out SaveData saveData)
    {
        saveData = new SaveData();
        Debug.Log("Loading save data from path: " + System.IO.Path.Combine(Application.persistentDataPath, SaveDataFileName));
        if (!System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveDataFileName)))
        {
            Debug.LogWarning("No save data found, start a new game.");
            return false;
        }

        string json = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveDataFileName));
        saveData = JsonUtility.FromJson<SaveData>(json);
        return true;
    }

    public void DeleteSaveData()
    {
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, SaveDataFileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
            Debug.Log("Save data deleted.");
        }
        else
        {
            Debug.LogWarning("No save data found to delete.");
        }
    }
}