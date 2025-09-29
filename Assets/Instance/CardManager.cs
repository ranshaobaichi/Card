using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public GameObject cardPrefab;

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

    private long cardIdentityID;
    public bool isDragging;
    public Card draggingCard;

    public void Start()
    {
        cardIdentityID = 1;
        isDragging = false;
        draggingCard = null;
    }

    public long GetCardIdentityID()
    {
        long newID = cardIdentityID++;
        return newID % long.MaxValue;
    }

    public Card CreateCard(Card.CardDescription cardDescription, Vector2 position = default)
    {
        Card newCard = Instantiate(cardPrefab, position, Quaternion.identity).GetComponent<Card>();
        newCard.SetCardType(cardDescription);
        return newCard;
    }
}