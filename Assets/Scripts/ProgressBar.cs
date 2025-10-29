using UnityEngine;
using UnityEngine.UI;
using System;

public class ProgressBar : MonoBehaviour
{
    public Image fillImage;
    
    public float currentTime = 0f;
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
        currentTime = curTime;
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
        currentTime += Time.deltaTime;

        // 计算填充比例
        float progress = Mathf.Clamp01(currentTime / duration);
        fillImage.fillAmount = progress;

        // 检查是否完成
        if (currentTime >= duration)
        {
            isRunning = false;
            OnProgressComplete?.Invoke();
        }
    }
    
    // 手动设置进度值(0-1)
    public void SetProgressValue(float value)
    {
        isRunning = false;
        fillImage.fillAmount = Mathf.Clamp01(value);
    }
    
    public void ResetProgress(bool state)
    {
        isRunning = state;
        currentTime = 0f;
        fillImage.fillAmount = 0f;
        OnProgressComplete = null;
    }
}