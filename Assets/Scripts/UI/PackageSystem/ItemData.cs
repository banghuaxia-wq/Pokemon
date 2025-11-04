using UnityEngine;

// 允许在 Unity 的 'Create' 菜单中创建新的物品数据
[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    // 物品 ID 或名称，用于唯一标识
    public int itemID;
    public string itemName = "New Item";
    
    // UI 需要显示的信息
    public Sprite itemIcon;
    public int maxStackSize = 1;

    // 物品描述 (可选)
    [TextArea(3, 10)]
    public string description;
}