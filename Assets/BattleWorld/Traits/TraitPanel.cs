using System.Collections.Generic;
using Category.Battle;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TraitPanel : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public Image traitIcon;
    public Text traitName;
    public Text traitDescription;
    public Text traitPopulation;
    public List<Text> levelDescriptions;
    private Vector3 _pointerOffset;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var rect = transform as RectTransform;
            var canvas = rect.GetComponentInParent<Canvas>();
            var cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            bool isPointerOver = RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, cam);
            if (!isPointerOver) gameObject.SetActive(false);
        }
    }

    public void InitializeForBattle(TraitItem traitItem)
    {
        var trait = traitItem.trait;
        var lineUp = traitItem.lineUp;

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
            if (lineUp == LineUp.Player) localPoint.x += 400;
            else localPoint.x -= 400;
            rt.anchoredPosition = localPoint;
        }
        else
        {
            // fallback for non-UI or missing canvas
            Vector3 mousePos = Input.mousePosition;
            transform.position = mousePos;
        }


        var attr = BattleWorldManager.Instance.traitAttributesDict[trait];
        B_Trait traitObj = BattleWorldManager.Instance.GetTraitObjDict(lineUp)[trait];
        traitIcon.sprite = attr.icon;
        traitName.text = trait.ToString();
        traitDescription.text = attr.description;

        // Set population text
        traitPopulation.text = traitItem.traitPopulation.text;

        // Set level descriptions
        int level = traitObj.level;
        levelDescriptions.ForEach(text => text.gameObject.SetActive(false));
        for (int i = 0; i < attr.levelDescriptions.Count; i++)
        {
            levelDescriptions[i].gameObject.SetActive(true);
            levelDescriptions[i].text = $"{attr.levelThresholds[i]}：{attr.levelDescriptions[i]}";
            Color color = levelDescriptions[i].color;
            color.a = i == level - 1 ? 1f : 0.5f;
            levelDescriptions[i].color = color;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        gameObject.SetActive(true);
    }

    public void InitializeNormal(Trait trait, Canvas canvas)
    {
        // Get the mouse position
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


        var attr = DataBaseManager.Instance.GetAllTraitAttributes()[trait];
        traitIcon.sprite = attr.icon;
        traitName.text = trait.ToString();
        traitDescription.text = attr.description;

        // Set population text
        string populationString = "";
        List<int> thresholds = attr.levelThresholds;
        for (int i = 0; i < thresholds.Count; i++)
        {
            if (i == thresholds.Count - 1)
                populationString += thresholds[i];
            else
                populationString += thresholds[i] + "/";
        }
        traitPopulation.text = populationString;
        Color popColor = traitPopulation.color;
        popColor.a = 1f;
        traitPopulation.color = popColor;

        // Set level descriptions
        levelDescriptions.ForEach(text => text.gameObject.SetActive(false));
        for (int i = 0; i < attr.levelDescriptions.Count; i++)
        {
            levelDescriptions[i].gameObject.SetActive(true);
            levelDescriptions[i].text = $"{attr.levelThresholds[i]}：{attr.levelDescriptions[i]}";
        }

        gameObject.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var _canvas = GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        _pointerOffset = mousePos - new Vector3(transform.position.x, transform.position.y);
    }

    public void OnDrag(PointerEventData eventData)
    {
        var _canvas = GetComponentInParent<Canvas>();
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out Vector3 mousePos);
        transform.position = mousePos - (Vector3)_pointerOffset;
    }
}
