using UnityEngine;

/// <summary>
/// 技能数据基类 - 使用ScriptableObject存储技能的静态数据
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "Pokemon/Skill Data", order = 1)]
public class SkillData : ScriptableObject
{
	[Header("基础信息")]
	[Tooltip("技能名称")]
	public string skillName = "新技能";
	
	[Tooltip("技能描述")]
	[TextArea(2, 4)]
	public string description = "技能描述";

	[Header("技能类型")]
	[Tooltip("技能类型（决定动画类型）")]
	public SkillType skillType = SkillType.Physical;
	
	[Tooltip("目标类型")]
	public SkillTarget targetType = SkillTarget.Enemy;

	[Header("数值")]
	[Tooltip("技能威力（伤害值）")]
	public int power = 40;
	
	[Tooltip("PP值（技能使用次数）")]
	public int maxPP = 15;
	
	[Tooltip("命中率（0-100）")]
	[Range(0, 100)]
	public int accuracy = 100;
	
	[Header("特殊效果")]
	[Tooltip("是否有特殊效果")]
	public bool hasSpecialEffect = false;
	
	[Tooltip("特殊效果描述")]
	[TextArea(1, 2)]
	public string specialEffectDesc = "";
}
