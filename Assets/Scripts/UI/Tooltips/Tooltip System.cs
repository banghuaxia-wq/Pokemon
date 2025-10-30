using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;
    public Tooltip tooltip;
    
    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        // 初始状态隐藏tooltip
        Hide();
    }
    
    private void OnDestroy()
    {
        // 清空静态引用，避免访问已销毁的对象
        if (current == this)
        {
            current = null;
        }
    }

    public static void Show(string content, string header = "")
    {
        // 检查TooltipSystem和tooltip是否存在且未被销毁
        if (current == null || current.tooltip == null)
        {
            return;
        }
        
        current.tooltip.SetText(content, header);
        current.tooltip.gameObject.SetActive(true);
    }
    
    public static void Hide()
    {
        // 检查TooltipSystem和tooltip是否存在且未被销毁
        if (current == null || current.tooltip == null)
        {
            return;
        }
        
        current.tooltip.gameObject.SetActive(false);
    }
}
