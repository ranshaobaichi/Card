using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoldNode : MonoBehaviour
{
    public Button foldButton;
    public List<GameObject> foldableItems;
    public bool isFolded;

    public void Start()
    {
        if (foldButton == null) throw new System.Exception("FoldNode: foldButton is null");
        foldButton.onClick.AddListener(ChangeFoldState);
    }

    public void AddItem(GameObject item)
    {
        if (!foldableItems.Contains(item)) foldableItems.Add(item);
    }
    
    public void ChangeFoldState()
    {
        foreach (var item in foldableItems) item.SetActive(isFolded);
        isFolded = !isFolded;
    }
}
