using UnityEngine;

/// <summary>
/// 技能实例 - 运行时技能对象，追踪PP值等状态
/// </summary>
[System.Serializable]
public class Skill
{
	[Tooltip("技能数据引用")]
	public SkillData data;
	
	[Tooltip("当前PP值")]
	[SerializeField] private int _currentPP;
	
	/// <summary>
	/// 当前剩余PP值
	/// </summary>
	public int currentPP => _currentPP;
	
	/// <summary>
	/// 技能名称（从数据中获取）
	/// </summary>
	public string skillName => data != null ? data.skillName : "未知技能";
	
	/// <summary>
	/// 技能威力（从数据中获取）
	/// </summary>
	public int power => data != null ? data.power : 0;
	
	/// <summary>
	/// 技能类型（从数据中获取）
	/// </summary>
	public SkillType skillType => data != null ? data.skillType : SkillType.TackleAttack;
	
	/// <summary>
	/// 是否还有PP可用
	/// </summary>
	public bool HasPP => _currentPP > 0;
	
	/// <summary>
	/// 构造函数 - 初始化技能
	/// </summary>
	public Skill(SkillData skillData)
	{
		data = skillData;
		_currentPP = skillData != null ? skillData.maxPP : 0;
	}
	
	/// <summary>
	/// 使用技能（消耗1点PP）
	/// </summary>
	/// <returns>是否成功使用</returns>
	public bool Use()
	{
		if (!HasPP)
		{
			Debug.LogWarning($"技能 {skillName} PP不足！");
			return false;
		}
		
		_currentPP--;
		return true;
	}
	
	/// <summary>
	/// 恢复PP
	/// </summary>
	/// <param name="amount">恢复量（默认恢复满）</param>
	public void RestorePP(int amount = -1)
	{
		if (data == null) return;
		
		if (amount < 0)
		{
			// 恢复满
			_currentPP = data.maxPP;
		}
		else
		{
			// 恢复指定量
			_currentPP = Mathf.Min(_currentPP + amount, data.maxPP);
		}
	}
	
	/// <summary>
	/// 重置PP到最大值
	/// </summary>
	public void ResetPP()
	{
		if (data != null)
		{
			_currentPP = data.maxPP;
		}
	}
	
	/// <summary>
	/// 设置当前PP值（用于从存档恢复）
	/// </summary>
	public void SetCurrentPP(int pp)
	{
		if (data != null)
		{
			_currentPP = Mathf.Clamp(pp, 0, data.maxPP);
		}
	}
}

