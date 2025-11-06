using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物品存储系统 - 管理玩家的仓库和背包物品数据
/// 使用单例模式，跨场景持久化
/// </summary>
public class ItemInventory : MonoBehaviour
{
    // 单例实例
    private static ItemInventory instance;
    public static ItemInventory Instance => instance;

    [Header("仓库数据")]
    [Tooltip("仓库物品列表（索引对应格子位置）")]
    public List<ItemSlotData> warehouseItems = new List<ItemSlotData>();
    
    [Header("背包数据")]
    [Tooltip("背包物品列表（索引对应格子位置）")]
    public List<ItemSlotData> bagItems = new List<ItemSlotData>();

    [Header("初始化设置")]
    [Tooltip("初始回血药数据")]
    public ItemData healMedicineData;
    
    [Tooltip("初始绳子数据")]
    public ItemData ropeData;
    
    [Tooltip("初始鞭子数据")]
    public ItemData whipData;
    
    [Tooltip("初始捕捉道具数据")]
    public ItemData captureData;

    [Header("容量设置")]
    [Tooltip("仓库容量")]
    public int warehouseCapacity = 40;
    
    [Tooltip("背包容量")]
    public int bagCapacity = 20;

    private void Awake()
    {
        // 单例模式（由父物体 PermanentManager 统一管理持久化）
        if (instance == null)
        {
            instance = this;
            
            // 初始化存储列表
            InitializeStorage();
            
            // 添加初始物品
            InitializeDefaultItems();
            
            Debug.Log("[ItemInventory] 单例已初始化");
        }
        else if (instance != this)
        {
            Debug.LogWarning("[ItemInventory] 检测到重复实例，销毁当前物体");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化存储列表（创建空格子）
    /// </summary>
    private void InitializeStorage()
    {
        // 初始化仓库格子
        warehouseItems.Clear();
        for (int i = 0; i < warehouseCapacity; i++)
        {
            warehouseItems.Add(new ItemSlotData());
        }

        // 初始化背包格子
        bagItems.Clear();
        for (int i = 0; i < bagCapacity; i++)
        {
            bagItems.Add(new ItemSlotData());
        }

        Debug.Log($"[ItemInventory] 初始化完成 - 仓库: {warehouseCapacity} 格，背包: {bagCapacity} 格");
    }

    /// <summary>
    /// 初始化默认物品（游戏开始时）
    /// </summary>
    private void InitializeDefaultItems()
    {
        // 仓库第一格：99个回血药
        if (healMedicineData != null)
        {
            warehouseItems[0] = new ItemSlotData(healMedicineData, 99);
            Debug.Log("[ItemInventory] 已添加初始物品: 回血药 x99");
        }

        // 仓库第二格：99个绳子
        if (ropeData != null)
        {
            warehouseItems[1] = new ItemSlotData(ropeData, 99);
            Debug.Log("[ItemInventory] 已添加初始物品: 绳子 x99");
        }

        // 仓库第三格：99个鞭子
        if (whipData != null)
        {
            warehouseItems[2] = new ItemSlotData(whipData, 99);
            Debug.Log("[ItemInventory] 已添加初始物品: 鞭子 x99");
        }
        
        // 仓库第四格：99个捕捉道具
        if (captureData != null)
        {
            warehouseItems[3] = new ItemSlotData(captureData, 99);
            Debug.Log("[ItemInventory] 已添加初始物品: 捕捉道具 x99");
        }
    }

    /// <summary>
    /// 添加物品到仓库
    /// </summary>
    public bool AddToWarehouse(ItemData itemData, int count)
    {
        if (itemData == null || count <= 0)
            return false;

        // 先尝试堆叠到现有格子
        for (int i = 0; i < warehouseItems.Count; i++)
        {
            if (warehouseItems[i].itemData == itemData && warehouseItems[i].count < itemData.maxStackSize)
            {
                int canAdd = Mathf.Min(count, itemData.maxStackSize - warehouseItems[i].count);
                warehouseItems[i].count += canAdd;
                count -= canAdd;

                if (count <= 0)
                    return true;
            }
        }

        // 如果还有剩余，放入空格子
        for (int i = 0; i < warehouseItems.Count; i++)
        {
            if (warehouseItems[i].itemData == null)
            {
                int addCount = Mathf.Min(count, itemData.maxStackSize);
                warehouseItems[i] = new ItemSlotData(itemData, addCount);
                count -= addCount;

                if (count <= 0)
                    return true;
            }
        }

        Debug.LogWarning("[ItemInventory] 仓库已满，无法添加更多物品！");
        return false;
    }

    /// <summary>
    /// 添加物品到背包
    /// </summary>
    public bool AddToBag(ItemData itemData, int count)
    {
        if (itemData == null || count <= 0)
            return false;

        // 先尝试堆叠到现有格子
        for (int i = 0; i < bagItems.Count; i++)
        {
            if (bagItems[i].itemData == itemData && bagItems[i].count < itemData.maxStackSize)
            {
                int canAdd = Mathf.Min(count, itemData.maxStackSize - bagItems[i].count);
                bagItems[i].count += canAdd;
                count -= canAdd;

                if (count <= 0)
                    return true;
            }
        }

        // 如果还有剩余，放入空格子
        for (int i = 0; i < bagItems.Count; i++)
        {
            if (bagItems[i].itemData == null)
            {
                int addCount = Mathf.Min(count, itemData.maxStackSize);
                bagItems[i] = new ItemSlotData(itemData, addCount);
                count -= addCount;

                if (count <= 0)
                    return true;
            }
        }

        Debug.LogWarning("[ItemInventory] 背包已满，无法添加更多物品！");
        return false;
    }

    /// <summary>
    /// 从仓库移除物品
    /// </summary>
    public void RemoveFromWarehouse(int index, int count)
    {
        if (index < 0 || index >= warehouseItems.Count)
            return;

        warehouseItems[index].count -= count;
        if (warehouseItems[index].count <= 0)
        {
            warehouseItems[index] = new ItemSlotData(); // 清空格子
        }
    }

    /// <summary>
    /// 从背包移除物品
    /// </summary>
    public void RemoveFromBag(int index, int count)
    {
        if (index < 0 || index >= bagItems.Count)
            return;

        bagItems[index].count -= count;
        if (bagItems[index].count <= 0)
        {
            bagItems[index] = new ItemSlotData(); // 清空格子
        }
    }
    
    /// <summary>
    /// 根据道具ID从背包移除物品
    /// </summary>
    /// <param name="itemID">道具ID</param>
    /// <param name="count">移除数量</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveItemByID(int itemID, int count)
    {
        for (int i = 0; i < bagItems.Count; i++)
        {
            if (bagItems[i].itemData != null && bagItems[i].itemData.itemID == itemID)
            {
                if (bagItems[i].count >= count)
                {
                    string itemName = bagItems[i].itemData.itemName; // 先保存名字
                    RemoveFromBag(i, count);
                    Debug.Log($"[ItemInventory] 从背包移除了 {count} 个 {itemName}");
                    return true;
                }
            }
        }
        Debug.LogWarning($"[ItemInventory] 背包中没有足够的道具 (ID: {itemID})");
        return false;
    }

    /// <summary>
    /// 交换仓库格子
    /// </summary>
    public void SwapWarehouseSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= warehouseItems.Count || indexB < 0 || indexB >= warehouseItems.Count)
            return;

        ItemSlotData temp = warehouseItems[indexA];
        warehouseItems[indexA] = warehouseItems[indexB];
        warehouseItems[indexB] = temp;
    }

    /// <summary>
    /// 交换背包格子
    /// </summary>
    public void SwapBagSlots(int indexA, int indexB)
    {
        if (indexA < 0 || indexA >= bagItems.Count || indexB < 0 || indexB >= bagItems.Count)
            return;

        ItemSlotData temp = bagItems[indexA];
        bagItems[indexA] = bagItems[indexB];
        bagItems[indexB] = temp;
    }
}

/// <summary>
/// 物品格子数据（可序列化，用于存档）
/// </summary>
[System.Serializable]
public class ItemSlotData
{
    public ItemData itemData;
    public int count;

    public ItemSlotData()
    {
        itemData = null;
        count = 0;
    }

    public ItemSlotData(ItemData data, int amount)
    {
        itemData = data;
        count = amount;
    }
}

