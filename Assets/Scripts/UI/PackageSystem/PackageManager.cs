using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包/仓库管理器 - 用于测试和管理物品
/// </summary>
public class PackageManager : MonoBehaviour
{
    [Header("UI 引用")]
    [Tooltip("仓库格子列表")]
    public List<PackageUIItem> warehouseSlots = new List<PackageUIItem>();
    
    [Tooltip("背包格子列表")]
    public List<PackageUIItem> bagSlots = new List<PackageUIItem>();

    [Header("测试数据")]
    [Tooltip("测试用的物品数据")]
    public ItemData testItemData;
    
    [Tooltip("测试物品数量")]
    public int testItemCount = 99;

    private void Start()
    {
        // 自动查找所有格子（如果没有手动拖入）
        if (warehouseSlots.Count == 0)
        {
            // 只在 Scroll View_Warehouse 中查找
            GameObject warehousePanel = GameObject.Find("Scroll View_Warehouse");
            if (warehousePanel != null)
            {
                PackageUIItem[] slots = warehousePanel.GetComponentsInChildren<PackageUIItem>();
                warehouseSlots.AddRange(slots);
                Debug.Log($"[PackageManager] 在仓库中找到 {slots.Length} 个格子（预期 40 个）");
            }
            else
            {
                Debug.LogWarning("[PackageManager] 未找到 'Scroll View_Warehouse'！请检查命名或手动拖入格子。");
            }
        }
        
        if (bagSlots.Count == 0)
        {
            // 只在 Scroll View_Bag 中查找
            GameObject bagPanel = GameObject.Find("Scroll View_Bag");
            if (bagPanel != null)
            {
                PackageUIItem[] slots = bagPanel.GetComponentsInChildren<PackageUIItem>();
                bagSlots.AddRange(slots);
                Debug.Log($"[PackageManager] 在背包中找到 {slots.Length} 个格子（预期 20 个）");
            }
            else
            {
                Debug.LogWarning("[PackageManager] 未找到 'Scroll View_Bag'！请检查命名或手动拖入格子。");
            }
        }
    }

    /// <summary>
    /// 测试：在仓库第一格放置物品
    /// </summary>
    [ContextMenu("测试：仓库添加血药")]
    public void TestAddHealMedicine()
    {
        if (warehouseSlots.Count > 0 && testItemData != null)
        {
            warehouseSlots[0].UpdateSlot(testItemData, testItemCount);
            Debug.Log($"已在仓库第一格添加 {testItemCount} 个 {testItemData.itemName}");
        }
        else
        {
            Debug.LogWarning("请先设置 Warehouse Slots 和 Test Item Data！");
        }
    }

    /// <summary>
    /// 测试：填满仓库前10格
    /// </summary>
    [ContextMenu("测试：填满前10格")]
    public void TestFillFirst10Slots()
    {
        if (testItemData == null)
        {
            Debug.LogWarning("请先设置 Test Item Data！");
            return;
        }

        int count = Mathf.Min(10, warehouseSlots.Count);
        for (int i = 0; i < count; i++)
        {
            warehouseSlots[i].UpdateSlot(testItemData, testItemCount);
        }
        Debug.Log($"已填满前 {count} 个格子");
    }

    /// <summary>
    /// 测试：清空所有格子
    /// </summary>
    [ContextMenu("测试：清空所有格子")]
    public void TestClearAllSlots()
    {
        foreach (var slot in warehouseSlots)
        {
            slot.UpdateSlot(null, 0);
        }
        
        foreach (var slot in bagSlots)
        {
            slot.UpdateSlot(null, 0);
        }
        Debug.Log("已清空所有格子");
    }

    /// <summary>
    /// 运行时测试按钮：添加物品到仓库
    /// </summary>
    public void AddItemToWarehouse()
    {
        if (warehouseSlots.Count > 0 && testItemData != null)
        {
            // 找到第一个空格子
            for (int i = 0; i < warehouseSlots.Count; i++)
            {
                if (warehouseSlots[i].GetItemData() == null)
                {
                    warehouseSlots[i].UpdateSlot(testItemData, testItemCount);
                    Debug.Log($"在格子 {i} 添加了 {testItemCount} 个 {testItemData.itemName}");
                    return;
                }
            }
            Debug.LogWarning("仓库已满！");
        }
    }

    /// <summary>
    /// 运行时测试按钮：添加物品到背包
    /// </summary>
    public void AddItemToBag()
    {
        if (bagSlots.Count > 0 && testItemData != null)
        {
            // 找到第一个空格子
            for (int i = 0; i < bagSlots.Count; i++)
            {
                if (bagSlots[i].GetItemData() == null)
                {
                    bagSlots[i].UpdateSlot(testItemData, testItemCount);
                    Debug.Log($"在背包格子 {i} 添加了 {testItemCount} 个 {testItemData.itemName}");
                    return;
                }
            }
            Debug.LogWarning("背包已满！");
        }
    }
}

