using UnityEngine;
using UnityEngine.UI;
using System;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    
    public float progress = 0f;
    private bool isRunning = false;
    private float duration = 1f;

    public event Action OnProgressComplete;
    
    public void PauseProgressBar() => isRunning = false;
    public void ResumeProgressBar() => isRunning = true;

    public void Start()
    {
        fillImage.fillAmount = 0f; // Initialize the fill amount to 0
    }

    public void StartProgressBar(float totalTime, Action onComplete, float curTime = 0f)
    {
        // 设置总持续时间
        ResetProgress(true);
        progress = curTime;
        duration = totalTime > 0 ? totalTime : 1f;
        OnProgressComplete += onComplete;
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
}