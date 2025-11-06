using System.Collections;
using UnityEngine;

/// <summary>
/// 礼物赠送系统 - 在战斗中赠送道具给野生宝可梦
/// </summary>
public class GiftSystem : MonoBehaviour
{
    private BattleSystem battleSystem;
    
    private void Awake()
    {
        battleSystem = GetComponent<BattleSystem>();
    }
    
    /// <summary>
    /// 赠送道具给野生宝可梦
    /// </summary>
    /// <param name="item">要赠送的道具</param>
    /// <param name="target">目标宝可梦（通常是敌方）</param>
    /// <returns>赠送流程协程</returns>
    public IEnumerator GiveGiftToPokemon(ItemData item, PokemonFromData target)
    {
        if (item == null || target == null)
        {
            Debug.LogError("[GiftSystem] 道具或目标为空！");
            yield break;
        }
        
        Debug.Log($"[GiftSystem] 赠送 {item.itemName} 给 {target.pokemonName}");
        
        // 显示赠送文本
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"你赠送了 {item.itemName} 给 {target.pokemonName}！"));
        yield return new WaitForSeconds(0.5f);
        
        // 从背包移除道具
        bool removed = ItemInventory.Instance.RemoveItemByID(item.itemID, 1);
        if (!removed)
        {
            Debug.LogWarning($"[GiftSystem] 无法从背包移除道具：{item.itemName}");
            yield return StartCoroutine(battleSystem.TypeDialog("道具不足！"));
            yield break;
        }
        
        Debug.Log($"[GiftSystem] 已从背包移除 {item.itemName}");
        
        // 根据道具类型触发不同效果
        yield return ProcessGiftEffect(item, target);
    }
    
    /// <summary>
    /// 处理礼物效果
    /// </summary>
    private IEnumerator ProcessGiftEffect(ItemData item, PokemonFromData target)
    {
        // 检查特殊道具：捕捉道具 = 捕捉宝可梦
        if (item.itemName == "Capture" || item.itemName == "捕捉道具")
        {
            yield return ProcessCapture(item, target);
        }
        // 检查特殊道具：绳子 + M猪 = 形态变化
        else if ((item.itemName == "绳子" || item.itemName == "Rope") && 
            (target.data.displayName.Contains("M猪") || target.data.displayName.Contains("MPig")))
        {
            yield return ProcessRopeForMPig(item, target);
        }
        // 检查特殊道具：鞭子 + S蛇 = 解锁鞭打技能
        else if ((item.itemName == "鞭子" || item.itemName == "Whip") && 
                 (target.data.displayName.Contains("S蛇") || target.data.displayName.Contains("SSnake")))
        {
            yield return ProcessWhipForSSnake(item, target);
        }
        // 检查其他治疗道具
        else if (item.IsTool())
        {
            yield return ProcessHealingItem(item, target);
        }
        // 通用礼物效果（提升好感度等）
        else
        {
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{target.pokemonName} 接受了礼物！"));
            // TODO: 这里可以添加好感度系统
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// 处理捕捉道具（好感度满100才能捕捉）
    /// </summary>
    private IEnumerator ProcessCapture(ItemData item, PokemonFromData target)
    {
        Debug.Log($"[GiftSystem] 尝试捕捉 {target.pokemonName}，当前好感度：{target.GetAffinity()}");
        
        // 检查好感度是否达到100
        if (target.GetAffinity() < 100)
        {
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{target.pokemonName} 拒绝了！好感度不足！"));
            yield return new WaitForSeconds(1f);
            yield break;
        }
        
        // 好感度满100，捕捉成功
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"使用了捕捉道具..."));
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{target.pokemonName} 愿意跟你走了！"));
        yield return new WaitForSeconds(1f);
        
        // 触发捕捉成功（会自动加入队伍）
        battleSystem.OnCaptureSuccess();
        
        Debug.Log($"[GiftSystem] 成功捕捉 {target.pokemonName}！");
    }
    
    /// <summary>
    /// 处理绳子对M猪的特殊效果
    /// </summary>
    private IEnumerator ProcessRopeForMPig(ItemData item, PokemonFromData mpig)
    {
        PokemonTransformation transformation = mpig.GetComponent<PokemonTransformation>();
        
        if (transformation == null)
        {
            Debug.LogWarning($"[GiftSystem] {mpig.pokemonName} 没有 PokemonTransformation 组件！");
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{mpig.pokemonName} 对绳子没有反应..."));
            yield break;
        }
        
        if (transformation.IsTransformed())
        {
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{mpig.pokemonName} 已经被捆绑了！"));
            yield break;
        }
        
        // 触发形态变化
        transformation.Transform(permanent: true);
        
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{mpig.pokemonName} 被绳子捆绑了！"));
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{mpig.pokemonName} 解锁了新技能！"));
        yield return new WaitForSeconds(0.5f);
        
        // 增加好感度：赠送绳子给M猪 +30
        mpig.AddAffinity(30);
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{mpig.pokemonName} 的好感度上升了！"));
        yield return new WaitForSeconds(0.5f);
        
        // 绿帽鱼获得一半好感度
        AffinityManager.Instance.AddGreenHatFishAffinity(15);
        
        // 刷新战斗UI（如果需要）
        // battleSystem.RefreshBattleUI();
    }
    
    /// <summary>
    /// 处理鞭子对S蛇的特殊效果
    /// </summary>
    private IEnumerator ProcessWhipForSSnake(ItemData item, PokemonFromData ssnake)
    {
        PokemonTransformation transformation = ssnake.GetComponent<PokemonTransformation>();
        
        if (transformation == null)
        {
            Debug.LogWarning($"[GiftSystem] {ssnake.pokemonName} 没有 PokemonTransformation 组件！");
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{ssnake.pokemonName} 对鞭子没有反应..."));
            yield break;
        }
        
        if (transformation.IsTransformed())
        {
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{ssnake.pokemonName} 已经拥有鞭子了！"));
            yield break;
        }
        
        // 触发形态变化（解锁鞭打技能）
        transformation.Transform(permanent: true);
        
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{ssnake.pokemonName} 获得了鞭子！"));
        yield return new WaitForSeconds(0.5f);
        
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{ssnake.pokemonName} 解锁了鞭打技能！"));
        yield return new WaitForSeconds(0.5f);
        
        // 增加好感度：赠送鞭子给S蛇 +30
        ssnake.AddAffinity(30);
        yield return StartCoroutine(battleSystem.TypeDialog(
            $"{ssnake.pokemonName} 的好感度上升了！"));
        yield return new WaitForSeconds(0.5f);
        
        // 绿帽鱼获得一半好感度
        AffinityManager.Instance.AddGreenHatFishAffinity(15);
    }
    
    /// <summary>
    /// 处理治疗道具（赠送给野生宝可梦恢复HP）
    /// </summary>
    private IEnumerator ProcessHealingItem(ItemData item, PokemonFromData target)
    {
        int healValue = item.healAmount;
        
        if (healValue > 0)
        {
            // 播放治疗动画
            BattleAnimationManager animManager = battleSystem.GetComponent<BattleAnimationManager>();
            if (animManager != null)
            {
                yield return StartCoroutine(animManager.PlaySkillAnimation(SkillType.Heal, target));
            }
            
            int actualHeal = target.Heal(healValue);
            
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{target.pokemonName} 恢复了 {actualHeal} HP！"));
            yield return new WaitForSeconds(1f);
            
            // TODO: 更新敌方HUD显示
            // battleSystem.enemyHUD.SetHP(target.currentHP);
        }
        else
        {
            yield return StartCoroutine(battleSystem.TypeDialog(
                $"{target.pokemonName} 接受了 {item.itemName}！"));
            yield return new WaitForSeconds(1f);
        }
    }
}

