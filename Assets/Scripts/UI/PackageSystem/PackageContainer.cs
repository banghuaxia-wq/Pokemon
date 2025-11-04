using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 背包/仓库容器 - 处理拖放到Viewport区域的逻辑
/// 重要：请将此组件添加到 Viewport 对象上，而不是Content！
/// 原因：Content有ContentSizeFitter，当所有格子为空时大小会变成0，无法接收拖拽
/// </summary>
public class PackageContainer : MonoBehaviour, IDropHandler
{
    [Header("容器设置")]
    [Tooltip("容器类型名称（用于调试）")]
    public string containerName = "容器";
    
    [Tooltip("Content对象（包含所有PackageUIItem的父物体）")]
    public Transform contentTransform;
    
    [Tooltip("此容器的所有格子（自动查找）")]
    public List<PackageUIItem> slots = new List<PackageUIItem>();
    
    [Header("自动堆叠设置")]
    [Tooltip("是否自动堆叠相同物品")]
    public bool autoStack = true;

    private void Start()
    {
        // 如果没有手动指定Content，尝试自动查找
        if (contentTransform == null)
        {
            // 假设结构为：Viewport -> Content
            contentTransform = transform.Find("Content");
            if (contentTransform == null)
            {
                Debug.LogError($"[PackageContainer] {containerName} 未找到Content！请手动拖入Content Transform。");
                return;
            }
        }
        
        // 自动查找Content下的所有 PackageUIItem
        if (slots.Count == 0)
        {
            slots.AddRange(contentTransform.GetComponentsInChildren<PackageUIItem>(true)); // true = 包括未激活的
            Debug.Log($"[PackageContainer] {containerName} 找到 {slots.Count} 个格子");
        }
    }

    /// <summary>
    /// 当物品拖拽到此容器时触发
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        PackageUIItem sourceItem = eventData.pointerDrag?.GetComponent<PackageUIItem>();
        
        if (sourceItem == null || sourceItem.GetItemData() == null)
        {
            return;
        }
        
        Debug.Log($"[PackageContainer] 物品拖入 {containerName}：{sourceItem.GetItemData().itemName}");
        
        // 检查是否按住Ctrl键（转移一半）
        bool transferHalf = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        
        if (transferHalf)
        {
            TransferHalfToContainer(sourceItem);
        }
        else
        {
            TransferToContainer(sourceItem);
        }
        
        // 确保清理拖拽图标（防止拖拽图标残留）
        sourceItem.CleanupDragIcon();
    }
    
    /// <summary>
    /// 转移物品到容器（全部转移）
    /// </summary>
    private void TransferToContainer(PackageUIItem sourceItem)
    {
        ItemData itemData = sourceItem.GetItemData();
        int itemCount = sourceItem.GetItemCount();
        
        if (itemData == null || itemCount <= 0)
        {
            return;
        }
        
        // 1. 如果开启自动堆叠，先尝试堆叠到现有物品
        if (autoStack)
        {
            int remainingCount = itemCount;
            
            foreach (var slot in slots)
            {
                if (slot.GetItemData() == itemData && slot.GetItemCount() < itemData.maxStackSize)
                {
                    int canAdd = itemData.maxStackSize - slot.GetItemCount();
                    int addCount = Mathf.Min(canAdd, remainingCount);
                    
                    slot.UpdateSlot(itemData, slot.GetItemCount() + addCount);
                    remainingCount -= addCount;
                    
                    Debug.Log($"[PackageContainer] 堆叠 {addCount} 个到现有格子，剩余 {remainingCount} 个");
                    
                    if (remainingCount <= 0)
                    {
                        // 全部堆叠完毕，清空源格子
                        sourceItem.UpdateSlot(null, 0);
                        return;
                    }
                }
            }
            
            // 如果还有剩余，放入空格子
            if (remainingCount > 0)
            {
                PackageUIItem emptySlot = FindFirstEmptySlot();
                if (emptySlot != null)
                {
                    emptySlot.UpdateSlot(itemData, remainingCount);
                    sourceItem.UpdateSlot(null, 0);
                    Debug.Log($"[PackageContainer] 剩余 {remainingCount} 个放入空格子");
                }
                else
                {
                    Debug.LogWarning($"[PackageContainer] {containerName} 已满，无法转移所有物品");
                    // 部分转移，更新源格子数量
                    sourceItem.UpdateSlot(itemData, remainingCount);
                }
            }
        }
        else
        {
            // 不自动堆叠，直接找空格子
            PackageUIItem emptySlot = FindFirstEmptySlot();
            if (emptySlot != null)
            {
                emptySlot.UpdateSlot(itemData, itemCount);
                sourceItem.UpdateSlot(null, 0);
                Debug.Log($"[PackageContainer] 转移 {itemCount} 个到空格子");
            }
            else
            {
                Debug.LogWarning($"[PackageContainer] {containerName} 已满");
            }
        }
    }
    
    /// <summary>
    /// 转移一半物品到容器
    /// </summary>
    private void TransferHalfToContainer(PackageUIItem sourceItem)
    {
        ItemData itemData = sourceItem.GetItemData();
        int itemCount = sourceItem.GetItemCount();
        
        if (itemData == null || itemCount <= 0)
        {
            return;
        }
        
        // 计算转移数量（向上取整）
        int transferCount = Mathf.CeilToInt(itemCount / 2f);
        int remainCount = itemCount - transferCount;
        
        Debug.Log($"[PackageContainer] Ctrl+拖拽：转移 {transferCount} 个，保留 {remainCount} 个");
        
        // 1. 如果开启自动堆叠，先尝试堆叠到现有物品
        if (autoStack)
        {
            int remainingTransfer = transferCount;
            
            foreach (var slot in slots)
            {
                if (slot.GetItemData() == itemData && slot.GetItemCount() < itemData.maxStackSize)
                {
                    int canAdd = itemData.maxStackSize - slot.GetItemCount();
                    int addCount = Mathf.Min(canAdd, remainingTransfer);
                    
                    slot.UpdateSlot(itemData, slot.GetItemCount() + addCount);
                    remainingTransfer -= addCount;
                    
                    if (remainingTransfer <= 0)
                    {
                        break;
                    }
                }
            }
            
            // 如果还有剩余，放入空格子
            if (remainingTransfer > 0)
            {
                PackageUIItem emptySlot = FindFirstEmptySlot();
                if (emptySlot != null)
                {
                    emptySlot.UpdateSlot(itemData, remainingTransfer);
                    remainingTransfer = 0;
                }
                else
                {
                    Debug.LogWarning($"[PackageContainer] {containerName} 空间不足");
                }
            }
            
            // 更新源格子（保留未能转移的部分）
            int actualTransferred = transferCount - remainingTransfer;
            sourceItem.UpdateSlot(itemData, itemCount - actualTransferred);
        }
        else
        {
            // 不自动堆叠，直接找空格子
            PackageUIItem emptySlot = FindFirstEmptySlot();
            if (emptySlot != null)
            {
                emptySlot.UpdateSlot(itemData, transferCount);
                sourceItem.UpdateSlot(itemData, remainCount);
                Debug.Log($"[PackageContainer] 转移一半：{transferCount} 个到空格子");
            }
            else
            {
                Debug.LogWarning($"[PackageContainer] {containerName} 已满");
            }
        }
    }
    
    /// <summary>
    /// 查找第一个空格子
    /// </summary>
    private PackageUIItem FindFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.GetItemData() == null)
            {
                return slot;
            }
        }
        return null;
    }
}

