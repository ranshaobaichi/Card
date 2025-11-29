using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageText : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Text damageText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [Header("Animation Settings")]
    [SerializeField] private float floatDistance = 100f;
    [SerializeField] private float duration = 1.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector2 defaultSizeDelta;
    private Vector3 defaultScale;

    // 预设颜色
    public static class PresetColors
    {
        public static readonly Color Physical = new Color(1f, 0.5f, 0f); // 橙色 - 物理伤害
        public static readonly Color Spell = new Color(0.5f, 0.5f, 1f); // 蓝色 - 法术伤害
        public static readonly Color TrueDamage = Color.white; // 白色 - 真实伤害
        public static readonly Color Heal = new Color(0f, 1f, 0f); // 绿色 - 治疗
        public static readonly Color Critical = new Color(1f, 0f, 0f); // 红色 - 暴击
        public static readonly Color Dodge = new Color(0.7f, 0.7f, 0.7f); // 灰色 - 闪避

        public static Color GetDamageColor(Category.Battle.DamageType damageType)
        {
            switch (damageType)
            {
                case Category.Battle.DamageType.Physical:
                    return Physical;
                case Category.Battle.DamageType.Spell:
                    return Spell;
                case Category.Battle.DamageType.TrueDamage:
                    return TrueDamage;
                default:
                    return Color.white;
            }
        }
    }

    private Vector3 startPosition;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        // 如果没有赋值则自动获取组件
        if (damageText == null)
            damageText = GetComponent<Text>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // 如果没有CanvasGroup则添加
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        defaultSizeDelta = rectTransform.sizeDelta;
        defaultScale = rectTransform.localScale;

    }

    /// <summary>
    /// 初始化伤害文本
    /// </summary>
    /// <param name="text">显示的文本内容</param>
    /// <param name="color">文本颜色</param>
    public void Initialize(string text, Color color)
    {
        damageText.text = text;
        damageText.color = color;
        canvasGroup.alpha = 1f;
        // 使用 position 而不是 anchoredPosition，因为已经在 GetFromPool 中设置了正确的位置
        startPosition = transform.position;

        // 停止之前的动画（如果有）
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    /// <summary>
    /// 初始化伤害文本（使用数值）
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="color">文本颜色</param>
    public void Initialize(float damage, Color color)
    {
        string damageStr = damage >= 0 ? $"-{damage:F1}" : $"+{Mathf.Abs(damage):F1}";
        Initialize(damageStr, color);
    }

    private IEnumerator PlayAnimation()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // 向上漂浮 - 使用 position
            float moveProgress = moveCurve.Evaluate(t);
            Vector3 offset = Vector3.up * (floatDistance * moveProgress);
            transform.position = startPosition + offset;

            // 淡出效果
            canvasGroup.alpha = fadeCurve.Evaluate(t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 动画结束，返回对象池
        DamageTextPool.Instance.ReturnToPool(this);
    }

    /// <summary>
    /// 重置对象状态
    /// </summary>
    public void ResetState()
    {
        canvasGroup.alpha = 1f;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        rectTransform.sizeDelta = defaultSizeDelta;
        rectTransform.localScale = defaultScale;
    }
}