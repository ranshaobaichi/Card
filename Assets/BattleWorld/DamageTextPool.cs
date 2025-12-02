using UnityEngine;
using System.Collections.Generic;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private Transform poolParent;

    private Queue<DamageText> pool = new Queue<DamageText>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 如果没有指定父对象，使用当前对象
        if (poolParent == null)
            poolParent = transform;

        // 预先创建对象池
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewDamageText();
        }
    }

    private DamageText CreateNewDamageText()
    {
        DamageText damageText = Instantiate(damageTextPrefab, poolParent);
        damageText.gameObject.SetActive(false);
        pool.Enqueue(damageText);
        return damageText;
    }

    /// <summary>
    /// 从对象池获取伤害文本
    /// </summary>
    public DamageText GetFromPool(Vector3 position, Transform parent = null)
    {
        DamageText damageText;

        if (pool.Count > 0)
        {
            damageText = pool.Dequeue();
        }
        else
        {
            damageText = CreateNewDamageText();
        }

        damageText.gameObject.SetActive(true);
        
        if (parent != null)
        {
            // 先设置父对象，保持世界位置
            damageText.transform.SetParent(parent, true);
            // 然后设置位置
            damageText.transform.position = position;
        }
        else
        {
            damageText.transform.position = position;
        }

        damageText.ResetState();
        return damageText;
    }

    /// <summary>
    /// 将伤害文本返回对象池
    /// </summary>
    public void ReturnToPool(DamageText damageText)
    {
        damageText.gameObject.SetActive(false);
        damageText.transform.SetParent(poolParent, false);
        pool.Enqueue(damageText);
    }

    /// <summary>
    /// 快速显示伤害文本的辅助方法 - 用于UI元素
    /// </summary>
    public void ShowDamageText(Vector3 uiPosition, string text, Color color, Transform parent = null)
    {
        // DamageText damageText = GetFromPool(uiPosition, parent);
        DamageText damageText = Instantiate(damageTextPrefab, uiPosition, Quaternion.identity, poolParent);
        damageText.Initialize(text, color);
    }

    /// <summary>
    /// 快速显示伤害数值的辅助方法 - 用于UI元素
    /// </summary>
    public void ShowDamageText(Vector3 uiPosition, float damage, Color color, Transform parent = null)
    {
        // DamageText damageText = GetFromPool(uiPosition, parent);
        DamageText damageText = Instantiate(damageTextPrefab, uiPosition, Quaternion.identity, poolParent);
        damageText.Initialize(damage, color);
    }
}