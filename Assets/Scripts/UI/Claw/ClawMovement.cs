using UnityEngine;

/// <summary>
/// 控制抓钩左右移动
/// </summary>
public class ClawMovement : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("左右移动速度")]
    public float moveSpeed = 5f;
    
    [Tooltip("最小X坐标限制")]
    public float minX = -8f;
    
    [Tooltip("最大X坐标限制")]
    public float maxX = 8f;

    [Header("移动控制")]
    [Tooltip("是否允许移动")]
    public bool canMove = true;

    private void Update()
    {
        if (!canMove) return;

        // 获取输入
        float horizontalInput = 0f;
        
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        // 计算新位置
        if (horizontalInput != 0f)
        {
            Vector3 newPosition = transform.position;
            newPosition.x += horizontalInput * moveSpeed * Time.deltaTime;
            
            // 限制移动范围
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// 设置是否可以移动
    /// </summary>
    public void SetCanMove(bool value)
    {
        canMove = value;
    }
}

