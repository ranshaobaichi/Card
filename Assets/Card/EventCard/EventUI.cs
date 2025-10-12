using UnityEngine;
using UnityEngine.UI;

public class EventUI : MonoBehaviour
{
    public CloseButton closeBtn;
    public Canvas UICanvas;

    public void CloseUI() => closeBtn.OnPointerClick(null);
}