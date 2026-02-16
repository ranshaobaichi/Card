using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CraftTableVisualized
{
    public class CT_VisualizedNodeStagePanel : MonoBehaviour
    {
        [SerializeField]
        private GameObject _displayCardPrefab;
        public Dictionary<CT_VisualizedLinkedNode, GameObject> linkedNodes = new Dictionary<CT_VisualizedLinkedNode, GameObject>();
        
        [CanBeNull]
        public GameObject GetLinkedNode(CT_VisualizedLinkedNode node) => linkedNodes.GetValueOrDefault(node);
        public GameObject CreateLinkedNode(CT_VisualizedLinkedNode node)
        {
            if (linkedNodes.ContainsKey(node))
            {
                return linkedNodes[node];
            }
            var linkedNode = Instantiate(_displayCardPrefab, transform);
            linkedNodes.Add(node, linkedNode);
            var displayCard = linkedNode.GetComponent<DisplayCard>();
            if (displayCard != null)
            {
                displayCard.Initialize(node.Card);
            }
            return linkedNode;
        }

        public void ClearLinkedNodes()
        {
            foreach (var linkedNode in linkedNodes.Values)
            {
                Destroy(linkedNode);
            }
            linkedNodes.Clear();
        }
    }
}
