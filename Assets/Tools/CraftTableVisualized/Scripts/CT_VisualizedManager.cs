using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CraftTableVisualized
{
    public class CT_VisualizedManager : MonoBehaviour
    {
        [SerializeField]
        private CraftTableDB _craftTableDB;
        
        #region Prefabs
        [Header("Prefabs")]
        [SerializeField]
        private GameObject _visualizedBtnPrefab;
        [SerializeField]
        private CT_VisualizedLines _linePrefab;
        [SerializeField]
        private GameObject _visualizedNodeStagePanelPrefab;
        #endregion
        
        #region Inspector References
        [Header("References")]
        [SerializeField]
        private CT_VisualizedView _view;
        [SerializeField] 
        private GameObject _visualizedBtnsPanel;
        [SerializeField]
        private GameObject _linesContainer;
        [SerializeField]
        private Camera _linesCamera;
        public GameObject _linkedNodePanel;
        #endregion
        
        [Header("Settings")]
        [SerializeField]
        private float _lineWidth = 2f;
        
        private List<CT_VisualizedNodeStagePanel> m_panels;
        private List<CT_VisualizedLines> m_lines;
        
        public CT_VisualizedBtn CreateVisualizedBtn()
            => Instantiate(_visualizedBtnPrefab, _visualizedBtnsPanel.transform).GetComponentInChildren<CT_VisualizedBtn>();
        
        private void Start()
        {
            var model = new CT_VisualizedModel();
            model.Init(_craftTableDB);

            var viewModel = new CT_VisualizedViewModel();
            viewModel.Init(model);

            _view.Init(viewModel, this);
            
            m_panels = new  List<CT_VisualizedNodeStagePanel>();
            m_lines = new List<CT_VisualizedLines>();
        }
        
        public CT_VisualizedNodeStagePanel CreateVisualizedNodeStagePanel()
        {
            var panel = Instantiate(_visualizedNodeStagePanelPrefab, _linkedNodePanel.transform).GetComponentInChildren<CT_VisualizedNodeStagePanel>();
            m_panels.Add(panel);
            return panel;
        }
        
        public void ClearPanels()
        {
            foreach (var panel in m_panels)
            {
                panel.ClearLinkedNodes();
                Destroy(panel.gameObject);
            }
            m_panels.Clear();
        }
        
        public CT_VisualizedLines CreateLine(CT_VisualizedLinkedNode from, CT_VisualizedLinkedNode to, CT_VisualizedLinkedEdge edge)
        {
            var fromPanel = m_panels.Find(x => x.linkedNodes.ContainsKey(from));
            var toPanel = m_panels.Find(x => x.linkedNodes.ContainsKey(to));
            if (fromPanel == null || toPanel == null)
                return null;

            var fromGo = fromPanel.GetLinkedNode(from);
            var toGo = toPanel.GetLinkedNode(to);
            if (fromGo == null || toGo == null)
                return null;

            var containerRect = _linesContainer.GetComponent<RectTransform>();
            Vector3 fromWorld = GetWorldPositionForLine(fromGo);
            Vector3 toWorld = GetWorldPositionForLine(toGo);
            var fromLocal = WorldToContainerLocal(containerRect, fromWorld);
            var toLocal   = WorldToContainerLocal(containerRect, toWorld);

            var line = Instantiate(_linePrefab, _linesContainer.transform);
            line.Init(new Vector3(fromLocal.x, fromLocal.y, 0f), new Vector3(toLocal.x, toLocal.y, 0f), _lineWidth, useWorldSpace: false);
            m_lines.Add(line);
            return line;
        }

        private static Vector3 GetWorldPositionForLine(GameObject nodeGo)
        {
            var rt = nodeGo.transform as RectTransform;
            if (rt != null)
            {
                var corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                return (corners[0] + corners[2]) * 0.5f;
            }
            return nodeGo.transform.position;
        }


        private static Vector2 WorldToContainerLocal(RectTransform container, Vector3 worldPos)
        {
            Vector3 local = container.InverseTransformPoint(worldPos);
            return new Vector2(local.x, local.y);
        }
        
        public void ClearLines()
        {
            foreach (var line in m_lines)
            {
                Destroy(line.gameObject);
            }
            m_lines.Clear();
        }
    }
}