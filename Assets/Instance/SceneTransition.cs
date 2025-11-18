using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;
    
    [SerializeField] private RectTransform transitionPanel;
    [SerializeField] private Image transitionImage;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 确保Canvas存在
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // 最高渲染层级
            
            // 初始化时隐藏
            if (transitionPanel != null)
            {
                // 使用 SetSizeWithCurrentAnchors 确保正确尺寸（考虑 CanvasScaler）
                transitionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 转场覆盖效果 (从上到下)
    /// </summary>
    public IEnumerator TransitionOut()
    {
        if (transitionPanel == null || transitionImage == null)
        {
            Debug.LogWarning("SceneTransition: Missing references!");
            yield break;
        }

        // 设置锚点在顶部（水平方向拉伸）
        transitionPanel.anchorMin = new Vector2(0, 1);
        transitionPanel.anchorMax = new Vector2(1, 1);
        transitionPanel.pivot = new Vector2(0.5f, 1);
        transitionPanel.anchoredPosition = Vector2.zero;
        
        transitionImage.color = Color.black;
        
        float elapsed = 0;
        // 使用 canvas 像素高度（兼容 CanvasScaler），没有则回退到 Screen.height
        float canvasHeight = (canvas != null) ? canvas.pixelRect.height : Screen.height;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 使用unscaledDeltaTime避免时间缩放影响
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float curveValue = transitionCurve.Evaluate(t);
            
            float height = canvasHeight * curveValue;
            transitionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            yield return null;
        }
        
        transitionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasHeight);
    }

    /// <summary>
    /// 转场消失效果 (从下到上)
    /// </summary>
    public IEnumerator TransitionIn()
    {
        if (transitionPanel == null || transitionImage == null)
        {
            Debug.LogWarning("SceneTransition: Missing references!");
            yield break;
        }

        // 设置锚点在底部（水平方向拉伸）
        transitionPanel.anchorMin = new Vector2(0, 0);
        transitionPanel.anchorMax = new Vector2(1, 0);
        transitionPanel.pivot = new Vector2(0.5f, 0);
        transitionPanel.anchoredPosition = Vector2.zero;
        
        float elapsed = 0;
        float canvasHeight = (canvas != null) ? canvas.pixelRect.height : Screen.height;
        transitionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvasHeight);
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            float curveValue = transitionCurve.Evaluate(t);
            
            float height = canvasHeight * (1 - curveValue);
            transitionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            yield return null;
        }
        
        transitionPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
    }
}