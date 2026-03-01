using UnityEngine;
using UnityEngine.InputSystem;

namespace CraftTableVisualized {
  public class CT_CameraInputReceiver : MonoBehaviour {
    [Header("Refs")]
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Transform camTransform;

    [Header("Move")]
    [SerializeField]
    private float moveSpeed = 10f;

    [Header("Zoom (Orthographic)")]
    [SerializeField]
    private float zoomSpeed = 5f; // 每个滚轮单位缩放强度
    [SerializeField]
    private float minOrthoSize = 2f;
    [SerializeField]
    private float maxOrthoSize = 30f;
    [SerializeField]
    private float zoomLerpSpeed = 12f;

    private Vector2 _moveInput;
    private float _scrollInput;
    private float _targetOrthoSize;

    private void Reset() {
      cam = Camera.main;
      camTransform = cam != null ? cam.transform : null;
    }

    private void Awake() {
      if (cam == null) {
        cam = Camera.main;
      }

      if (camTransform == null && cam != null) {
        camTransform = cam.transform;
      }

      if (cam != null) {
        _targetOrthoSize = cam.orthographicSize;
      }
    }

    private void Update() {
      if (camTransform == null || cam == null) {
        return;
      }

      // 1) WASD 平移：Move 是 Vector2，x=左右，y=上下
      Vector3 delta = new Vector3(_moveInput.x, _moveInput.y, 0f) * (moveSpeed * Time.unscaledDeltaTime);
      camTransform.position += delta;

      // 2) 滚轮缩放：Zoom 是 float（Axis）
      if (Mathf.Abs(_scrollInput) > 0.0001f || Mathf.Abs(cam.orthographicSize - _targetOrthoSize) > 0.0001f)
      {
        _targetOrthoSize = Mathf.Clamp(_targetOrthoSize - _scrollInput * zoomSpeed, minOrthoSize, maxOrthoSize);
        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            _targetOrthoSize,
            1f - Mathf.Exp(-zoomLerpSpeed * Time.unscaledDeltaTime) // 帧率无关写法
        );
      }
    }
    
    // Move(performed + canceled) 都绑到这个
    public void OnMove(InputAction.CallbackContext ctx) {
      if (ctx.canceled) { _moveInput = Vector2.zero; return; }
      _moveInput = ctx.ReadValue<Vector2>();
    }

    // Zoom(performed) 绑到这个
    public void OnZoom(InputAction.CallbackContext ctx) {
      _scrollInput = ctx.ReadValue<float>(); // <Mouse>/scroll/y
      if (ctx.canceled || Mathf.Abs(_scrollInput) < 0.0001f) {
        _scrollInput = 0f;
      }
    }
  }
}