using UnityEngine;

/// <summary>
/// 持久化管理器 - 确保所有单例系统跨场景保留
/// 挂载在 PermanentManager GameObject 上
/// </summary>
public class PermanentManager : MonoBehaviour
{
    private static PermanentManager instance;
    
    private void Awake()
    {
        // 单例模式
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 保留整个 PermanentManager 及其所有子物体
            Debug.Log("[PermanentManager] 已设置为跨场景持久化");
        }
        else
        {
            // 如果场景中已经有一个 PermanentManager，销毁新的
            Debug.LogWarning("[PermanentManager] 场景中已存在 PermanentManager，销毁重复实例");
            Destroy(gameObject);
        }
    }
}

