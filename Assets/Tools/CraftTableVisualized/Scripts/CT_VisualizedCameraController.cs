using UnityEngine;

namespace CraftTableVisualized
{
    public class CT_VisualizedCameraController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Camera cam;

        [Header("Move")]
        [SerializeField] private float moveSpeed = 10f; // 世界单位/秒

        [Header("Zoom (Orthographic Size)")]
        [SerializeField] private float zoomSpeed = 5f;  // 每个滚轮单位缩放强度
        [SerializeField] private float minOrthoSize = 2f;
        [SerializeField] private float maxOrthoSize = 30f;
        [SerializeField] private float zoomLerpSpeed = 12f;

        private float _targetOrthoSize;
        private CT_CameraControls _actions;

        private void Reset()
        {
            cam = GetComponent<Camera>();
        }

        private void Awake()
        {
            if (cam == null) cam = GetComponent<Camera>();
            _actions = new CT_CameraControls();
            _targetOrthoSize = cam.orthographicSize;
        }

        private void OnEnable()
        {
            _actions.Enable();
        }

        private void OnDisable()
        {
            _actions.Disable();
        }

        private void Update()
        {
            // 1) WASD 平移：Move 是 Vector2，x=左右，y=上下
            Vector2 move = _actions.camera.move.ReadValue<Vector2>();
            Vector3 delta = new Vector3(move.x, move.y, 0f) * (moveSpeed * Time.unscaledDeltaTime);
            transform.position += delta;

            // 2) 滚轮缩放：Zoom 是 float（Axis）
            float scroll = _actions.camera.zoom.ReadValue<float>();
            if (Mathf.Abs(scroll) > 0.0001f)
            {
                _targetOrthoSize = Mathf.Clamp(_targetOrthoSize - scroll * zoomSpeed, minOrthoSize, maxOrthoSize);
            }

            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                _targetOrthoSize,
                1f - Mathf.Exp(-zoomLerpSpeed * Time.unscaledDeltaTime) // 帧率无关写法
            );
        }
    }
}