using UnityEngine;
using UnityEngine.UI;

public class HexLayoutGroup : LayoutGroup
{
    public float cellWidth = 100f;
    public float cellHeight = 86.6f;
    public int columns = 5;

    [Tooltip("放置时自动给子物体的 HexNode 设置轴向坐标")]
    public bool assignHexCoordinates = true;
    
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        
        // 计算总宽度，偶数行会比奇数行多一列
        float totalWidth = (columns + 0.5f) * cellWidth;
        
        // 添加padding到总宽度
        totalWidth += padding.left + padding.right;
        
        // 设置布局的水平尺寸
        SetLayoutInputForAxis(
            totalWidth, // minWidth
            totalWidth, // preferredWidth
            -1,         // flexibleWidth (不灵活)
            0           // axis (水平)
        );
    }
    
    public override void CalculateLayoutInputVertical()
    {
        // 计算行数
        int totalChildren = rectChildren.Count;
        int rows = CalculateRows(totalChildren);
        
        // 考虑到六边形垂直方向重叠
        float totalHeight = (rows <= 0) ? 0 : (rows - 1) * (cellHeight * 0.75f) + cellHeight;
        
        // 添加padding到总高度
        totalHeight += padding.top + padding.bottom;
        
        // 设置布局的垂直尺寸
        SetLayoutInputForAxis(
            totalHeight, // minHeight
            totalHeight, // preferredHeight
            -1,          // flexibleHeight (不灵活)
            1            // axis (垂直)
        );
    }
    
    // 计算给定子对象数量需要多少行
    private int CalculateRows(int childCount)
    {
        if (childCount <= 0) return 0;
        
        int row = 0;
        int processed = 0;
        
        while (processed < childCount)
        {
            // 偶数行多一个
            processed += (row % 2 == 0) ? columns + 1 : columns;
            row++;
        }
        
        return row;
    }

    public override void SetLayoutHorizontal() => UpdateChildren();
    public override void SetLayoutVertical() => UpdateChildren();

    private void UpdateChildren()
    {
        if (rectChildren.Count == 0)
            return;

        if (HexNodeManager.Instance != null)
        {
            HexNodeManager.Instance.evenColumns = columns;
        }

        // 计算布局的总大小
        int rows = CalculateRows(rectChildren.Count);
        float layoutWidth = (columns + 0.5f) * cellWidth;
        float layoutHeight = (rows <= 0) ? 0 : (rows - 1) * (cellHeight * 0.75f) + cellHeight;

        // 计算对齐偏移
        float alignmentOffsetX = 0;
        float alignmentOffsetY = 0;

        // 水平对齐
        if (childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter)
        {
            alignmentOffsetX = (rectTransform.rect.width - layoutWidth - padding.left - padding.right) * 0.5f;
        }
        else if (childAlignment == TextAnchor.UpperRight || childAlignment == TextAnchor.MiddleRight || childAlignment == TextAnchor.LowerRight)
        {
            alignmentOffsetX = rectTransform.rect.width - layoutWidth - padding.right;
        }
        else
        {
            alignmentOffsetX = padding.left;
        }

        // 垂直对齐
        if (childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleRight)
        {
            alignmentOffsetY = (rectTransform.rect.height - layoutHeight - padding.top - padding.bottom) * 0.5f;
        }
        else if (childAlignment == TextAnchor.LowerLeft || childAlignment == TextAnchor.LowerCenter || childAlignment == TextAnchor.LowerRight)
        {
            alignmentOffsetY = rectTransform.rect.height - layoutHeight - padding.bottom;
        }
        else
        {
            alignmentOffsetY = padding.top;
        }

        int index = 0;
        int row = 0;

        while (index < rectChildren.Count)
        {
            int rowColumns = (row % 2 == 0) ? columns : columns + 1;

            for (int col = 0; col < rowColumns && index < rectChildren.Count; col++)
            {
                var item = rectChildren[index];
                float offsetX = (row % 2 == 0) ? 0 : cellWidth * 0.5f;
                float posX = col * cellWidth - offsetX + alignmentOffsetX;
                float posY = row * (cellHeight * 0.75f) + alignmentOffsetY;

                SetChildAlongAxis(item, 0, posX, cellWidth);
                SetChildAlongAxis(item, 1, posY, cellHeight);

                if (assignHexCoordinates)
                {
                    var node = item.GetComponent<HexNode>();
                    if (node != null)
                    {
                        // odd-r 偏移坐标 转 axial
                        int q = col;
                        int r = row;
                        node.coord = new AxialCoordinate(r, q);
                    }
                }

                index++;
            }
            row++;
        }
        
        HexNodeManager.Instance?.InitHexNodeBoard();
    }
    
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        columns = Mathf.Max(1, columns);
        cellWidth = Mathf.Max(1f, cellWidth);
        cellHeight = Mathf.Max(1f, cellHeight);
        SetDirty();
    }
#endif
}