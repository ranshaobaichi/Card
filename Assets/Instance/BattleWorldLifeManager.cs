using System;
using System.Collections.Generic;
using Category;
using UnityEngine;

public class BattleWorldLifeManager : MonoBehaviour
{
    public static BattleWorldLifeManager Instance;

    public event Action OnBeforeTick;
    public Dictionary<int, List<Action>> OnTick;
    public event Action OnAfterTick;

    // public int TickCount { get; private set; }
    // public float TickInterval = 1f;

    void Start()
    {
        OnTick = new Dictionary<int, List<Action>>();
    }

    public void AddObject(B_Object obj)
    {
        if (!OnTick.ContainsKey(obj.priority))
        {
            OnTick[obj.priority] = new List<Action>();
        }
        OnBeforeTick += obj.BeforeTick;
        OnTick[obj.priority].Add(obj.Tick);
        OnAfterTick += obj.AfterTick;
    }

    public void RemoveObject(B_Object obj)
    {
        OnBeforeTick -= obj.BeforeTick;
        OnTick.Remove(obj.priority);
        if (OnTick[obj.priority].Count == 0)
        {
            OnTick.Remove(obj.priority);
        }
        OnAfterTick -= obj.AfterTick;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteTick();
        }
    }

    private void ExecuteTick()
    {
        Debug.Log("Executing Tick");
        OnBeforeTick?.Invoke();
        // Tick前更新地图上地块物品的放置情况：
        HexNodeManager.Instance.UpdateWalkableState();
        foreach (var actionList in OnTick.Values)
        {
            foreach (var action in actionList)
            {
                action?.Invoke();
            }
        }
        OnAfterTick?.Invoke();
    }
}