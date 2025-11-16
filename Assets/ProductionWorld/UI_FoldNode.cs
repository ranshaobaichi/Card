using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FoldNode : MonoBehaviour
{
    public Button foldButton;
    public List<GameObject> foldableItems = new List<GameObject>();
    public bool isFolded;

    public void Start()
    {
        isFolded = true;
        foldButton.onClick.AddListener(ChangeFoldState);
    }

    public void OnDisable()
    {
        if (!isFolded) ChangeFoldState();
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
