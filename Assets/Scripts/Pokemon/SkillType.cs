using UnityEngine;

/// <summary>
/// 技能类型枚举 - 用于区分技能的动画类型
/// </summary>
public enum SkillType
{
    Physical,       // 物理攻击（近战、撞击等）
    Special,        // 特殊攻击（远程、魔法等）
    Status,         // 状态技能（buff、debuff等）
    Heal            // 治疗技能
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

