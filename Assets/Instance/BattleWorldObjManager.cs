using System;
using System.Collections.Generic;
using Category.Battle;
using UnityEngine;

public class BattleWorldObjManager : MonoBehaviour
{
    public static List<B_Object> objects = new List<B_Object>();
    public static List<B_Creatures> creatures = new List<B_Creatures>();
    public static Dictionary<LineUp, List<B_Creatures>> teams = new Dictionary<LineUp, List<B_Creatures>>()
    {
        { LineUp.Player, new List<B_Creatures>() },
        { LineUp.Enemy, new List<B_Creatures>() }
    };
    public static B_Object draggingObj;

    public static List<B_Creatures> GetCreatures(LineUp lineUp) => teams[lineUp];

    #region Create Obj Api
    [Header("Create Obj Api Options")]
    public GameObject creaturePrefab;
    public GameObject canvas;

    [ContextMenu("Instantiate Obj")]
    public void InstantiateObj(LineUp targetLineUp = LineUp.Player, int targetPriority = 0)
    {
        if (creaturePrefab != null)
        {
            var obj = Instantiate(creaturePrefab, canvas.transform);
            var creature = obj.GetComponent<B_Creatures>();
            if (creature != null)
            {
                creature.LineUp = targetLineUp;
                creature.priority = targetPriority;
                AddObject(creature);
            }
            else
            {
                Debug.LogError("The prefab does not have a B_Creatures component.");
                Destroy(obj);
            }
        }
        else
        {
            Debug.LogError("Creature Prefab is not assigned.");
        }
    }
    #endregion

    public static void AddObject(B_Object obj)
    {
        objects.Add(obj);
        if (obj is B_Creatures creature)
        {
            creatures.Add(creature);
            if (creature.LineUp == LineUp.Player)
            {
                teams[LineUp.Player].Add(creature);
            }
            else if (creature.LineUp == LineUp.Enemy)
            {
                teams[LineUp.Enemy].Add(creature);
            }
            else
            {
                throw new Exception("Unknown LineUp");
            }
        }
        BattleWorldLifeManager.Instance.AddObject(obj);
    }

    public static void RemoveObject(B_Object obj)
    {
        objects.Remove(obj);
        if (obj is B_Creatures creature)
        {
            creatures.Remove(creature);
            if (creature.LineUp == LineUp.Player)
            {
                teams[LineUp.Player].Remove(creature);
            }
            else if (creature.LineUp == LineUp.Enemy)
            {
                teams[LineUp.Enemy].Remove(creature);
            }
            else
            {
                throw new Exception("Unknown LineUp");
            }
        }

        BattleWorldLifeManager.Instance.RemoveObject(obj);
    }

}