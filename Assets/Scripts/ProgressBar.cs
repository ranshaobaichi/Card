using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class ProgressBar : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Image fillImage;
    public Text progressText;
    public Text tooltipText;
    
    public float progress = 0f;
    private bool isRunning = false;
    private float duration = 1f;
    private bool isHovered = false;
    private bool permanentDisplayProgress = false;

    public event Action OnProgressComplete;
    
    public void PauseProgressBar() => isRunning = false;
    public void ResumeProgressBar() => isRunning = true;

    public void Start()
    {
        fillImage.fillAmount = 0f; // Initialize the fill amount to 0
    }

    public void SetTooltipText(string text)
    {
        if (tooltipText != null)
        {
            tooltipText.text = text;
        }
    }

    public void StartProgressBar(float totalTime, Action onComplete, float curTime = 0f)
    {
        gameObject.SetActive(true);
        // 设置总持续时间
        ResetProgress(true);
        progress = curTime;
        duration = totalTime > 0 ? totalTime : 1f;
        OnProgressComplete += onComplete;

        isHovered = false;
        permanentDisplayProgress = false;
        if (progressText != null)
            progressText.gameObject.SetActive(false);
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
    }

    public void StopProgressBar()
    {
        isRunning = false;
        OnProgressComplete = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isRunning) return;

        // 累加已经过时间
        progress += Time.deltaTime;

        // 计算填充比例
        float fillAmount = Mathf.Clamp01(progress / duration);
        fillImage.fillAmount = fillAmount;

        if (progressText != null && progressText.gameObject.activeSelf)
        {
            progressText.text = $"{(fillAmount * 100).ToString("F1")}%";
        }

        // 检查是否完成
        if (progress >= duration)
        {
            isRunning = false;
            OnProgressComplete?.Invoke();
        }
    }
    
    public void SetProgressValue(float value)
    {
        float percentage = value / duration;
        progress = Mathf.Clamp(value, 0f, duration);
    }
    
    public void ResetProgress(bool state)
    {
        isRunning = state;
        progress = 0f;
        fillImage.fillAmount = 0f;
        OnProgressComplete = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (progressText != null && !string.IsNullOrEmpty(progressText.text))
            progressText.gameObject.SetActive(true);
        if (tooltipText != null && !string.IsNullOrEmpty(tooltipText.text))
            tooltipText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (progressText != null && !permanentDisplayProgress)
            progressText.gameObject.SetActive(false);
        if (tooltipText != null && !permanentDisplayProgress)
            tooltipText.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isHovered && progressText != null)
        {
            // 将屏幕坐标转换为填充图片的本地坐标
            RectTransform rt = fillImage.rectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                permanentDisplayProgress = !permanentDisplayProgress;
                if (progressText != null)
                    progressText.gameObject.SetActive(permanentDisplayProgress);
                if (tooltipText != null)
                    tooltipText.gameObject.SetActive(permanentDisplayProgress);
            }
        }
    }
}