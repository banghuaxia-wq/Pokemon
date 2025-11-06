using UnityEngine;

/// <summary>
/// 物品类型枚举
/// </summary>
public enum ItemType
{
    None = 0,       // 无类型（默认）
    Tool = 1,       // 道具类型（战斗中可用）
    Gift = 2,       // 礼物类型（战斗中可用）
    ToolAndGift = 3 // 既是道具又是礼物
}

// 允许在 Unity 的 'Create' 菜单中创建新的物品数据
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("基础信息")]
    // 物品 ID 或名称，用于唯一标识
    public int itemID;
    public string itemName = "New Item";
    
    // UI 需要显示的信息
    public Sprite itemIcon;
    public int maxStackSize = 1;

    // 物品描述 (可选)
    [TextArea(3, 10)]
    public string description;
    
    [Header("物品类型")]
    [Tooltip("物品类型：None=普通物品, Tool=道具(战斗可用), Gift=礼物(战斗可用), ToolAndGift=两者都是")]
    public ItemType itemType = ItemType.None;
    
    [Header("道具效果")]
    [Tooltip("治疗量（仅对治疗道具有效）")]
    public int healAmount = 0;
    
    /// <summary>
    /// 判断是否是道具类型（战斗中可用）
    /// </summary>
    public bool IsTool()
    {
        return itemType == ItemType.Tool || itemType == ItemType.ToolAndGift;
    }
    
    /// <summary>
    /// 判断是否是礼物类型（战斗中可用）
    /// </summary>
    public bool IsGift()
    {
        return itemType == ItemType.Gift || itemType == ItemType.ToolAndGift;
    }
}