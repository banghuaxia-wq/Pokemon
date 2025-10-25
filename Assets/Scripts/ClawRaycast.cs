using UnityEngine;

/// <summary>
/// 控制抓钩射线检测和激活白色描边
/// </summary>
public class ClawRaycast : MonoBehaviour
{
    [Header("射线设置")]
    [Tooltip("射线检测距离")]
    public float raycastDistance = 10f;
    
    [Tooltip("射线检测层级")]
    public LayerMask targetLayer;
    
    [Tooltip("是否显示射线")]
    public bool showDebugRay = true;
    
    [Tooltip("射线颜色")]
    public Color rayColor = Color.yellow;

    [Header("白边设置")]
    [Tooltip("白边物体的后缀名称")]
    public string outlineSuffix = "_outline";

    private GameObject currentTarget = null;
    private GameObject currentOutline = null;

    private void Update()
    {
        DetectBelow();
    }

    /// <summary>
    /// 向下检测物体
    /// </summary>
    private void DetectBelow()
    {
        // 从抓钩位置向下发射射线
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, targetLayer);

        // 显示调试射线
        if (showDebugRay)
        {
            if (hit.collider != null)
            {
                Debug.DrawLine(transform.position, hit.point, Color.green);
            }
            else
            {
                Debug.DrawRay(transform.position, Vector2.down * raycastDistance, rayColor);
            }
        }

        // 如果检测到物体
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // 如果是新的物体
            if (hitObject != currentTarget)
            {
                // 先禁用之前的白边
                DeactivateCurrentOutline();
                
                // 设置新的目标
                currentTarget = hitObject;
                
                // 激活新的白边
                ActivateOutline(hitObject);
            }
        }
        else
        {
            // 没有检测到物体，禁用当前白边
            DeactivateCurrentOutline();
        }
    }

    /// <summary>
    /// 激活物体的白色描边
    /// </summary>
    private void ActivateOutline(GameObject target)
    {
        // 查找白边子物体
        Transform outlineTransform = target.transform.Find(target.name + outlineSuffix);
        
        if (outlineTransform != null)
        {
            currentOutline = outlineTransform.gameObject;
            currentOutline.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"未找到白边物体: {target.name}{outlineSuffix}");
        }
    }

    /// <summary>
    /// 禁用当前的白色描边
    /// </summary>
    private void DeactivateCurrentOutline()
    {
        if (currentOutline != null)
        {
            currentOutline.SetActive(false);
            currentOutline = null;
        }
        
        currentTarget = null;
    }

    /// <summary>
    /// 获取当前检测到的目标
    /// </summary>
    public GameObject GetCurrentTarget()
    {
        return currentTarget;
    }

    private void OnDisable()
    {
        // 禁用时清理白边
        DeactivateCurrentOutline();
    }
}

