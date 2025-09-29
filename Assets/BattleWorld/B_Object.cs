using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class B_Object : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public HexNode hexNode;
    [Range(1, 10)] public int priority;
    protected Image image;
    protected Vector2 oriPosition;

    #region Lifecycle
    public abstract void BeforeTick();
    public abstract void Tick();
    public abstract void AfterTick();
    #endregion

    void Awake()
    {
        image = GetComponent<Image>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        oriPosition = transform.position;
        BattleWorldObjManager.draggingObj = this;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            var node = eventData.pointerCurrentRaycast.gameObject.GetComponent<HexNode>();
            if (node != null && node.walkable)
            {
                HexNodeManager.MoveObject(this, hexNode, node);
            }
            else
            {
                transform.position = oriPosition;
            }
        }

        BattleWorldObjManager.draggingObj = null;
        image.raycastTarget = true;
    }
}