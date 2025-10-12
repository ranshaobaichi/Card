using UnityEngine;
using UnityEngine.EventSystems;

public class CloseButton : MonoBehaviour, IPointerClickHandler
{
    public GameObject targetUI;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetUI != null)
        {
            targetUI.SetActive(false);
        }
    }
}