using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeButtonSprites : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ChangeType
    {
        改变按下图标,
        切换图标
    }
    public Sprite normalSprite;
    public Sprite pressedSprite;
    private UnityEngine.UI.Button button;
    public ChangeType changeType = ChangeType.改变按下图标;

    public void Start()
    {
        button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            Debug.LogError("ChangeButtonSprites: No Button component found!");
            return;
        }

        // 初始化为正常状态
        if (normalSprite != null && button.image != null)
        {
            button.image.sprite = normalSprite;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && button.interactable && changeType == ChangeType.改变按下图标)
        {
            button.image.sprite = pressedSprite;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (button != null && button.interactable)
        {
            if (changeType == ChangeType.改变按下图标)
            {
                button.image.sprite = normalSprite;
            }
            else if (changeType == ChangeType.切换图标)
            {
                button.image.sprite = button.image.sprite == normalSprite ? pressedSprite : normalSprite;
            }
        }
    }
}