using UnityEngine;

/// <summary>
/// 技能类型枚举 - 用于区分技能的动画类型
/// </summary>
public enum SkillType
{
    TackleAttack,   // 撞击攻击（宝可梦本体抖动）
    WhipAttack,     // 鞭打攻击（生成鞭子特效）
    DefenseStance,  // 防御姿态（生成护盾特效，提升防御力）
    Heal,           // 治疗技能（生成治疗特效，恢复HP）
    Taunt           // 嘲讽技能（强制敌人攻击自己，多人战斗用）
}

/// <summary>
/// 技能目标类型
/// </summary>
public enum SkillTarget
{
    Enemy,          // 敌方单体
    Self,           // 自己
    AllEnemies,     // 敌方全体
    AllAllies       // 己方全体
}

