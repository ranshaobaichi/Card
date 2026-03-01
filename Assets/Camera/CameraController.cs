using UnityEngine;

public class CameraController : MonoBehaviour {
  public Transform upRightBoundary;
  public Transform downLeftBoundary;

  [Header("移动设置")] public float moveSpeed = 200f;
  public float edgeThreshold = 20f; // 鼠标距离屏幕边缘多少像素时触发移动

  [Header("拖拽设置")] public bool enableDrag = true;
  public float dragSpeed = 1f;

  [Header("缩放设置")] public float zoomSpeed = 50f;
  public float minSize = 400f;
  public float maxSize = 800f;

  private Camera cam;
  private Vector3 dragOrigin;
  private bool isDragging = false;

  private void Start() {
    cam = GetComponent<Camera>();
    if (cam == null) {
      Debug.LogError("CameraController需要挂载在带有Camera组件的对象上！");
    }
  }

  private void Update() {
    HandleDrag();
    HandleEdgeMovement();
    HandleZoom();
  }

  private void HandleDrag() {
    if (!enableDrag) {
      return;
    }

    // 按下鼠标左键开始拖拽
    if (Input.GetMouseButtonDown(button: 1) && CardManager.Instance.isDragging == false) {
      dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
      isDragging = true;
    }

    // 拖拽中
    if (isDragging && Input.GetMouseButton(button: 1)) {
      var currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
      var difference = dragOrigin - currentPos;

      var newPosition = transform.position + difference * dragSpeed;

      // 限制摄像机移动范围
      if (upRightBoundary != null && downLeftBoundary != null) {
        newPosition.x = Mathf.Clamp(newPosition.x, downLeftBoundary.position.x, upRightBoundary.position.x);
        newPosition.y = Mathf.Clamp(newPosition.y, downLeftBoundary.position.y, upRightBoundary.position.y);
      }

      transform.position = newPosition;
      dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    // 松开鼠标停止拖拽
    if (Input.GetMouseButtonUp(button: 1)) {
      isDragging = false;
    }
  }

  private void HandleEdgeMovement() {
    // 拖拽时禁用边缘移动
    if (isDragging) {
      return;
    }

    var moveDirection = Vector3.zero;

    // 检测鼠标位置
    var mousePosition = Input.mousePosition;

    // 检测左边缘
    if (mousePosition.x < edgeThreshold) {
      moveDirection += Vector3.left;
    }
    // 检测右边缘
    else if (mousePosition.x > Screen.width - edgeThreshold) {
      moveDirection += Vector3.right;
    }

    // 检测下边缘
    if (mousePosition.y < edgeThreshold) {
      moveDirection += Vector3.down;
    }
    // 检测上边缘
    else if (mousePosition.y > Screen.height - edgeThreshold) {
      moveDirection += Vector3.up;
    }

    // 移动摄像机
    if (moveDirection != Vector3.zero) {
      var newPosition = transform.position + moveDirection * moveSpeed * Time.unscaledDeltaTime;

      // 限制摄像机移动范围
      if (upRightBoundary != null && downLeftBoundary != null) {
        newPosition.x = Mathf.Clamp(newPosition.x, downLeftBoundary.position.x, upRightBoundary.position.x);
        newPosition.y = Mathf.Clamp(newPosition.y, downLeftBoundary.position.y, upRightBoundary.position.y);
      }

      transform.position = newPosition;
    }
  }

  private void HandleZoom() {
    if (cam == null || !cam.orthographic) {
      return;
    }

    var mainUIManager = FindAnyObjectByType<MainUIManager>();
    if (mainUIManager != null) {
      if (mainUIManager.CraftTablePanel.activeSelf ||
          mainUIManager.PopulationPanel.activeSelf ||
          mainUIManager.TaskPanel.activeSelf) {
        return; // 如果任一面板打开,禁止缩放
      }
    }

    if (EventUI.openEventUICount > 0) {
      return; // 如果有事件UI打开,禁止缩放
    }

    // 获取鼠标滚轮输入
    var scrollInput = Input.GetAxis("Mouse ScrollWheel");

    if (scrollInput != 0) {
      // 调整摄像机的orthographicSize
      var newSize = cam.orthographicSize - scrollInput * zoomSpeed;
      cam.orthographicSize = Mathf.Clamp(newSize, minSize, maxSize);
    }
  }
}