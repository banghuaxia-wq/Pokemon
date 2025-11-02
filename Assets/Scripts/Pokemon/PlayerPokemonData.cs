using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家宝可梦数据 - 存储在存档中的运行时数据
/// </summary>
[System.Serializable]
public class PlayerPokemonData
{
	[Header("基础引用")]
	[Tooltip("宝可梦种族数据")]
	public PokemonData pokemonData;
	
	[Header("个体数据")]
	[Tooltip("宝可梦昵称（可选）")]
	public string nickname = "";
	
	[Tooltip("当前等级")]
	public int level = 5;
	
	[Tooltip("当前经验值")]
	public int experience = 0;
	
	[Tooltip("当前HP")]
	public int currentHP;
	
	[Header("技能数据")]
	[Tooltip("已学会的技能（最多3个）")]
	public List<SkillData> learnedSkills = new List<SkillData>();
	
	[Tooltip("各技能的当前PP值")]
	public List<int> skillCurrentPP = new List<int>();
	
	[Header("状态")]
	[Tooltip("是否在队伍中")]
	public bool isInParty = true;
	
	/// <summary>
	/// 构造函数 - 捕获野生宝可梦时使用
	/// </summary>
	public PlayerPokemonData(PokemonData data, int catchLevel = 5)
	{
		pokemonData = data;
		level = catchLevel;
		currentHP = data.baseHP;
		
		// 复制技能列表
		learnedSkills = new List<SkillData>(data.skills);
		
		// 初始化PP值
		skillCurrentPP = new List<int>();
		foreach (var skill in learnedSkills)
		{
			skillCurrentPP.Add(skill != null ? skill.maxPP : 0);
		}
	}
	
	/// <summary>
	/// 获取显示名称（昵称优先，否则种族名）
	/// </summary>
	public string GetDisplayName()
	{
		return string.IsNullOrEmpty(nickname) ? pokemonData.displayName : nickname;
	}
	
	/// <summary>
	/// 恢复所有PP
	/// </summary>
	public void RestoreAllPP()
	{
		for (int i = 0; i < skillCurrentPP.Count; i++)
		{
			if (i < learnedSkills.Count && learnedSkills[i] != null)
			{
				skillCurrentPP[i] = learnedSkills[i].maxPP;
			}
		}
	}
	
	/// <summary>
	/// 完全恢复（HP和PP）
	/// </summary>
	public void FullRestore()
	{
		currentHP = pokemonData.baseHP;
		RestoreAllPP();
	}
	
	/// <summary>
	/// 学习新技能
	/// </summary>
	public bool LearnSkill(SkillData newSkill)
	{
		if (learnedSkills.Count >= 3)
		{
			Debug.LogWarning("技能已满，无法学习新技能！");
			return false;
		}
		
		learnedSkills.Add(newSkill);
		skillCurrentPP.Add(newSkill.maxPP);
		return true;
	}
	
	/// <summary>
	/// 替换技能
	/// </summary>
	public void ReplaceSkill(int index, SkillData newSkill)
	{
		if (index >= 0 && index < learnedSkills.Count)
		{
			learnedSkills[index] = newSkill;
			skillCurrentPP[index] = newSkill.maxPP;
		}
	}
}

