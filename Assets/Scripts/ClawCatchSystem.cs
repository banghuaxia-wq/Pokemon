using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Claw 抓取系统 - 处理抓取宝可梦和进入战斗场景
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class ClawCatchSystem : MonoBehaviour
{
    [Header("抓取设置")]
    [Tooltip("是否启用调试日志")]
    public bool showDebugInfo = true;
    
    [Tooltip("Claw_scratch 动画的持续时间（秒）")]
    public float scratchAnimationDuration = 1.0f;
    
    private Animator animator;
    private PokemonData caughtPokemonData; // 被抓取的宝可梦数据
    private bool isProcessingCatch = false; // 是否正在处理抓取流程
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果已经在处理抓取流程，忽略新的碰撞
        if (isProcessingCatch)
        {
            return;
        }
        
        // 检查碰撞对象是否在 Pokemon 层
        if (collision.gameObject.layer != LayerMask.NameToLayer("Pokemon"))
        {
            return;
        }
        
        DebugLog($"[ClawCatchSystem] 碰撞到 Pokemon 层物体: {collision.gameObject.name}");
        
        // 尝试获取 PokemonFromData 组件
        PokemonFromData pokemonComponent = collision.GetComponent<PokemonFromData>();
        if (pokemonComponent != null && pokemonComponent.data != null)
        {
            DebugLog($"[ClawCatchSystem] 检测到宝可梦: {pokemonComponent.data.displayName}");
            
            // 触发抓取动画
            TriggerCatchAnimation(pokemonComponent.data);
        }
        else
        {
            DebugLog($"[ClawCatchSystem] 物体 {collision.gameObject.name} 没有有效的 PokemonFromData 组件");
        }
    }
    
    /// <summary>
    /// 触发抓取动画
    /// </summary>
    private void TriggerCatchAnimation(PokemonData pokemonData)
    {
        if (animator == null)
        {
            Debug.LogError("[ClawCatchSystem] Animator 组件未找到！");
            return;
        }
        
        caughtPokemonData = pokemonData;
        isProcessingCatch = true;
        
        // 设置 Touch 参数为 true，触发 Claw_normal -> Claw_touch -> Claw_scratch
        animator.SetBool("Touch", true);
        DebugLog($"[ClawCatchSystem] 触发抓取动画 - 宝可梦: {pokemonData.displayName}");
        
        // 等待 Claw_scratch 动画播放完毕后进入战斗
        StartCoroutine(WaitForScratchAndEnterBattle());
    }
    
    /// <summary>
    /// 等待 Claw_scratch 动画播放完毕后进入战斗
    /// </summary>
    private IEnumerator WaitForScratchAndEnterBattle()
    {
        // 等待动画切换到 Claw_scratch
        yield return new WaitUntil(() => IsPlayingAnimation("Claw_scratch"));
        
        DebugLog("[ClawCatchSystem] Claw_scratch 动画开始播放");
        
        // 等待 Claw_scratch 动画播放完毕
        yield return new WaitForSeconds(scratchAnimationDuration);
        
        DebugLog("[ClawCatchSystem] Claw_scratch 动画播放完毕");
        
        // 重置 Touch 参数
        animator.SetBool("Touch", false);
        
        // 进入战斗场景
        EnterBattle();
    }
    
    /// <summary>
    /// 检查当前是否在播放指定动画
    /// </summary>
    private bool IsPlayingAnimation(string animationName)
    {
        if (animator == null)
            return false;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(animationName);
    }
    
    /// <summary>
    /// 进入战斗场景
    /// </summary>
    private void EnterBattle()
    {
        if (caughtPokemonData == null)
        {
            Debug.LogError("[ClawCatchSystem] 抓取的宝可梦数据为空！");
            isProcessingCatch = false;
            return;
        }
        
        // 检查玩家背包是否有宝可梦
        if (PlayerInventory.Instance == null || PlayerInventory.Instance.pokemonParty.Count == 0)
        {
            Debug.LogError("[ClawCatchSystem] 玩家背包为空，无法进入战斗！");
            isProcessingCatch = false;
            return;
        }
        
        // 获取玩家的第一只宝可梦（或当前选中的宝可梦）
        PlayerPokemonData playerPokemon = PlayerInventory.Instance.pokemonParty[0];
        
        // 判断是否是 Boss 战（Boss 对象名称包含 "Boss"）
        bool isBossBattle = caughtPokemonData.displayName.Contains("Boss") || 
                            caughtPokemonData.displayName.Contains("boss") ||
                            caughtPokemonData.name.Contains("Boss");
        
        if (isBossBattle)
        {
            DebugLog($"[ClawCatchSystem] 检测到 Boss 战 - {caughtPokemonData.displayName}");
            
            // TODO: 多人战斗暂未搭建，显示提示
            Debug.LogWarning("[ClawCatchSystem] Boss 战斗场景暂未搭建！");
            
            // 暂时不进入战斗
            // BattleInitializer.SetupMultiBattle(playerPokemon, caughtPokemonData);
            // SceneManager.LoadScene("MultiBattleScene");
            
            isProcessingCatch = false;
            return;
        }
        else
        {
            DebugLog($"[ClawCatchSystem] 进入单人战斗 - 玩家: {playerPokemon.pokemonData.displayName}, 敌方: {caughtPokemonData.displayName}");
            
            // 设置单人战斗数据
            BattleInitializer.SetupSingleBattle(playerPokemon, caughtPokemonData);
            
            // 加载战斗场景
            SceneManager.LoadScene("SingleBattleScene");
        }
    }
    
    /// <summary>
    /// 调试日志输出
    /// </summary>
    private void DebugLog(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log(message);
        }
    }
}

