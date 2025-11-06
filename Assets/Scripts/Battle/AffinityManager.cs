using UnityEngine;

/// <summary>
/// 全局好感度管理器 - 管理所有宝可梦的好感度，特别是绿帽鱼的持久化好感度
/// </summary>
public class AffinityManager : MonoBehaviour
{
    private static AffinityManager _instance;
    public static AffinityManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("AffinityManager");
                _instance = go.AddComponent<AffinityManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("绿帽鱼好感度")]
    [Tooltip("绿帽鱼的全局好感度（0-100）")]
    [SerializeField] private int greenHatFishAffinity = 0;

    [Tooltip("绿帽鱼是否已被捕捉")]
    [SerializeField] private bool greenHatFishCaptured = false;

    private const int MAX_AFFINITY = 100;
    private const int MIN_AFFINITY = 0;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 获取绿帽鱼的好感度
    /// </summary>
    public int GetGreenHatFishAffinity()
    {
        return greenHatFishAffinity;
    }

    /// <summary>
    /// 增加绿帽鱼的好感度
    /// </summary>
    public void AddGreenHatFishAffinity(int amount)
    {
        int oldAffinity = greenHatFishAffinity;
        greenHatFishAffinity = Mathf.Clamp(greenHatFishAffinity + amount, MIN_AFFINITY, MAX_AFFINITY);
        Debug.Log($"[AffinityManager] 绿帽鱼好感度: {oldAffinity} → {greenHatFishAffinity} (+{amount})");
    }

    /// <summary>
    /// 设置绿帽鱼的好感度
    /// </summary>
    public void SetGreenHatFishAffinity(int value)
    {
        greenHatFishAffinity = Mathf.Clamp(value, MIN_AFFINITY, MAX_AFFINITY);
        Debug.Log($"[AffinityManager] 绿帽鱼好感度设置为: {greenHatFishAffinity}");
    }

    /// <summary>
    /// 检查绿帽鱼是否已被捕捉
    /// </summary>
    public bool IsGreenHatFishCaptured()
    {
        return greenHatFishCaptured;
    }

    /// <summary>
    /// 设置绿帽鱼已被捕捉
    /// </summary>
    public void SetGreenHatFishCaptured(bool captured)
    {
        greenHatFishCaptured = captured;
        Debug.Log($"[AffinityManager] 绿帽鱼捕捉状态: {captured}");
    }

    /// <summary>
    /// 检查绿帽鱼是否会逃跑（好感度未满100）
    /// </summary>
    public bool WillGreenHatFishEscape()
    {
        return greenHatFishAffinity < MAX_AFFINITY;
    }
}

