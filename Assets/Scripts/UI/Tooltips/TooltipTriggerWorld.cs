using UnityEngine;

/// <summary>
/// 用于场景中物体（2D/3D GameObject）的Tooltip触发器
/// 需要物体有Collider组件
/// </summary>
public class TooltipTriggerWorld : MonoBehaviour
{
    [Header("提示信息")]
    public string header;
    [TextArea]
    public string content;

    [Header("设置")]
    [Tooltip("是否启用")]
    public bool isEnabled = true;

    /// <summary>
    /// 鼠标进入物体时触发
    /// </summary>
    private void OnMouseEnter()
    {
        if (isEnabled)
        {
            TooltipSystem.Show(content, header);
        }
    }

    /// <summary>
    /// 鼠标离开物体时触发
    /// </summary>
    private void OnMouseExit()
    {
        if (isEnabled)
        {
            TooltipSystem.Hide();
        }
    }

    /// <summary>
    /// 禁用时隐藏tooltip
    /// </summary>
    private void OnDisable()
    {
        // 只在游戏运行时隐藏tooltip，避免场景销毁时的错误
        // TooltipSystem.Hide() 内部已经有空值检查，所以即使tooltip被销毁也不会报错
        if (Application.isPlaying)
        {
            TooltipSystem.Hide();
        }
    }
}

