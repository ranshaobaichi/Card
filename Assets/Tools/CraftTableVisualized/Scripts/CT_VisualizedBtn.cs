using System;
using UnityEngine;
using UnityEngine.UI;

namespace CraftTableVisualized
{
    public class CT_VisualizedBtn : MonoBehaviour
    {
        [SerializeField]
        private Button _btn;
        [SerializeField]
        private DisplayCard _displayCard;
      
        [NonSerialized]
        public Card.CardDescription cardDescription;

        public void Init(Card.CardDescription cardDescription, Action<CT_VisualizedBtn> callback)
        {
            this.cardDescription = cardDescription;
            _displayCard.Initialize(cardDescription);
            _btn.onClick.AddListener(() => callback(this));
        }
    }
}

