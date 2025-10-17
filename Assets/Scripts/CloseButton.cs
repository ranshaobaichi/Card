using UnityEngine;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour, IPointerClickHandler
{
    public GameObject targetUI;
    public event System.Action OnClosed;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetUI != null)
        {
            targetUI.SetActive(false);
            OnClosed?.Invoke();
        }
    }
}