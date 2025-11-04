using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class PackageUIItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // UI 组件的引用，通过 Inspector 拖拽赋值
    [Header("UI References")]
    [SerializeField] private Image iconImage;           // 物品图标
    [SerializeField] private TextMeshProUGUI countText; // 数量文本
    [SerializeField] private GameObject defaultBg;      // 默认背景
    [SerializeField] private GameObject selectedBg;     // 选中背景

    // 当前物品数据和数量
    private ItemData currentItemData;
    private int currentItemCount = 0;
    
    // 静态变量：追踪当前选中的格子
    private static PackageUIItem currentSelectedItem = null;
    
    // 拖拽相关
    [Header("拖拽设置")]
    [SerializeField] private Canvas canvas; // Canvas引用，用于拖拽时正确显示
    private GameObject dragIcon; // 拖拽时显示的临时图标
    private RectTransform dragIconRect;
    private CanvasGroup canvasGroup; // 用于控制透明度
    
    private void Awake()
    {
        // 确保有 Button 组件
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<UnityEngine.UI.Button>();
            Debug.Log($"[PackageUIItem] 自动添加 Button 组件到 {gameObject.name}");
        }
        
        // 绑定点击事件
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnSlotClicked);
        
        // 获取或添加 CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // 自动查找Canvas（如果没有手动设置）
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }
    
    private void Start()
    {
        // 初始化：确保空格子处于正确状态
        UpdateSlot(null, 0);
    }
    
    // --- 公开方法：由外部（如 InventoryManager）调用 ---

    /// <summary>
    /// 更新槽位 UI 的显示。
    /// </summary>
    public void UpdateSlot(ItemData itemData, int count)
    {
        currentItemData = itemData;
        currentItemCount = count;

        if (itemData != null && count > 0)
        {
            // 有物品：显示图标和数量
            gameObject.SetActive(true);
            
            // 1. 设置图标
            iconImage.sprite = itemData.itemIcon;
            iconImage.color = Color.white; // 确保图标可见（不透明）
            iconImage.enabled = true; // 启用图标显示

            // 2. 设置数量文本
            countText.text = count > 1 ? count.ToString() : ""; // 数量大于1时才显示
            
            // 3. 显示默认背景，不自动选中
            SetSelected(false);
        }
        else
        {
            // 槽位为空：完全隐藏格子
            
            // 如果当前格子是选中状态，取消选中
            if (currentSelectedItem == this)
            {
                currentSelectedItem = null;
            }
            
            // 清空图标和数量
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
            iconImage.enabled = false;
            countText.text = "";
            SetSelected(false);
            
            // 完全隐藏格子（包括Default背景）
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 设置槽位的选中/未选中状态。
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (defaultBg != null)
        {
            defaultBg.SetActive(!isSelected);
        }
        
        if (selectedBg != null)
        {
            selectedBg.SetActive(isSelected);
        }
    }

    /// <summary>
    /// 获取当前物品数据。
    /// </summary>
    public ItemData GetItemData()
    {
        return currentItemData;
    }
    
    /// <summary>
    /// 获取当前物品数量。
    /// </summary>
    public int GetItemCount()
    {
        return currentItemCount;
    }
    
    // --- 交互方法：由 Button 组件调用 ---
    
    /// <summary>
    /// 当玩家点击此格子时触发的方法（通过 Button 组件）
    /// </summary>
    public void OnSlotClicked()
    {
        // 如果点击的是已经选中的格子，取消选中
        if (currentSelectedItem == this)
        {
            SetSelected(false);
            currentSelectedItem = null;
            Debug.Log("[PackageUIItem] 取消选中");
            return;
        }
        
        // 取消上一个选中的格子（自动回到 Default 状态）
        if (currentSelectedItem != null)
        {
            currentSelectedItem.SetSelected(false);
            Debug.Log($"[PackageUIItem] 取消上一个选中的格子");
        }
        
        // 选中当前格子
        if (currentItemData != null)
        {
            Debug.Log($"[PackageUIItem] 点击物品: {currentItemData.itemName}，数量: {currentItemCount}");
            SetSelected(true);
            currentSelectedItem = this;
            
            // TODO: 在这里通知 InventoryManager/UIManager 该物品被点击
            // 示例：InventoryManager.Instance.SelectItem(this);
        }
        else
        {
            Debug.Log("[PackageUIItem] 点击了空格子");
            // 空格子也可以被选中（如果需要的话）
            // SetSelected(true);
            // currentSelectedItem = this;
        }
    }
    
    // --- Tooltip 相关方法 ---
    
    /// <summary>
    /// 鼠标进入格子时显示 Tooltip
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 只有当格子有物品时才显示 Tooltip
        if (currentItemData != null && currentItemCount > 0)
        {
            TooltipSystem.Show(currentItemData.description, currentItemData.itemName);
        }
    }
    
    /// <summary>
    /// 鼠标离开格子时隐藏 Tooltip
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
    
    // --- 拖拽相关方法 ---
    
    /// <summary>
    /// 开始拖拽
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 只有有物品的格子才能拖拽
        if (currentItemData == null || currentItemCount <= 0)
        {
            eventData.pointerDrag = null; // 取消拖拽
            return;
        }
        
        Debug.Log($"[PackageUIItem] 开始拖拽: {currentItemData.itemName}，数量: {currentItemCount}");
        
        // 隐藏Tooltip
        TooltipSystem.Hide();
        
        // 创建拖拽图标
        CreateDragIcon();
        
        // 降低原格子透明度
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // 允许射线穿透，这样可以检测到下方的格子
    }
    
    /// <summary>
    /// 拖拽中
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null && dragIconRect != null)
        {
            // 更新拖拽图标位置跟随鼠标
            dragIconRect.position = eventData.position;
        }
    }
    
    /// <summary>
    /// 结束拖拽
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[PackageUIItem] 结束拖拽: {currentItemData?.itemName}");
        CleanupDragIcon();
    }
    
    /// <summary>
    /// 清理拖拽图标和状态（公共方法，供外部调用）
    /// </summary>
    public void CleanupDragIcon()
    {
        // 恢复透明度
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }
        
        // 销毁拖拽图标
        if (dragIcon != null)
        {
            Destroy(dragIcon);
            dragIcon = null;
            dragIconRect = null;
            Debug.Log($"[PackageUIItem] 已清理拖拽图标");
        }
    }
    
    /// <summary>
    /// 创建拖拽图标
    /// </summary>
    private void CreateDragIcon()
    {
        if (canvas == null || currentItemData == null)
        {
            return;
        }
        
        // 创建临时GameObject
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform, false);
        dragIcon.transform.SetAsLastSibling(); // 显示在最上层
        
        // 添加Image组件并设置图标
        Image image = dragIcon.AddComponent<Image>();
        image.sprite = currentItemData.itemIcon;
        image.raycastTarget = false; // 不阻挡射线检测
        
        // 设置RectTransform
        dragIconRect = dragIcon.GetComponent<RectTransform>();
        dragIconRect.sizeDelta = new Vector2(80, 80); // 拖拽图标大小
        
        // 添加半透明效果
        CanvasGroup dragCanvasGroup = dragIcon.AddComponent<CanvasGroup>();
        dragCanvasGroup.alpha = 0.8f;
        dragCanvasGroup.blocksRaycasts = false;
    }
    
}