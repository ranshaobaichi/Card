using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform upRightBoundary;
    public Transform downLeftBoundary;
    
    [Header("移动设置")]
    public float moveSpeed = 200f;
    public float edgeThreshold = 20f; // 鼠标距离屏幕边缘多少像素时触发移动
    
    [Header("缩放设置")]
    public float zoomSpeed = 50f;
    public float minSize = 400f;
    public float maxSize = 800f;
    
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController需要挂载在带有Camera组件的对象上！");
        }
    }
    
    void Update()
    {
        HandleEdgeMovement();
        HandleZoom();
    }
    
    void HandleEdgeMovement()
    {
        Vector3 moveDirection = Vector3.zero;
        
        // 检测鼠标位置
        Vector3 mousePosition = Input.mousePosition;
        
        // 检测左边缘
        if (mousePosition.x < edgeThreshold)
        {
            moveDirection += Vector3.left;
        }
        // 检测右边缘
        else if (mousePosition.x > Screen.width - edgeThreshold)
        {
            moveDirection += Vector3.right;
        }
        
        // 检测下边缘
        if (mousePosition.y < edgeThreshold)
        {
            moveDirection += Vector3.down;
        }
        // 检测上边缘
        else if (mousePosition.y > Screen.height - edgeThreshold)
        {
            moveDirection += Vector3.up;
        }
        
        // 移动摄像机
        if (moveDirection != Vector3.zero)
        {
            Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.unscaledDeltaTime;
            
            // 限制摄像机移动范围
            if (upRightBoundary != null && downLeftBoundary != null)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, downLeftBoundary.position.x, upRightBoundary.position.x);
                newPosition.y = Mathf.Clamp(newPosition.y, downLeftBoundary.position.y, upRightBoundary.position.y);
            }
            
            transform.position = newPosition;
        }
    }
    
    void HandleZoom()
    {
        if (cam == null || !cam.orthographic) return;
        
        // 获取鼠标滚轮输入
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        if (scrollInput != 0)
        {
            // 调整摄像机的orthographicSize
            float newSize = cam.orthographicSize - scrollInput * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minSize, maxSize);
        }
    }
}