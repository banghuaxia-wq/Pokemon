using UnityEngine;

/// <summary>
/// 技能动画配置 - 定义每种技能类型的动画表现
/// </summary>
[CreateAssetMenu(fileName = "SkillAnimationConfig", menuName = "Battle/Skill Animation Config")]
public class SkillAnimationConfig : ScriptableObject
{
    [Header("鞭打攻击动画")]
    [Tooltip("鞭打攻击的特效 Prefab")]
    public GameObject whipAttackPrefab;
    
    [Header("防御姿态动画")]
    [Tooltip("防御姿态的特效 Prefab")]
    public GameObject shieldPrefab;
    
    [Header("治疗动画")]
    [Tooltip("治疗技能的特效 Prefab")]
    public GameObject healPrefab;
    
    [Header("撞击攻击设置")]
    [Tooltip("撞击攻击的抖动幅度")]
    public float tackleShakeAmount = 0.2f;
    
    [Tooltip("撞击攻击的抖动时长")]
    public float tackleDuration = 0.5f;
    
    [Header("特效持续时间")]
    [Tooltip("鞭打特效持续时间（秒）")]
    public float whipEffectDuration = 1.0f;
    
    [Tooltip("护盾特效持续时间（秒）")]
    public float shieldEffectDuration = 1.5f;
    
    [Tooltip("治疗特效持续时间（秒）")]
    public float healEffectDuration = 1.2f;
}

