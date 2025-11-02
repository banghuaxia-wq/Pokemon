using UnityEngine;
using System.Collections;

/// <summary>
/// 控制抓钩向下移动和抓取物体
/// </summary>
public class ClawGrab : MonoBehaviour
{
    [Header("组件引用")]
    [Tooltip("Animator组件")]
    public Animator animator;
    
    [Tooltip("移动脚本引用")]
    public ClawMovement clawMovement;

    [Header("下降设置")]
    [Tooltip("下降速度")]
    public float descendSpeed = 5f;
    
    [Tooltip("最大下降距离")]
    public float maxDescendDistance = 10f;
    
    [Tooltip("上升速度")]
    public float ascendSpeed = 8f;

    [Header("抓取设置")]
    [Tooltip("Claw_scratch动画名称")]
    public string scratchAnimationName = "Claw_scratch";
    
    [Tooltip("可抓取物体的层级")]
    public LayerMask grabLayer;

    [Header("碰撞检测")]
    [Tooltip("是否使用碰撞体检测")]
    public bool useCollisionDetection = true;

    // 状态变量
    private bool isDescending = false;
    private bool isAscending = false;
    private bool hasGrabbed = false;
    private bool isPlayingGrabAnimation = false;
    private Vector3 originalPosition;
    private GameObject grabbedObject = null;

    // Animator参数名称
    private const string TOUCH_PARAM = "Touch";

    private void Start()
    {
        // 自动获取组件
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (clawMovement == null)
        {
            clawMovement = GetComponent<ClawMovement>();
        }

        // 记录初始位置
        originalPosition = transform.position;
        
        // 检查并配置Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 配置Rigidbody2D为Kinematic（避免受重力影响）
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        // 下降过程
        if (isDescending && !isPlayingGrabAnimation)
        {
            DescendClaw();
        }
        // 上升过程
        else if (isAscending && !isPlayingGrabAnimation)
        {
            AscendClaw();
        }
    }

    /// <summary>
    /// 开始抓取（由按钮调用）
    /// </summary>
    public void StartGrab()
    {
        // 如果正在执行动作，不响应
        if (isDescending || isAscending || isPlayingGrabAnimation)
        {
            return;
        }

        // 记录当前位置作为原始位置
        originalPosition = transform.position;
        
        // 开始下降
        isDescending = true;
        hasGrabbed = false;
        isPlayingGrabAnimation = false;
        
        // 禁止左右移动
        if (clawMovement != null)
        {
            clawMovement.SetCanMove(false);
        }

        // 重置Touch参数
        if (animator != null)
        {
            animator.SetBool(TOUCH_PARAM, false);
        }
    }

    /// <summary>
    /// 下降过程
    /// </summary>
    private void DescendClaw()
    {
        // 向下移动
        transform.position += Vector3.down * descendSpeed * Time.deltaTime;
        
        // 检查是否超过最大下降距离
        float descendedDistance = originalPosition.y - transform.position.y;
        if (descendedDistance >= maxDescendDistance)
        {
            // 到达最大距离，开始上升
            StartAscend();
        }
    }

    /// <summary>
    /// 上升过程
    /// </summary>
    private void AscendClaw()
    {
        // 向上移动
        transform.position += Vector3.up * ascendSpeed * Time.deltaTime;
        
        // 检查是否回到原始位置
        if (transform.position.y >= originalPosition.y)
        {
            // 回到原始位置
            transform.position = originalPosition;
            
            // 结束上升
            isAscending = false;
            
            // 恢复左右移动
            if (clawMovement != null)
            {
                clawMovement.SetCanMove(true);
            }

            // 重置Touch参数
            if (animator != null)
            {
                animator.SetBool(TOUCH_PARAM, false);
            }

            // 如果抓到了物体，可以在这里处理
            if (hasGrabbed && grabbedObject != null)
            {
                OnGrabSuccess(grabbedObject);
            }
            
            hasGrabbed = false;
            grabbedObject = null;
        }
    }

    /// <summary>
    /// 开始上升
    /// </summary>
    private void StartAscend()
    {
        isDescending = false;
        isAscending = true;
    }

    /// <summary>
    /// 碰撞检测（2D）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!useCollisionDetection)
        {
            return;
        }
        
        if (!isDescending)
        {
            return;
        }
        
        if (hasGrabbed)
        {
            return;
        }

        // 检查是否在可抓取层级
        int layerMask = 1 << collision.gameObject.layer;
        bool isInGrabLayer = (grabLayer.value & layerMask) != 0;
        
        if (grabLayer != 0 && !isInGrabLayer)
        {
            return;
        }

        // 检测到可抓取物体
        OnHitObject(collision.gameObject);
    }

    /// <summary>
    /// 碰撞检测（3D）- 如果使用3D碰撞体
    /// </summary>
    private void OnTriggerEnter(Collider collision)
    {
        if (!useCollisionDetection || !isDescending || hasGrabbed)
        {
            return;
        }

        // 检测到物体
        OnHitObject(collision.gameObject);
    }

    /// <summary>
    /// 击中物体时的处理
    /// </summary>
    private void OnHitObject(GameObject hitObject)
    {
        hasGrabbed = true;
        grabbedObject = hitObject;
        isPlayingGrabAnimation = true;
        
        // 停止下降
        isDescending = false;
        
        // 触发Touch动画
        if (animator != null)
        {
            animator.SetBool(TOUCH_PARAM, true);
        }

        // 等待Claw_scratch动画播放完成后再上升
        StartCoroutine(WaitForScratchAnimation());
    }
    
    /// <summary>
    /// 等待Claw_scratch动画播放完成
    /// </summary>
    private IEnumerator WaitForScratchAnimation()
    {
        // 等待一帧，确保动画开始播放
        yield return null;
        
        if (animator == null)
        {
            isPlayingGrabAnimation = false;
            StartAscend();
            yield break;
        }

        // 等待进入Claw_scratch动画
        float waitTime = 0f;
        float maxWaitTime = 2f; // 最多等待2秒
        
        while (waitTime < maxWaitTime)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 检查是否进入了Claw_scratch动画
            if (stateInfo.IsName(scratchAnimationName))
            {
                break;
            }
            
            waitTime += Time.deltaTime;
            yield return null;
        }
        
        // 等待Claw_scratch动画播放完成
        while (true)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            
            // 如果还在播放Claw_scratch动画
            if (stateInfo.IsName(scratchAnimationName))
            {
                // 检查动画是否接近结束（播放进度 >= 0.95）
                if (stateInfo.normalizedTime >= 0.95f)
                {
                    break;
                }
            }
            else
            {
                // 已经切换到其他动画了，也结束等待
                break;
            }
            
            yield return null;
        }
        
        // Claw_scratch动画播放完成，开始上升
        isPlayingGrabAnimation = false;
        StartAscend();
    }

    /// <summary>
    /// 抓取成功后的处理
    /// </summary>
    private void OnGrabSuccess(GameObject obj)
    {
        // 这里可以添加抓取成功后的逻辑，比如增加分数、触发事件等
    }

    /// <summary>
    /// 取消当前抓取动作
    /// </summary>
    public void CancelGrab()
    {
        if (isDescending)
        {
            StartAscend();
        }
    }

    /// <summary>
    /// 获取是否正在执行抓取
    /// </summary>
    public bool IsGrabbing()
    {
        return isDescending || isAscending;
    }
}
