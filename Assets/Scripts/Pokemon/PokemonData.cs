using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PokemonData", menuName = "Pokemon/Create Pokemon Data", order = 0)]
public class PokemonData : ScriptableObject
{
	[Header("基础信息")]
	public string displayName;
	
	[Tooltip("宝可梦图标")]
	public Sprite icon;
	
	[Header("视觉表现")]
	[Tooltip("战斗场景中使用的宝可梦 Prefab（包含模型、动画等）")]
	public GameObject battlePrefab;

	[Header("基础数值")]
	public int baseHP = 100;
	public int baseAttack = 10;
	public int baseDefense = 5;

	[Header("技能列表")]
	public List<SkillData> skills = new List<SkillData>();
}
