using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 背包/仓库管理器 - 用于测试和管理物品
/// </summary>
public class PackageManager : MonoBehaviour
{
    // 单例实例
    private static PackageManager instance;
    public static PackageManager Instance => instance;

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

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        // 设置单例
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.LogWarning("[PackageManager] 场景中存在多个 PackageManager，这可能导致问题");
        }

        // 获取或添加 CanvasGroup 组件
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 初始隐藏面板
        HidePanel();
    }

    private void Start()
    {
        // 延迟一帧初始化，确保所有 PackageUIItem 都已初始化
        StartCoroutine(DelayedInitialize());
    }

    private IEnumerator DelayedInitialize()
    {
        // 等待一帧，让所有 Start() 执行完毕
        yield return null;
        
        // 自动查找所有格子（如果没有手动拖入）
        if (warehouseSlots.Count == 0)
        {
            // 只在 Scroll View_Warehouse 中查找
            GameObject warehousePanel = GameObject.Find("Scroll View_Warehouse");
            if (warehousePanel != null)
            {
                PackageUIItem[] slots = warehousePanel.GetComponentsInChildren<PackageUIItem>(true); // true = 包括未激活的
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
                PackageUIItem[] slots = bagPanel.GetComponentsInChildren<PackageUIItem>(true); // true = 包括未激活的
                bagSlots.AddRange(slots);
                Debug.Log($"[PackageManager] 在背包中找到 {slots.Length} 个格子（预期 20 个）");
            }
            else
            {
                Debug.LogWarning("[PackageManager] 未找到 'Scroll View_Bag'！请检查命名或手动拖入格子。");
            }
        }

        // 设置格子索引和容器类型
        InitializeSlotIndices();

        // 从 ItemInventory 加载数据
        LoadItemsFromInventory();
    }
    
    /// <summary>
    /// 初始化格子索引和容器类型
    /// </summary>
    private void InitializeSlotIndices()
    {
        // 设置仓库格子
        for (int i = 0; i < warehouseSlots.Count; i++)
        {
            if (warehouseSlots[i] != null)
            {
                warehouseSlots[i].isWarehouseSlot = true;
                warehouseSlots[i].slotIndex = i;
            }
        }
        
        // 设置背包格子
        for (int i = 0; i < bagSlots.Count; i++)
        {
            if (bagSlots[i] != null)
            {
                bagSlots[i].isWarehouseSlot = false;
                bagSlots[i].slotIndex = i;
            }
        }
        
        Debug.Log($"[PackageManager] 已初始化 {warehouseSlots.Count} 个仓库格子和 {bagSlots.Count} 个背包格子的索引");
    }

    /// <summary>
    /// 从 ItemInventory 加载物品数据到UI
    /// </summary>
    public void LoadItemsFromInventory()
    {
        if (ItemInventory.Instance == null)
        {
            Debug.LogWarning("[PackageManager] ItemInventory 未找到！");
            return;
        }

        // 加载仓库物品
        for (int i = 0; i < warehouseSlots.Count && i < ItemInventory.Instance.warehouseItems.Count; i++)
        {
            ItemSlotData slotData = ItemInventory.Instance.warehouseItems[i];
            warehouseSlots[i].UpdateSlot(slotData.itemData, slotData.count);
        }

        // 加载背包物品
        for (int i = 0; i < bagSlots.Count && i < ItemInventory.Instance.bagItems.Count; i++)
        {
            ItemSlotData slotData = ItemInventory.Instance.bagItems[i];
            bagSlots[i].UpdateSlot(slotData.itemData, slotData.count);
        }

        Debug.Log("[PackageManager] 已从 ItemInventory 加载物品数据");
    }

    /// <summary>
    /// 打开背包面板
    /// </summary>
    public void OpenPanel()
    {
        ShowPanel();
        LoadItemsFromInventory(); // 刷新显示
    }

    /// <summary>
    /// 关闭背包面板
    /// </summary>
    public void ClosePanel()
    {
        HidePanel();
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    private void ShowPanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    private void HidePanel()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
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

