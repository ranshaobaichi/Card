using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipText : MonoBehaviour
{
    public enum TooltipMode
    {
        Normal, Warning, Error
    }
    private Text tooltipText;
    private static Queue<TooltipText> tooltipQueue = new Queue<TooltipText>();
    private const int maxTooltipCount = 3;
    public float appearTime = 2f;
    public float durationTime = 1f;

    private void Awake()
    {
        tooltipText = GetComponent<Text>();
    }

    public static void ClearAllTooltips()
    {
        while (tooltipQueue.Count > 0)
        {
            TooltipText oldTooltip = tooltipQueue.Dequeue();
            if (oldTooltip != null)
            {
                oldTooltip.StopAllCoroutines();
                Destroy(oldTooltip.gameObject);
            }
        }
    }

    public void SetTooltipText(string text, TooltipMode mode = TooltipMode.Normal, float appear = -1f, float duration = -1f)
    {
        // Get the mouse position
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform rt = transform as RectTransform;
        if (canvas != null && rt != null)
        {
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                cam,
                out Vector2 localPoint
            );
            rt.anchoredPosition = localPoint;
        }
        else
        {
            // fallback for non-UI or missing canvas
            Vector3 mousePos = Input.mousePosition;
            transform.position = mousePos;
        }


        tooltipText.text = text;
        duration = duration < 0 ? durationTime : duration;
        appear = appear < 0 ? appearTime : appear;
        tooltipText.color = mode switch
        {
            TooltipMode.Normal => Color.black,
            TooltipMode.Warning => Color.yellow,
            TooltipMode.Error => Color.red,
            _ => throw new System.NotImplementedException(),
        };

        while (tooltipQueue.Count > maxTooltipCount)
        {
            TooltipText oldTooltip = tooltipQueue.Dequeue();
            if (oldTooltip != null)
            {
                oldTooltip.StopAllCoroutines();
                Destroy(oldTooltip.gameObject);
            }
        }

        tooltipQueue.Enqueue(this);
        StartCoroutine(Disappear(appear, duration));
    }

    private IEnumerator Disappear(float appear, float duration)
    {
        yield return new WaitForSeconds(appear);
        float alpha = tooltipText.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(alpha, 0, elapsed / duration);
            tooltipText.color = new Color(tooltipText.color.r, tooltipText.color.g, tooltipText.color.b, newAlpha);
            yield return null;
        }
        tooltipText.text = "";
        Destroy(gameObject);
    }
}
