using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimeHoveredDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject displayPanel;
    private bool isDisplayed = false;
    private Text TimeText;
    void Awake()
    {
        TimeText = displayPanel.GetComponentInChildren<Text>();
    }
    void Update()
    {
        if (isDisplayed)
        {
            float leftTime = TimeManager.Instance.GetLeftTime();
            TimeText.text = $"剩余: {leftTime:F1} s\n当前回合: {CardManager.Instance.currentWave}";
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isDisplayed = true;
        displayPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isDisplayed = false;
        displayPanel.SetActive(false);
    }
}