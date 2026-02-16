using UnityEngine;

namespace CraftTableVisualized
{
    public class CT_VisualizedLines : MonoBehaviour
    {
        [SerializeField]
        private LineRenderer _lineRenderer;

        public void Init(Vector3 start, Vector3 end, float width, bool useWorldSpace)
        {
            _lineRenderer.useWorldSpace = useWorldSpace;
            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }
    }
}