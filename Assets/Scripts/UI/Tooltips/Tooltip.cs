using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;

    public LayoutElement layoutElement;

    public int characterWrapLimit;

    [Header("位置设置")]
    public float padding = 8f; // 与屏幕边缘的留白

    private RectTransform rectTransform;
    private Canvas rootCanvas;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void SetText(string content, string header = "")
    {
        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
        }
        else
        {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        contentField.text = content;
        
        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;

    }
    private void Update()
    {
        if (Application.isEditor)
        {
            int headerLength = headerField.text.Length;
            int contentLength = contentField.text.Length;
            layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;
        }

        if (rootCanvas == null)
        {
            // 兜底：没有找到Canvas时按屏幕像素定位并做边界裁切
            Vector2 pos = Input.mousePosition;
            Vector2 size = rectTransform != null ? rectTransform.rect.size : new Vector2(200, 100);
            Vector2 pivot = rectTransform != null ? rectTransform.pivot : new Vector2(0.5f, 0.5f);
            pos.x = Mathf.Clamp(pos.x, padding + size.x * pivot.x, Screen.width - padding - size.x * (1f - pivot.x));
            pos.y = Mathf.Clamp(pos.y, padding + size.y * pivot.y, Screen.height - padding - size.y * (1f - pivot.y));
            transform.position = pos;
            return;
        }

        // 根据Canvas渲染模式分别处理定位与边界裁切
        if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 pos = Input.mousePosition;
            Vector2 size = rectTransform.rect.size;
            Vector2 pivot = rectTransform.pivot;
            pos.x = Mathf.Clamp(pos.x, padding + size.x * pivot.x, Screen.width - padding - size.x * (1f - pivot.x));
            pos.y = Mathf.Clamp(pos.y, padding + size.y * pivot.y, Screen.height - padding - size.y * (1f - pivot.y));
            transform.position = pos;
        }
        else
        {
            // ScreenSpace-Camera 或 WorldSpace：使用锚点坐标并在Canvas矩形内裁切
            RectTransform canvasRect = rootCanvas.transform as RectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? rootCanvas.worldCamera : null, out localPoint);

            Vector2 size = rectTransform.rect.size;
            Vector2 pivot = rectTransform.pivot;
            // 计算在Canvas本地坐标下的边界
            float minX = -canvasRect.rect.width * 0.5f + padding + size.x * pivot.x;
            float maxX =  canvasRect.rect.width * 0.5f - padding - size.x * (1f - pivot.x);
            float minY = -canvasRect.rect.height * 0.5f + padding + size.y * pivot.y;
            float maxY =  canvasRect.rect.height * 0.5f - padding - size.y * (1f - pivot.y);

            localPoint.x = Mathf.Clamp(localPoint.x, minX, maxX);
            localPoint.y = Mathf.Clamp(localPoint.y, minY, maxY);

            rectTransform.anchoredPosition = localPoint;
        }
    }

    
}
