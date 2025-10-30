using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 用于UI元素的Tooltip触发器
/// </summary>
public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("提示信息")]
    public string header;
    [TextArea]
    public string content;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }

    private void OnDisable()
    {
        // 禁用时隐藏tooltip
        if (Application.isPlaying)
        {
            TooltipSystem.Hide();
        }
    }
}
