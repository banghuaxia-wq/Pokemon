using System.Collections;
using UnityEngine;

/// <summary>
/// 战斗动画管理器 - 负责播放技能动画特效
/// </summary>
public class BattleAnimationManager : MonoBehaviour
{
    [Header("动画配置")]
    [Tooltip("技能动画配置数据")]
    public SkillAnimationConfig animationConfig;
    
    /// <summary>
    /// 播放技能动画
    /// </summary>
    /// <param name="skillType">技能类型</param>
    /// <param name="attacker">攻击者宝可梦</param>
    /// <param name="target">目标宝可梦（可选）</param>
    /// <returns>动画播放协程</returns>
    public IEnumerator PlaySkillAnimation(SkillType skillType, PokemonFromData attacker, PokemonFromData target = null)
    {
        if (animationConfig == null)
        {
            Debug.LogWarning("[BattleAnimationManager] 未设置动画配置！");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
        
        switch (skillType)
        {
            case SkillType.TackleAttack:
                yield return PlayTackleAnimation(attacker);
                break;
            
            case SkillType.WhipAttack:
                yield return PlayWhipAttackAnimation(attacker);
                break;
            
            case SkillType.DefenseStance:
                yield return PlayDefenseStanceAnimation(attacker);
                break;
            
            case SkillType.Heal:
                yield return PlayHealAnimation(attacker);
                break;
            
            case SkillType.Taunt:
                yield return PlayTauntAnimation(attacker);
                break;
            
            default:
                Debug.LogWarning($"[BattleAnimationManager] 未定义的技能类型: {skillType}");
                yield return new WaitForSeconds(0.5f);
                break;
        }
    }
    
    /// <summary>
    /// 撞击攻击动画（宝可梦抖动）
    /// </summary>
    private IEnumerator PlayTackleAnimation(PokemonFromData attacker)
    {
        if (attacker == null)
        {
            Debug.LogWarning("[BattleAnimationManager] 攻击者为空！");
            yield break;
        }
        
        Vector3 originalPosition = attacker.transform.position;
        float elapsed = 0f;
        float duration = animationConfig.tackleDuration;
        float shakeAmount = animationConfig.tackleShakeAmount;
        
        Debug.Log($"[BattleAnimationManager] 播放撞击动画：{attacker.pokemonName}");
        
        // 抖动循环
        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-shakeAmount, shakeAmount);
            float y = originalPosition.y + Random.Range(-shakeAmount, shakeAmount);
            attacker.transform.position = new Vector3(x, y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 恢复原位置
        attacker.transform.position = originalPosition;
    }
    
    /// <summary>
    /// 鞭打攻击动画（生成鞭子特效）
    /// </summary>
    private IEnumerator PlayWhipAttackAnimation(PokemonFromData attacker)
    {
        if (animationConfig.whipAttackPrefab == null)
        {
            Debug.LogWarning("[BattleAnimationManager] 未设置鞭打特效 Prefab！");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
        
        // 获取生成点
        Transform spawnPoint = attacker.GetAnimationSpawnPoint(SkillType.WhipAttack);
        
        // 实例化鞭子特效
        GameObject whipEffect = Instantiate(animationConfig.whipAttackPrefab, spawnPoint.position, Quaternion.identity);
        
        // 设置朝向（根据攻击者的朝向）
        SetEffectFacing(whipEffect, attacker.transform);
        
        Debug.Log($"[BattleAnimationManager] 播放鞭打动画：{attacker.pokemonName}，位置：{spawnPoint.position}");
        
        // 等待特效播放完毕
        yield return new WaitForSeconds(animationConfig.whipEffectDuration);
        
        // 销毁特效
        Destroy(whipEffect);
    }
    
    /// <summary>
    /// 防御姿态动画（生成护盾特效）
    /// </summary>
    private IEnumerator PlayDefenseStanceAnimation(PokemonFromData defender)
    {
        if (animationConfig.shieldPrefab == null)
        {
            Debug.LogWarning("[BattleAnimationManager] 未设置护盾特效 Prefab！");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
        
        // 获取生成点
        Transform spawnPoint = defender.GetAnimationSpawnPoint(SkillType.DefenseStance);
        
        // 实例化护盾特效
        GameObject shieldEffect = Instantiate(animationConfig.shieldPrefab, spawnPoint.position, Quaternion.identity);
        
        // 设置朝向
        SetEffectFacing(shieldEffect, defender.transform);
        
        Debug.Log($"[BattleAnimationManager] 播放防御动画：{defender.pokemonName}，位置：{spawnPoint.position}");
        
        // 等待特效播放完毕
        yield return new WaitForSeconds(animationConfig.shieldEffectDuration);
        
        // 销毁特效
        Destroy(shieldEffect);
    }
    
    /// <summary>
    /// 治疗动画（生成治疗特效）
    /// </summary>
    private IEnumerator PlayHealAnimation(PokemonFromData healer)
    {
        if (animationConfig.healPrefab == null)
        {
            Debug.LogWarning("[BattleAnimationManager] 未设置治疗特效 Prefab！");
            yield return new WaitForSeconds(0.5f);
            yield break;
        }
        
        // 获取生成点
        Transform spawnPoint = healer.GetAnimationSpawnPoint(SkillType.Heal);
        
        // 实例化治疗特效
        GameObject healEffect = Instantiate(animationConfig.healPrefab, spawnPoint.position, Quaternion.identity);
        
        // 设置朝向
        SetEffectFacing(healEffect, healer.transform);
        
        Debug.Log($"[BattleAnimationManager] 播放治疗动画：{healer.pokemonName}，位置：{spawnPoint.position}");
        
        // 等待特效播放完毕
        yield return new WaitForSeconds(animationConfig.healEffectDuration);
        
        // 销毁特效
        Destroy(healEffect);
    }
    
    /// <summary>
    /// 嘲讽动画（宝可梦跳动）
    /// </summary>
    private IEnumerator PlayTauntAnimation(PokemonFromData taunter)
    {
        if (taunter == null)
        {
            Debug.LogWarning("[BattleAnimationManager] 嘲讽者为空！");
            yield break;
        }
        
        Vector3 originalPosition = taunter.transform.position;
        float duration = 0.8f;
        float elapsed = 0f;
        
        Debug.Log($"[BattleAnimationManager] 播放嘲讽动画：{taunter.pokemonName}");
        
        // 跳动循环（3次）
        for (int i = 0; i < 3; i++)
        {
            // 向上跳
            float jumpHeight = 0.3f;
            float jumpDuration = 0.2f;
            elapsed = 0f;
            
            while (elapsed < jumpDuration)
            {
                float progress = elapsed / jumpDuration;
                float yOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                taunter.transform.position = originalPosition + Vector3.up * yOffset;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // 恢复原位置
        taunter.transform.position = originalPosition;
    }
    
    /// <summary>
    /// 设置特效朝向（根据特效生成位置判断）
    /// </summary>
    /// <param name="effect">特效 GameObject</param>
    /// <param name="pokemonTransform">宝可梦 Transform</param>
    private void SetEffectFacing(GameObject effect, Transform pokemonTransform)
    {
        if (effect == null || pokemonTransform == null) return;
        
        Vector3 effectScale = effect.transform.localScale;
        
        // 根据特效相对于宝可梦的位置判断朝向
        // 如果特效在宝可梦右边，说明应该朝右攻击
        // 如果特效在宝可梦左边，说明应该朝左攻击
        float relativeX = effect.transform.position.x - pokemonTransform.position.x;
        
        // ⚠️ 特效 Prefab 默认朝右，所以需要互换正负号
        if (relativeX > 0.01f)
        {
            // 特效在宝可梦右边 → 应该朝右 → 保持原样
            effectScale.x = Mathf.Abs(effectScale.x);
            Debug.Log($"[BattleAnimationManager] 特效在右边，朝右攻击，Scale.x = {effectScale.x}");
        }
        else if (relativeX < -0.01f)
        {
            // 特效在宝可梦左边 → 应该朝左 → 翻转
            effectScale.x = Mathf.Abs(effectScale.x) * -1f;
            Debug.Log($"[BattleAnimationManager] 特效在左边，朝左攻击，Scale.x = {effectScale.x}");
        }
        else
        {
            // 特效在宝可梦中间（防御、治疗等）→ 保持原样
            effectScale.x = Mathf.Abs(effectScale.x);
            Debug.Log($"[BattleAnimationManager] 特效在中间，保持朝向，Scale.x = {effectScale.x}");
        }
        
        effect.transform.localScale = effectScale;
    }
}

