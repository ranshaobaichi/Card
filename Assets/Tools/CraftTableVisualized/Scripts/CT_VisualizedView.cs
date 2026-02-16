using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CraftTableVisualized
{
    public class CT_VisualizedView : MonoBehaviour
    {
        private CT_VisualizedViewModel _viewModel;
        private CT_VisualizedManager _manager;
        private List<CT_VisualizedBtn> _btns;

        public void Init(CT_VisualizedViewModel viewModel, CT_VisualizedManager manager)
        {
            _viewModel = viewModel;
            _manager = manager;
            _btns = new List<CT_VisualizedBtn>();
            foreach (var cardDescription in _viewModel.cardDescriptions)
            {
                var btn = _manager.CreateVisualizedBtn();
                btn.Init(cardDescription, UpdateSelectedCard);
                _btns.Add(btn);
            }
        }
        
        private void UpdateSelectedCard(CT_VisualizedBtn btn)
        {
            // foreach (var x in _btns)
            // {
            //     x.SetInteractable(true);
            // }
            // btn.SetInteractable(false);
            if (_viewModel.UpdateSelectedCard(btn.cardDescription))
            {
                Render();
            }
        }

        private void Render()
        {
            _manager.ClearPanels();
            _manager.ClearLines();
            
            // Create panels for each stage and populate them with linked nodes
            foreach (var nodes in _viewModel.stageNodes)
            {
                var panel = _manager.CreateVisualizedNodeStagePanel();
                foreach (var node in nodes)
                {
                    panel.CreateLinkedNode(node);
                }
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_manager._linkedNodePanel.GetComponent<RectTransform>());
            
            // Draw lines between the panels based on the edges
            foreach (var edges in _viewModel.stageEdges)
            {
                foreach (var edge in edges)
                {
                    _manager.CreateLine(edge.FromCard, edge.ToCard, edge);
                }
            }
        }
    }
}